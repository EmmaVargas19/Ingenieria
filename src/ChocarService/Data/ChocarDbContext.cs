using ChocarService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChocarService.Data;

public class ChocarDbContext : DbContext
{
    public ChocarDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Auction> Auctions { get; set; }

    public DbSet<Bid> Bids { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
