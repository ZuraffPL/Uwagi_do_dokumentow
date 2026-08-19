namespace UwagiDoDokumentow.Domain.ValueObjects;

/// <summary>
/// Pełny numer dokumentu, złożony z symbolu (np. FO) i numeru (np. 123/2026).
/// </summary>
public readonly record struct DocumentNumber(string Symbol, string Number)
{
    public override string ToString() => $"{Symbol} {Number}";
}
