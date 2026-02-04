using Microsoft.EntityFrameworkCore;
using WheelyGoodCars.Models;

namespace WheelyGoodCars.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options)
        : base(options) { }

    public DbSet<Car> Cars { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<CarTag> CarTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CarTag>()
            .HasKey(ct => new { ct.CarId, ct.TagId });
    }
}
