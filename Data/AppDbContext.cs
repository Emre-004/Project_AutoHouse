using Microsoft.EntityFrameworkCore;
using Project_AutoHouse.Models;

namespace Project_AutoHouse.Data
{
    // Наследяваме DbContext, което е "магията" на Microsoft
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Казваме му: "Направи ми SQL таблица за Колите и една за Потребителите!"
        public DbSet<CarAd> Cars { get; set; }
     
        public DbSet<User> Users { get; set; }

        public DbSet<Favorite> Favorites { get; set; }
    }
}