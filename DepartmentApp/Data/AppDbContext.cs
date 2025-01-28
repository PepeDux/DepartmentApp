using Microsoft.EntityFrameworkCore;

namespace DepartmentApp.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Department> Departments { get; set; } // Таблица подразделений

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка связи между родительским и дочерними подразделениями
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Parent) // У подразделения есть один родитель
                .WithMany(d => d.Children) // У родителя может быть много дочерних подразделений
                .HasForeignKey(d => d.ParentId) // Внешний ключ для связи
                .OnDelete(DeleteBehavior.Restrict); // Запрещаем каскадное удаление
        }
    }
}