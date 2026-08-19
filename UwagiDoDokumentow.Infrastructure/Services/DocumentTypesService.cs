using Microsoft.EntityFrameworkCore;
using UwagiDoDokumentow.Application.Interfaces;
using UwagiDoDokumentow.Domain.Entities;
using UwagiDoDokumentow.Infrastructure.Persistence;

namespace UwagiDoDokumentow.Infrastructure.Services;

/// <summary>
/// Zarządzanie słownikiem symboli dokumentów. Dodawanie tylko przez panel administracyjny.
/// </summary>
public class DocumentTypesService : IDocumentTypesService
{
    private readonly NotesDbContext _dbContext;

    public DocumentTypesService(NotesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<DocumentType>> GetAllAsync(bool onlyActive = false, CancellationToken ct = default)
    {
        var query = _dbContext.DocumentTypes.AsQueryable();
        if (onlyActive)
        {
            query = query.Where(t => t.IsActive);
        }
        return await query.OrderBy(t => t.Symbol).ToListAsync(ct);
    }

    public async Task AddAsync(string symbol, string? description, CancellationToken ct = default)
    {
        if (await _dbContext.DocumentTypes.AnyAsync(t => t.Symbol == symbol, ct))
        {
            throw new InvalidOperationException($"Symbol „{symbol}” już istnieje.");
        }

        _dbContext.DocumentTypes.Add(new DocumentType
        {
            Symbol = symbol,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.Now
        });

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task SetActiveAsync(string symbol, bool isActive, CancellationToken ct = default)
    {
        var type = await _dbContext.DocumentTypes.FirstOrDefaultAsync(t => t.Symbol == symbol, ct)
            ?? throw new InvalidOperationException($"Nie znaleziono symbolu „{symbol}”.");

        type.IsActive = isActive;
        await _dbContext.SaveChangesAsync(ct);
    }
}
