using UwagiDoDokumentow.Application.DTO;

namespace UwagiDoDokumentow.Application.Interfaces;

/// <summary>
/// Zarządzanie użytkownikami i uwierzytelnianie. Konta nie są fizycznie usuwane,
/// tylko dezaktywowane (IsActive = false).
/// </summary>
public interface IUserService
{
    Task<UserDto?> AuthenticateAsync(string username, string password, CancellationToken ct = default);
    Task<List<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<UserDto> CreateAsync(string username, string displayName, string password, bool isAdmin, bool canAdd, bool canEdit, bool canDelete, CancellationToken ct = default);
    Task UpdatePermissionsAsync(int userId, bool isAdmin, bool canAdd, bool canEdit, bool canDelete, CancellationToken ct = default);
    Task SetActiveAsync(int userId, bool isActive, CancellationToken ct = default);
    Task ResetPasswordAsync(int userId, string newPassword, CancellationToken ct = default);

    /// <summary>
    /// Fizyczne usunięcie konta. Dozwolone tylko dla kont nieaktywnych (IsActive = false)
    /// i tylko wtedy, gdy użytkownik nie posiada żadnych powiązanych rekordów
    /// (uwag, załączników, wpisów w logu aktywności) — w przeciwnym razie zgłasza wyjątek.
    /// </summary>
    Task DeleteAsync(int userId, int performedByUserId, CancellationToken ct = default);
}
