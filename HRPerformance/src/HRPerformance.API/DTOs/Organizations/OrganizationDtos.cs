using HRPerformance.Enums;
namespace HRPerformance.DTOs.Organizations;
public record OrganizationDto(Guid Id, string Name, string Code, string? LogoPath, bool IsActive);
public record OrganizationUnitDto(Guid Id, Guid OrganizationId, Guid? ParentId, string Name, string Code, OrganizationUnitType UnitType, int Level, bool IsActive, IList<OrganizationUnitDto>? Children);
public record CreateOrganizationRequest(string Name, string Code, string? Address, string? Phone, string? Email);
public record CreateOrganizationUnitRequest(Guid OrganizationId, Guid? ParentId, string Name, string Code, OrganizationUnitType UnitType);
