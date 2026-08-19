using Microsoft.EntityFrameworkCore;
using UwagiDoDokumentow.Application.DTO;
using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Domain.Entities;
using UwagiDoDokumentow.Domain.Enums;
using UwagiDoDokumentow.Infrastructure.Persistence;
using UwagiDoDokumentow.Infrastructure.Security;

namespace UwagiDoDokumentow.Infrastructure.Services;

/// <summary>
/// Zarządzanie użytkownikami i uwierzytelnianie. Konta nie są fizycznie usuwane z bazy.
/// </summary>
public class UserService : IUserService
{
    private readonly NotesDbContext _dbContext;
    private readonly IActivityLogService _activityLog;

    public UserService(NotesDbContext dbContext, IActivityLogService activityLog)
    {
        _dbContext = dbContext;
        _activityLog = activityLog;
    }

    public async Task<UserDto?> AuthenticateAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsActive, ct);
        if (user is null || !PasswordHasher.Verify(password, user.PasswordHash))
        {
            if (user is not null)
            {
                await _activityLog.LogAsync(user.Id, ActivityActionType.LoginFailed, nameof(User), user.Id, ct: ct);
            }
            return null;
        }

        user.LastLoginAt = DateTime.Now;
        await _dbContext.SaveChangesAsync(ct);
        await _activityLog.LogAsync(user.Id, ActivityActionType.Login, nameof(User), user.Id, ct: ct);

        return ToDto(user);
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken ct = default) =>
        await _dbContext.Users.OrderBy(u => u.Username).Select(u => ToDto(u)).ToListAsync(ct);

    public async Task<UserDto> CreateAsync(string username, string displayName, string password, bool isAdmin, bool canAdd, bool canEdit, bool canDelete, CancellationToken ct = default)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Username == username, ct))
        {
            throw new InvalidOperationException($"Użytkownik „{username}” już istnieje.");
        }

        var user = new User
        {
            Username = username,
            DisplayName = displayName,
            PasswordHash = PasswordHasher.Hash(password),
            IsAdmin = isAdmin,
            CanAdd = canAdd,
            CanEdit = canEdit,
            CanDelete = canDelete,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(ct);

        return ToDto(user);
    }

    public async Task UpdatePermissionsAsync(int userId, bool isAdmin, bool canAdd, bool canEdit, bool canDelete, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidOperationException($"Nie znaleziono użytkownika o id {userId}.");

        user.IsAdmin = isAdmin;
        user.CanAdd = canAdd;
        user.CanEdit = canEdit;
        user.CanDelete = canDelete;

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task SetActiveAsync(int userId, bool isActive, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidOperationException($"Nie znaleziono użytkownika o id {userId}.");

        user.IsActive = isActive;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task ResetPasswordAsync(int userId, string newPassword, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidOperationException($"Nie znaleziono użytkownika o id {userId}.");

        user.PasswordHash = PasswordHasher.Hash(newPassword);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int userId, int performedByUserId, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidOperationException($"Nie znaleziono użytkownika o id {userId}.");

        if (user.IsActive)
        {
            throw new InvalidOperationException("Nie można usunąć aktywnego konta. Najpierw je dezaktywuj.");
        }

        var hasRelatedRecords =
            await _dbContext.DocumentNotes.AnyAsync(n => n.CreatedByUserId == userId || n.UpdatedByUserId == userId, ct) ||
            await _dbContext.NoteAttachments.AnyAsync(a => a.UploadedByUserId == userId, ct) ||
            await _dbContext.ActivityLog.AnyAsync(l => l.UserId == userId, ct);

        if (hasRelatedRecords)
        {
            throw new InvalidOperationException(
                "Nie można usunąć tego konta, ponieważ posiada powiązane wpisy (utworzone uwagi, załączniki lub log aktywności). " +
                "Pozostaw je konto zdezaktywowane zamiast usuwać.");
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(ct);

        await _activityLog.LogAsync(performedByUserId, ActivityActionType.Delete, nameof(User), userId, details: $"Usunięto konto „{user.Username}”.", ct: ct);
    }

    private static UserDto ToDto(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        DisplayName = u.DisplayName,
        IsAdmin = u.IsAdmin,
        CanAdd = u.CanAdd,
        CanEdit = u.CanEdit,
        CanDelete = u.CanDelete,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt,
        LastLoginAt = u.LastLoginAt
    };
}
