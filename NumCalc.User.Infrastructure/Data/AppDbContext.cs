using Microsoft.EntityFrameworkCore;
using NumCalc.User.Domain.Entities;

namespace NumCalc.User.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<CalculationHistory> CalculationHistoryRecords { get; set; }
    public DbSet<SavedInput> SavedInputs { get; set; }
    public DbSet<SavedPdfExport> SavedPdfExports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}