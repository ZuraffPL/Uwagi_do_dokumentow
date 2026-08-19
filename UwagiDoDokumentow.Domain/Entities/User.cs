namespace UwagiDoDokumentow.Domain.Entities;

/// <summary>
/// Użytkownik aplikacji wraz z uprawnieniami. Konto nigdy nie jest fizycznie usuwane,
/// tylko dezaktywowane (IsActive = false), aby nie urwać historii autorstwa wpisów.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool CanAdd { get; set; } = true;
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
