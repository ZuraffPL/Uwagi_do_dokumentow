using Microsoft.EntityFrameworkCore;
using UwagiDoDokumentow.Domain.Entities;
using UwagiDoDokumentow.Infrastructure.Persistence.Configurations;

namespace UwagiDoDokumentow.Infrastructure.Persistence;

/// <summary>
/// Jedyny punkt dostępu do bazy SQLite. Baza pracuje w trybie WAL (Write-Ahead Logging),
/// co jest wymagane, jeśli plik bazy leży na współdzielonym zasobie sieciowym.
/// </summary>
public class NotesDbContext : DbContext
{
    public NotesDbContext(DbContextOptions<NotesDbContext> options) : base(options)
    {
    }

    public DbSet<DocumentNote> DocumentNotes => Set<DocumentNote>();
    public DbSet<NoteAttachment> NoteAttachments => Set<NoteAttachment>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ActivityLogEntry> ActivityLog => Set<ActivityLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DocumentNoteConfiguration());
        modelBuilder.ApplyConfiguration(new NoteAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ActivityLogConfiguration());
    }
}
