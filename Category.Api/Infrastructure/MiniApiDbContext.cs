using Category.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Category.Api.Infrastructure
{
    public class MiniApiDbContext : DbContext
    {
        public MiniApiDbContext(DbContextOptions<MiniApiDbContext> options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
    }
}
