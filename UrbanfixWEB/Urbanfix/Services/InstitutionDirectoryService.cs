using Microsoft.AspNetCore.Mvc.Rendering;
using Urbanfix.Models;

namespace Urbanfix.Services;

public class InstitutionDirectoryService
{
    private static readonly object SyncRoot = new();
    private static readonly List<InstitutionApplication> Applications = [];

    private static readonly List<ServiceZone> Zones =
    [
        new()
        {
            Code = "CENTER",
            Name = "Zona centro",
            MinLatitude = 14.58m,
            MaxLatitude = 14.68m,
            MinLongitude = -90.56m,
            MaxLongitude = -90.46m
        },
        new()
        {
            Code = "NORTH",
            Name = "Corredor norte",
            MinLatitude = 14.68m,
            MaxLatitude = 14.90m,
            MinLongitude = -90.62m,
            MaxLongitude = -90.42m
        },
        new()
        {
            Code = "SOUTH",
            Name = "Corredor sur",
            MinLatitude = 14.35m,
            MaxLatitude = 14.58m,
            MinLongitude = -90.62m,
            MaxLongitude = -90.42m
        },
        new()
        {
            Code = "EAST",
            Name = "Zona este",
            MinLatitude = 14.50m,
            MaxLatitude = 14.72m,
            MinLongitude = -90.46m,
            MaxLongitude = -90.25m
        },
        new()
        {
            Code = "WEST",
            Name = "Zona oeste",
            MinLatitude = 14.50m,
            MaxLatitude = 14.72m,
            MinLongitude = -90.78m,
            MaxLongitude = -90.56m
        }
    ];

    public IReadOnlyList<ServiceZone> GetZones() => Zones;

    public IReadOnlyList<SelectListItem> GetZoneOptions(string? selectedCode = null)
    {
        return Zones
            .Select(zone => new SelectListItem(
                zone.Name,
                zone.Code,
                string.Equals(zone.Code, selectedCode, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public ServiceZone? GetZone(string? zoneCode)
    {
        return Zones.FirstOrDefault(
            zone => string.Equals(zone.Code, zoneCode, StringComparison.OrdinalIgnoreCase));
    }

    public string ResolveZoneCode(decimal latitude, decimal longitude)
    {
        var matchingZone = Zones.FirstOrDefault(zone =>
            latitude >= zone.MinLatitude
            && latitude <= zone.MaxLatitude
            && longitude >= zone.MinLongitude
            && longitude <= zone.MaxLongitude);

        return matchingZone?.Code ?? "CENTER";
    }

    public InstitutionApplication? GetApplicationForUser(string profileId)
    {
        lock (SyncRoot)
        {
            return Applications
                .Where(application => application.ProfileId == profileId)
                .OrderByDescending(application => application.CreatedAt)
                .FirstOrDefault();
        }
    }

    public InstitutionApplication? GetVerifiedApplicationForUser(string profileId)
    {
        lock (SyncRoot)
        {
            return Applications.FirstOrDefault(application =>
                application.ProfileId == profileId
                && application.Status == InstitutionStatus.Verified);
        }
    }

    public IReadOnlyList<InstitutionApplication> GetApplications()
    {
        lock (SyncRoot)
        {
            return Applications
                .OrderBy(application => application.Status)
                .ThenByDescending(application => application.CreatedAt)
                .ToList();
        }
    }

    public (bool Success, string? Error) SubmitApplication(
        string profileId,
        string contactEmail,
        InstitutionRegistrationViewModel model,
        string? proofDocumentFileName)
    {
        var normalizedDomain = NormalizeDomain(model.OfficialDomain);
        var officialEmailDomain = model.OfficialEmail.Split('@').LastOrDefault()?.Trim().ToLowerInvariant();

        if (!string.Equals(officialEmailDomain, normalizedDomain, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "El correo oficial debe pertenecer al dominio institucional indicado.");
        }

        if (model.CoverageCategories.Count == 0)
        {
            return (false, "Selecciona al menos una categoria que cubre la institucion.");
        }

        if (GetZone(model.ServiceZoneCode) is null)
        {
            return (false, "Selecciona una zona de servicio valida.");
        }

        lock (SyncRoot)
        {
            var existing = Applications.FirstOrDefault(application => application.ProfileId == profileId);
            if (existing is not null)
            {
                Applications.Remove(existing);
            }

            Applications.Add(new InstitutionApplication
            {
                ProfileId = profileId,
                ContactEmail = contactEmail,
                InstitutionName = model.InstitutionName.Trim(),
                OfficialEmail = model.OfficialEmail.Trim(),
                OfficialDomain = normalizedDomain,
                Department = model.Department?.Trim(),
                ServiceZoneCode = model.ServiceZoneCode,
                CoverageCategories = model.CoverageCategories.Distinct().ToList(),
                ProofDocumentFileName = proofDocumentFileName
            });
        }

        return (true, null);
    }

    public bool Approve(Guid applicationId, string? reviewNote = null)
    {
        return UpdateStatus(applicationId, InstitutionStatus.Verified, reviewNote);
    }

    public bool Reject(Guid applicationId, string? reviewNote = null)
    {
        return UpdateStatus(applicationId, InstitutionStatus.Rejected, reviewNote);
    }

    private static bool UpdateStatus(Guid applicationId, InstitutionStatus status, string? reviewNote)
    {
        lock (SyncRoot)
        {
            var application = Applications.FirstOrDefault(item => item.Id == applicationId);
            if (application is null)
            {
                return false;
            }

            application.Status = status;
            application.ReviewNote = reviewNote;
            application.ReviewedAt = DateTime.UtcNow;
            return true;
        }
    }

    private static string NormalizeDomain(string domain)
    {
        return domain
            .Trim()
            .TrimStart('@')
            .ToLowerInvariant();
    }
}
