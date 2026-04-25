using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NumCalc.User.Domain.Entities;

namespace NumCalc.User.Infrastructure.Data.Configurations;

public class SavedInputConfiguration : IEntityTypeConfiguration<SavedInput>
{
    public void Configure(EntityTypeBuilder<SavedInput> builder)
    {
        builder.ToTable("SavedInputs");

        builder.HasKey(input => input.Id);
        builder.Property(input => input.Id)
            .ValueGeneratedOnAdd();

        builder.HasOne(input => input.User)
            .WithMany(user => user.SavedInputs)
            .HasForeignKey(input => input.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(input => input.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(input => input.Type)
            .IsRequired();
        
        builder.Property(input => input.InputsJson)
            .IsRequired();
            
        builder.Property(record => record.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
    }
}