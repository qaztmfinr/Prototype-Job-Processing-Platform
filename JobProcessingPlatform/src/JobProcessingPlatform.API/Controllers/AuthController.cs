using Microsoft.AspNetCore.Mvc;
using JobProcessingPlatform.Application.Commands;
using JobProcessingPlatform.Application.Queries;
using JobProcessingPlatform.Application.Handlers;
using JobProcessingPlatform.Application.Services;
using JobProcessingPlatform.Domain.Interfaces;

namespace JobProcessingPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public AuthController(IUserRepository userRepository, IPasswordService passwordService, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        try
        {
            var existingUser = await _userRepository.GetByUsernameAsync(command.Username);
            if (existingUser != null)
                return Conflict(new { error = "Username already exists" });

            var existingEmail = await _userRepository.GetByEmailAsync(command.Email);
            if (existingEmail != null)
                return Conflict(new { error = "Email already exists" });

            var passwordHash = _passwordService.HashPassword(command.Password);
            var user = Domain.Entities.User.Create(command.Username, command.Email, passwordHash);

            await _userRepository.AddAsync(user);

            return CreatedAtAction(nameof(Register), new { id = user.Id }, new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Login and get JWT token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginQuery query)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(query.Username);
            if (user == null || !_passwordService.VerifyPassword(query.Password, user.PasswordHash))
                return Unauthorized(new { error = "Invalid credentials" });

            if (!user.IsActive)
                return Unauthorized(new { error = "User account is inactive" });

            user.UpdateLastLogin();
            await _userRepository.UpdateAsync(user);

            var token = _tokenService.GenerateToken(user.Id, user.Username, user.Role.ToString());

            return Ok(new { token, userId = user.Id, username = user.Username });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
