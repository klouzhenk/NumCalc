using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NumCalc.User.Domain.Entities;

namespace NumCalc.User.Infrastructure.Data.Configurations;

public class SavedPdfExportConfiguration : IEntityTypeConfiguration<SavedPdfExport>
{
    public void Configure(EntityTypeBuilder<SavedPdfExport> builder)
    {
        builder.ToTable("SavedPdfExports");

        builder.HasKey(file => file.Id);
        builder.Property(file => file.Id)
            .ValueGeneratedOnAdd();

        builder.HasOne(file => file.User)
            .WithMany(user => user.SavedPdfs)
            .HasForeignKey(file => file.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(file => file.FileName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(file => file.Type)
            .IsRequired();
        
        builder.Property(file => file.MethodName)
            .IsRequired();
        
        builder.Property(file => file.FileData)
            .IsRequired()
            .HasColumnType("varbinary(max)");
            
        builder.Property(record => record.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
    }
}