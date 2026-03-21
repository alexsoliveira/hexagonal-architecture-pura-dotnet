namespace HexagonalLab.Infrastructure.Data.EntityConfigurations;

using HexagonalLab.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Entity Configuration: Mapping de Item no banco.
/// Fluent API configuration para Item table e properties.
/// </summary>
public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        // Tabela
        builder.ToTable("Items");

        // Primary Key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.ProcessedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(x => x.Status).HasDatabaseName("IX_Items_Status");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_Items_CreatedAt");
    }
}
