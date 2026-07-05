using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanFix.API.Contracts.Requests;
using UrbanFix.API.Contracts.Responses;
using UrbanFix.API.Data;
using UrbanFix.API.Entities;
using UrbanFix.API.Helpers;
using UrbanFix.API.Services;

namespace UrbanFix.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private const int MaxFilesPerReport = 5;

    private readonly ReportContext _context;
    private readonly IImageUploadService _imageUpload;
    private readonly ISupabaseStorageService _storage;

    public ReportsController(
        ReportContext context,
        IImageUploadService imageUpload,
        ISupabaseStorageService storage)
    {
        _context = context;
        _imageUpload = imageUpload;
        _storage = storage;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetReports(CancellationToken cancellationToken)
    {
        var reports = await _context.Reports
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReportDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                Category = r.Category,
                Priority = r.Priority,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                CivilianUserId = r.CivilianUserId,
                InstitutionId = r.InstitutionId,
                Status = r.Status,
                AiCategory = r.AiCategory,
                AiPriority = r.AiPriority,
                AiConfidence = r.AiConfidence,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Images = r.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new ReportImageDto
                    {
                        Id = i.Id,
                        StoragePath = i.StoragePath,
                        ContentType = i.ContentType,
                        FileSizeBytes = i.FileSizeBytes,
                        SortOrder = i.SortOrder
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        foreach (var report in reports)
        {
            SetImageUrls(report);
        }

        return Ok(reports);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReportDto>> GetReport(Guid id, CancellationToken cancellationToken)
    {
        var report = await _context.Reports
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ReportDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                Category = r.Category,
                Priority = r.Priority,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                CivilianUserId = r.CivilianUserId,
                InstitutionId = r.InstitutionId,
                Status = r.Status,
                AiCategory = r.AiCategory,
                AiPriority = r.AiPriority,
                AiConfidence = r.AiConfidence,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Images = r.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new ReportImageDto
                    {
                        Id = i.Id,
                        StoragePath = i.StoragePath,
                        ContentType = i.ContentType,
                        FileSizeBytes = i.FileSizeBytes,
                        SortOrder = i.SortOrder
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (report is null)
        {
            return NotFound();
        }

        SetImageUrls(report);
        return Ok(report);
    }

    [HttpGet("{reportId:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> GetReportImage(
        Guid reportId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var storagePath = await _context.ReportImages
            .AsNoTracking()
            .Where(i => i.Id == imageId && i.ReportId == reportId)
            .Select(i => i.StoragePath)
            .FirstOrDefaultAsync(cancellationToken);

        if (storagePath is null)
        {
            return NotFound();
        }

        return Redirect(_storage.GetPublicUrl(storagePath));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ReportDto>> UpdateReportStatus(
        Guid id,
        [FromBody] UpdateReportStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.Status.HasValue || !Enum.IsDefined(typeof(ReportStatus), request.Status.Value))
        {
            return BadRequest("El estado indicado no es válido.");
        }

        var report = await _context.Reports
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (report is null)
        {
            return NotFound();
        }

        report.Status = request.Status.Value;
        report.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(MapToDto(report));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteReport(Guid id, CancellationToken cancellationToken)
    {
        var report = await _context.Reports
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (report is null)
        {
            return NotFound();
        }

        var storagePaths = report.Images
            .Select(i => i.StoragePath)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .ToList();

        try
        {
            if (storagePaths.Count > 0)
            {
                await _storage.DeleteAsync(storagePaths, cancellationToken);
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest($"No se pudo eliminar el reporte: {ex.Message}");
        }
    }

    [Authorize]
    [HttpPost]
    [RequestSizeLimit(25 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ReportDto>> CreateReport(
        [FromForm] CreateReportRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var profile = await _context.Profiles
            .AsNoTracking()
            .Where(p => p.Id == userId.Value)
            .Select(p => new { p.Role })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return BadRequest(
                "Tu cuenta no tiene perfil en la base de datos. Registra el usuario en Supabase Auth y verifica el trigger handle_new_user.");
        }

        var reportId = Guid.NewGuid();
        var savedPaths = new List<string>();

        try
        {
            var report = new Report
            {
                Id = reportId,
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Category = request.Category,
                Priority = request.Priority,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                CivilianUserId = userId.Value,
                Status = ReportStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            if (request.Images is { Count: > 0 })
            {
                short sortOrder = 0;

                foreach (var file in request.Images)
                {
                    var prepared = await _imageUpload.PrepareImageAsync(
                        reportId,
                        file,
                        sortOrder++,
                        cancellationToken);

                    savedPaths.Add(prepared.StoragePath);

                    report.Images.Add(new ReportImage
                    {
                        Id = prepared.Id,
                        ReportId = reportId,
                        StoragePath = prepared.StoragePath,
                        ContentType = prepared.ContentType,
                        FileSizeBytes = prepared.FileSizeBytes,
                        SortOrder = (short)(sortOrder - 1),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            _context.Reports.Add(report);
            await _context.SaveChangesAsync(cancellationToken);

            var response = MapToDto(report);
            return CreatedAtAction(nameof(GetReport), new { id = report.Id }, response);
        }
        catch (DbUpdateException ex)
        {
            await _storage.DeleteAsync(savedPaths, cancellationToken);
            return BadRequest($"No se pudo guardar el reporte en la base de datos: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            await _storage.DeleteAsync(savedPaths, cancellationToken);
            return BadRequest(ex.Message);
        }
        catch
        {
            await _storage.DeleteAsync(savedPaths, cancellationToken);
            throw;
        }
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var userId) ? userId : null;
    }

    private string? ValidateRequest(CreateReportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return "El título es obligatorio.";
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return "La descripción es obligatoria.";
        }

        if (request.Latitude is < -90 or > 90)
        {
            return "La latitud debe estar entre -90 y 90.";
        }

        if (request.Longitude is < -180 or > 180)
        {
            return "La longitud debe estar entre -180 y 180.";
        }

        if (request.Images is { Count: > 0 })
        {
            if (request.Images.Count > MaxFilesPerReport)
            {
                return $"Se permiten como máximo {MaxFilesPerReport} imágenes por reporte.";
            }

            if (request.Images.Any(file => file.Length == 0))
            {
                return "Una o más imágenes están vacías.";
            }
        }

        return null;
    }

    private ReportDto MapToDto(Report report)
    {
        var dto = new ReportDto
        {
            Id = report.Id,
            Title = report.Title,
            Description = report.Description,
            Category = report.Category,
            Priority = report.Priority,
            Latitude = report.Latitude,
            Longitude = report.Longitude,
            CivilianUserId = report.CivilianUserId,
            InstitutionId = report.InstitutionId,
            Status = report.Status,
            AiCategory = report.AiCategory,
            AiPriority = report.AiPriority,
            AiConfidence = report.AiConfidence,
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt,
            Images = report.Images
                .OrderBy(i => i.SortOrder)
                .Select(i => new ReportImageDto
                {
                    Id = i.Id,
                    StoragePath = i.StoragePath,
                    ContentType = i.ContentType,
                    FileSizeBytes = i.FileSizeBytes,
                    SortOrder = i.SortOrder
                })
                .ToList()
        };

        SetImageUrls(dto);
        return dto;
    }

    private void SetImageUrls(ReportDto report)
    {
        foreach (var image in report.Images)
        {
            image.Url = _storage.GetPublicUrl(image.StoragePath);
        }
    }
}
