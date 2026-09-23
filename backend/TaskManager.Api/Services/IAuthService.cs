using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
}