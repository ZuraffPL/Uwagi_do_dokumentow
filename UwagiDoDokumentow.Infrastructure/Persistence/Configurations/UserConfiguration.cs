using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UwagiDoDokumentow.Domain.Entities;

namespace UwagiDoDokumentow.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Username).HasColumnName("username").IsRequired();
        builder.Property(x => x.DisplayName).HasColumnName("display_name").IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(x => x.IsAdmin).HasColumnName("is_admin").HasDefaultValue(false);
        builder.Property(x => x.CanAdd).HasColumnName("can_add").HasDefaultValue(true);
        builder.Property(x => x.CanEdit).HasColumnName("can_edit").HasDefaultValue(false);
        builder.Property(x => x.CanDelete).HasColumnName("can_delete").HasDefaultValue(false);
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.LastLoginAt).HasColumnName("last_login_at");

        builder.HasIndex(x => x.Username).IsUnique();
    }
}
