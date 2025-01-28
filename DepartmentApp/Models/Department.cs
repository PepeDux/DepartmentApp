namespace DepartmentApp.Models
{
    public class Department
    {
        public int Id { get; set; } // Уникальный идентификатор подразделения
        public string Name { get; set; } // Название подразделения
        public int? ParentId { get; set; } // Идентификатор родительского подразделения (null для корневых)
        public Department Parent { get; set; } // Навигационное свойство для родителя
        public ICollection<Department> Children { get; set; } = new List<Department>(); // Дочерние подразделения
        public int OrderNumber { get; set; } // Порядковый номер подразделения
    }
}