using UwagiDoDokumentow.Application.DTO;

namespace UwagiDoDokumentow.Application.Interfaces;

/// <summary>
/// Trzyma dane aktualnie zalogowanego użytkownika w ramach sesji aplikacji.
/// Używany do stemplowania created_by/updated_by, włączania/wyłączania komend
/// w ViewModelach wg uprawnień oraz zapisu do activity_log.
/// </summary>
public interface ICurrentUserService
{
    UserDto? Current { get; }
    bool IsLoggedIn { get; }
    void SignIn(UserDto user);
    void SignOut();
}
