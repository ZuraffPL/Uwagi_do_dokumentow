using UwagiDoDokumentow.Application.DTO;
using UwagiDoDokumentow.Application.Interfaces;

namespace UwagiDoDokumentow.App.Services;

/// <summary>
/// Trzyma dane zalogowanego użytkownika w pamięci na czas trwania sesji aplikacji (singleton).
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    public UserDto? Current { get; private set; }
    public bool IsLoggedIn => Current is not null;

    public void SignIn(UserDto user) => Current = user;

    public void SignOut() => Current = null;
}
