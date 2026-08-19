using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UwagiDoDokumentow.Domain.Entities;

namespace UwagiDoDokumentow.Infrastructure.Persistence.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLogEntry>
{
    public void Configure(EntityTypeBuilder<ActivityLogEntry> builder)
    {
        builder.ToTable("activity_log");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.ActionType).HasColumnName("action_type").HasConversion<string>().IsRequired();
        builder.Property(x => x.EntityType).HasColumnName("entity_type");
        builder.Property(x => x.EntityId).HasColumnName("entity_id");
        builder.Property(x => x.Details).HasColumnName("details");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId).HasDatabaseName("ix_activity_log_user_id");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_activity_log_created_at");
    }
}
