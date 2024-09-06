using Microsoft.EntityFrameworkCore;
using webapi.Models;

namespace webapi.Database
{
    public class ApplicationDbContext : DbContext   
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Register> Registers { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}
