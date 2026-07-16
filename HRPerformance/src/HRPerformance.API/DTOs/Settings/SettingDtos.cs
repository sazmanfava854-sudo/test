namespace HRPerformance.DTOs.Settings;
public record SettingDto(Guid Id, string Key, string Value, string Category, string? Description, string DataType);
public record UpdateSettingRequest(string Key, string Value);
public record HolidayDto(Guid Id, string Title, DateTime HolidayDate, bool IsRecurring, string? Description);
