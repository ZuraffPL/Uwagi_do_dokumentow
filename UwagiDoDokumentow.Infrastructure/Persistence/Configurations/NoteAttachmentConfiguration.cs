using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UwagiDoDokumentow.Domain.Entities;

namespace UwagiDoDokumentow.Infrastructure.Persistence.Configurations;

public class NoteAttachmentConfiguration : IEntityTypeConfiguration<NoteAttachment>
{
    public void Configure(EntityTypeBuilder<NoteAttachment> builder)
    {
        builder.ToTable("note_attachments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.NoteId).HasColumnName("note_id").IsRequired();
        builder.Property(x => x.OriginalFileName).HasColumnName("original_file_name").IsRequired();
        builder.Property(x => x.StoredFileName).HasColumnName("stored_file_name").IsRequired();
        builder.Property(x => x.RelativePath).HasColumnName("relative_path").IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("content_type");
        builder.Property(x => x.Extension).HasColumnName("extension").IsRequired();
        builder.Property(x => x.SizeBytes).HasColumnName("size_bytes").IsRequired();
        builder.Property(x => x.UploadedByUserId).HasColumnName("uploaded_by_user_id").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(x => x.UploadedByUser)
            .WithMany()
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.NoteId).HasDatabaseName("ix_note_attachments_note_id");
    }
}
