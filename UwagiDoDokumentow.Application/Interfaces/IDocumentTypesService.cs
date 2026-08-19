using UwagiDoDokumentow.Domain.Entities;

namespace UwagiDoDokumentow.Application.Interfaces;

/// <summary>
/// Zarządzanie słownikiem symboli dokumentów (document_types). Dodawanie nowych symboli
/// tylko przez ekran administracyjny.
/// </summary>
public interface IDocumentTypesService
{
    Task<List<DocumentType>> GetAllAsync(bool onlyActive = false, CancellationToken ct = default);
    Task AddAsync(string symbol, string? description, CancellationToken ct = default);
    Task SetActiveAsync(string symbol, bool isActive, CancellationToken ct = default);
}
