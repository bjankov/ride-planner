using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RydePlannr.AuthService.DTOs;
using RydePlannr.AuthService.Services.Interfaces;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Exceptions;
using RydePlannr.Domain.Interfaces;

namespace RydePlannr.AuthService.Services.Implementations;

public class AuthenticationService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthenticationService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<string> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(dto.Email, cancellationToken))
            throw new InvalidOperationException("Korisnik s tim emailom već postoji.");

        var userRole = await _unitOfWork.Repository<Role>().GetByIdAsync(RoleIds.DefaultUser, cancellationToken)
            ?? throw new InvalidOperationException("Defaultna rola nije pronađena.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = userRole.Id
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DuplicateFieldException ex)
        {
            throw ex.FieldName switch
            {
                "Email" => new InvalidOperationException("Korisnik s tim emailom već postoji."),
                "Username" => new InvalidOperationException("Korisničko ime je već zauzeto."),
                _ => new InvalidOperationException("Korisnik s tim podacima već postoji.")
            };
        }

        return GenerateJwtToken(user, userRole.Name);
    }

    public async Task<string> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Pogrešan email ili lozinka.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Pogrešan email ili lozinka.");

        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(user.RoleId, cancellationToken)
            ?? throw new InvalidOperationException("Rola korisnika nije pronađena.");

        return GenerateJwtToken(user, role.Name);
    }

    private string GenerateJwtToken(User user, string roleName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT ključ nije konfiguriran.")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, roleName)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
