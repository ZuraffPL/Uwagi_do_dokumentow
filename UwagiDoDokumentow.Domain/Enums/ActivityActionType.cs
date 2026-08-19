namespace UwagiDoDokumentow.Domain.Enums;

/// <summary>
/// Typ akcji rejestrowanej w logu aktywności biznesowej (activity_log).
/// </summary>
public enum ActivityActionType
{
    Login,
    LoginFailed,
    Logout,
    Create,
    Update,
    Delete,
    Print,
    Export,
    Import
}
