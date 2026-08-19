using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UwagiDoDokumentow.Domain.Entities;

namespace UwagiDoDokumentow.Infrastructure.Persistence.Configurations;

public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.ToTable("document_types");

        builder.HasKey(x => x.Symbol);
        builder.Property(x => x.Symbol).HasColumnName("symbol");
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasData(SeedData());
    }

    private static IEnumerable<DocumentType> SeedData()
    {
        string[] symbols = { "FO", "FI", "PZ", "PI", "DZ", "EK", "RE", "RR", "SO", "WZ", "RO", "KZ", "IV", "MM", "M1", "M2", "UN", "KB", "KF" };
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return symbols.Select(s => new DocumentType { Symbol = s, IsActive = true, CreatedAt = createdAt });
    }
}
