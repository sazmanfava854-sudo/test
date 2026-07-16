namespace HRPerformance.DTOs.Auth;
public record LoginRequest(string UserName, string Password);
public record RefreshTokenRequest(string AccessToken, string RefreshToken);
public record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserDto User);
public record UserDto(Guid Id, string UserName, string Email, string FirstName, string LastName, string FullName, Guid? OrganizationId, Guid? EmployeeId, IList<string> Roles);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
