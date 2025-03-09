using Microsoft.EntityFrameworkCore;

namespace Hotel_Management_System.Models
{
    public class HotelManagementDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        // Add this constructor
        public HotelManagementDbContext(DbContextOptions<HotelManagementDbContext> options)
            : base(options)
        {
        }
    }
}