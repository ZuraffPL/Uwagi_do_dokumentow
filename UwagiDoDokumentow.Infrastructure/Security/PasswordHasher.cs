namespace UwagiDoDokumentow.Infrastructure.Security;

/// <summary>
/// Hashowanie haseł użytkowników przy użyciu BCrypt (hash + sól w jednym stringu).
/// </summary>
public static class PasswordHasher
{
    public static string Hash(string plainPassword) => BCrypt.Net.BCrypt.HashPassword(plainPassword);

    public static bool Verify(string plainPassword, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);
}
