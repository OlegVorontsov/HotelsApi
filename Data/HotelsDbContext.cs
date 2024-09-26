using Microsoft.EntityFrameworkCore;

namespace HotelsApi.Data
{
  public class HotelsDbContext : DbContext
  {
    public HotelsDbContext(DbContextOptions<HotelsDbContext> options) : base(options){}
    public DbSet<Hotel> Hotels => Set<Hotel>();
  }
}