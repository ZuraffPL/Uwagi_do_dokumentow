using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UwagiDoDokumentow.Domain.Entities;

namespace UwagiDoDokumentow.Infrastructure.Persistence.Configurations;

public class DocumentNoteConfiguration : IEntityTypeConfiguration<DocumentNote>
{
    public void Configure(EntityTypeBuilder<DocumentNote> builder)
    {
        builder.ToTable("document_notes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.DocumentDate).HasColumnName("document_date").IsRequired();
        builder.Property(x => x.DocumentSymbol).HasColumnName("document_symbol").IsRequired();
        builder.Property(x => x.DocumentNumber).HasColumnName("document_number").IsRequired();
        builder.Property(x => x.OrderedBy).HasColumnName("ordered_by").IsRequired();
        builder.Property(x => x.Title).HasColumnName("title");
        builder.Property(x => x.Content).HasColumnName("content").IsRequired();
        builder.Property(x => x.Tags).HasColumnName("tags");
        builder.Property(x => x.IsArchived).HasColumnName("is_archived").HasDefaultValue(false);
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        builder.Property(x => x.UpdatedByUserId).HasColumnName("updated_by_user_id").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasOne(x => x.DocumentType)
            .WithMany()
            .HasForeignKey(x => x.DocumentSymbol)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.Note)
            .HasForeignKey(x => x.NoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.DocumentDate).HasDatabaseName("ix_document_notes_document_date");
        builder.HasIndex(x => new { x.DocumentSymbol, x.DocumentNumber }).HasDatabaseName("ix_document_notes_symbol_number");
        builder.HasIndex(x => x.UpdatedAt).HasDatabaseName("ix_document_notes_updated_at");
        builder.HasIndex(x => x.OrderedBy).HasDatabaseName("ix_document_notes_ordered_by");
    }
}
