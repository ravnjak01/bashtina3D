// Dtos/SiteDtos.cs
using Bastina3D.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bastina3D.Application.DTOs;

public record SiteListItemDto(Guid Id, string Name, string Slug, SiteCategory Category,
    string? Municipality, double Longitude, double Latitude);

public record ModelDto(string Url, double Scale, double HeadingDeg, double AltitudeOffsetM, long? FileSizeBytes);
public record MediaDto(string Url, string? Caption, string? Attribution, string? License);

public record SiteDetailDto(Guid Id, string Name, string? NameLocal, string Slug, string? Description,
    SiteCategory Category, string? Period, int? YearBuilt, string? Municipality, bool IsNationalMonument,
    double Longitude, double Latitude, double? ElevationM, string? WikidataId, string? HeroImageUrl,
    ModelDto? Model, IReadOnlyList<MediaDto> Media);

public record CreateSiteRequest(
    [Required, MaxLength(300)] string Name,
    string? NameLocal,
    string? Description,
    SiteCategory Category,
    string? Period,
    int? YearBuilt,
    string? Municipality,
    bool IsNationalMonument,
    [Range(-180, 180)] double Longitude,
    [Range(-90, 90)] double Latitude,
    string? WikidataId);

public record SiteQuery(string? Q, SiteCategory? Category, string? Bbox, int Page = 1, int PageSize = 50);