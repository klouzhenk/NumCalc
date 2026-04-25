using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NumCalc.User.Domain.Entities;

namespace NumCalc.User.Infrastructure.Data.Configurations;

public class CalculationHistoryConfiguration : IEntityTypeConfiguration<CalculationHistoryRecord>
{
    public void Configure(EntityTypeBuilder<CalculationHistoryRecord> builder)
    {
        builder.ToTable("CalculationHistoryRecords");
        
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Id)
            .ValueGeneratedOnAdd();

        builder.HasOne(record => record.User)
            .WithMany(user => user.History)
            .HasForeignKey(record => record.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(record => record.Type)
            .IsRequired();

        builder.Property(record => record.MethodName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(record => record.InputsJson)
            .IsRequired();

        builder.Property(record => record.ResultSummary)
            .IsRequired();
        
        builder.Property(record => record.ExecutionTimeMs)
            .IsRequired();

        builder.Property(record => record.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
    }
}