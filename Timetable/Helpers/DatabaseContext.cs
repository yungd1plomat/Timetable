using Microsoft.EntityFrameworkCore;
using Timetable.Models;

namespace Timetable.Helpers
{
    public class DatabaseContext : DbContext
    {
        /// <summary>
        /// Все группы
        /// </summary>
        public DbSet<Group> Groups { get; set; }

        /// <summary>
        /// Все занятия
        /// </summary>
        public DbSet<Lesson> Lessons { get; set; }

        /// <summary>
        /// Все пользователи
        /// </summary>
        public DbSet<BotUser> Users { get; set; }

        public DatabaseContext() : base()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLazyLoadingProxies() // Ленивая загрузка данных
                .UseMySql("Server=45.153.231.141;Port=3306;Database=timetable;Uid=admin;Pwd=MAvlus5DNG0mUnGC;", new MySqlServerVersion(new Version(8, 0, 27)), options => options.EnableRetryOnFailure(10));
        }
    }
}
