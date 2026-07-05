using Microsoft.EntityFrameworkCore;
using Urbanfix.Data;
using Urbanfix.Models;

namespace Urbanfix.Services;

public class PuntoService
{
    private readonly UrbanfixDbContext _context;

    public PuntoService(UrbanfixDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PuntoMapaDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        return await QueryValidos()
            .Select(p => new PuntoMapaDto
            {
                Id = p.Id,
                Latitud = p.Latitud,
                Longitud = p.Longitud,
                Titulo = p.Titulo,
                Descripcion = p.Descripcion
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PuntoMapaDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await QueryValidos()
            .Where(p => p.Id == id)
            .Select(p => new PuntoMapaDto
            {
                Id = p.Id,
                Latitud = p.Latitud,
                Longitud = p.Longitud,
                Titulo = p.Titulo,
                Descripcion = p.Descripcion
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PuntoMapaDto>> ObtenerPorBoundsAsync(
        double north,
        double south,
        double east,
        double west,
        CancellationToken cancellationToken = default)
    {
        return await QueryValidos()
            .Where(p => p.Latitud <= north
                && p.Latitud >= south
                && p.Longitud <= east
                && p.Longitud >= west)
            .Select(p => new PuntoMapaDto
            {
                Id = p.Id,
                Latitud = p.Latitud,
                Longitud = p.Longitud,
                Titulo = p.Titulo,
                Descripcion = p.Descripcion
            })
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Punto> QueryValidos()
    {
        return _context.Puntos
            .AsNoTracking()
            .Where(p => p.Latitud >= -90 && p.Latitud <= 90)
            .Where(p => p.Longitud >= -180 && p.Longitud <= 180);
    }
}
