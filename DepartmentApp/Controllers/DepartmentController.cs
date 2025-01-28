using Microsoft.AspNetCore.Mvc;
using DepartmentApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;

namespace DepartmentApp.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly AppDbContext _context;

        // Конструктор для внедрения зависимости контекста базы данных
        public DepartmentController(AppDbContext context)
        {
            _context = context;
        }

        // Вывод иерархического справочника подразделений
        public async Task<IActionResult> Index()
        {
            // Получаем все корневые подразделения и их дочерние элементы
            var departments = await _context.Departments
                .Include(d => d.Children) // Включаем дочерние подразделения
                .Where(d => d.ParentId == null) // Фильтруем корневые элементы
                .ToListAsync();

            return View(departments); // Передаем данные в представление
        }

        // Добавление нового подразделения
        [HttpPost]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid) // Проверка валидации данных
            {
                // Генерация порядкового номера для нового подразделения
                department.OrderNumber = await GenerateOrderNumberAsync(department.ParentId);
                _context.Add(department); // Добавляем подразделение в контекст
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных

                return RedirectToAction(nameof(Index)); // Перенаправляем на главную страницу
            }

            return View(department); // Если данные невалидны, возвращаем форму с ошибками
        }

        // Удаление подразделения
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments.FindAsync(id); // Ищем подразделение по ID
            if (department != null)
            {
                _context.Departments.Remove(department); // Удаляем подразделение
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
            }

            return RedirectToAction(nameof(Index)); // Перенаправляем на главную страницу
        }

        // Перемещение подразделения
        [HttpPost]
        public async Task<IActionResult> Move(int id, int? newParentId)
        {
            var department = await _context.Departments.FindAsync(id); // Ищем подразделение по ID
            if (department != null)
            {
                department.ParentId = newParentId; // Устанавливаем нового родителя
                department.OrderNumber = await GenerateOrderNumberAsync(newParentId); // Обновляем порядковый номер
                _context.Update(department); // Обновляем подразделение в контексте
                await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
            }

            return RedirectToAction(nameof(Index)); // Перенаправляем на главную страницу
        }

        // Экспорт справочника в XML
        public async Task<IActionResult> ExportToXml()
        {
            // Получаем все подразделения из базы данных
            var departments = await _context.Departments.ToListAsync();
            // Сериализуем данные в XML
            var xmlBytes = SerializeToXml(departments);

            // Возвращаем XML как файл для скачивания
            return File(xmlBytes, "application/xml", "departments.xml");
        }

        // Импорт справочника из XML
        [HttpPost]
        public async Task<IActionResult> ImportFromXml(IFormFile file)
        {
            if (file != null && file.Length > 0) // Проверяем, что файл был загружен
            {
                // Десериализуем XML в список подразделений
                var departments = DeserializeFromXml(file);
                // Добавляем подразделения в контекст
                _context.Departments.AddRange(departments);
                // Сохраняем изменения в базе данных
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index)); // Перенаправляем на главную страницу
        }

        // Вспомогательный метод для генерации порядкового номера
        private async Task<int> GenerateOrderNumberAsync(int? parentId)
        {
            // Находим последний порядковый номер среди дочерних подразделений
            var lastOrderNumber = await _context.Departments
                .Where(d => d.ParentId == parentId)
                .OrderByDescending(d => d.OrderNumber)
                .Select(d => d.OrderNumber)
                .FirstOrDefaultAsync();

            // Возвращаем следующий порядковый номер
            return lastOrderNumber + 1;
        }

        // Вспомогательный метод для сериализации в XML
        private byte[] SerializeToXml(List<Department> departments)
        {
            // Создаем сериализатор для списка подразделений
            var serializer = new XmlSerializer(typeof(List<Department>));
            using (var memoryStream = new MemoryStream()) // Используем MemoryStream для хранения XML
            {
                using (var writer = new StreamWriter(memoryStream, Encoding.UTF8)) // Записываем XML в поток
                {
                    serializer.Serialize(writer, departments); // Сериализуем данные
                }

                return memoryStream.ToArray(); // Возвращаем XML как массив байтов
            }
        }

        // Вспомогательный метод для десериализации из XML
        private List<Department> DeserializeFromXml(IFormFile file)
        {
            using (var stream = file.OpenReadStream()) // Открываем поток для чтения файла
            {
                // Создаем сериализатор для списка подразделений
                var serializer = new XmlSerializer(typeof(List<Department>));

                // Десериализуем XML в список подразделений
                return (List<Department>)serializer.Deserialize(stream);
            }
        }
    }
}