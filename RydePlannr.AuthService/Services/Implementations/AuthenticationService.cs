using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;

    public AuthenticationService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _accessTokenExpiryMinutes = configuration.GetValue("Jwt:AccessTokenExpiryMinutes", 30);
        _refreshTokenExpiryDays = configuration.GetValue("Jwt:RefreshTokenExpiryDays", 30);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
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

        var refreshTokenValue = await IssueRefreshTokenAsync(user, cancellationToken);

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

        return new AuthResponseDto
        {
            AccessToken = GenerateAccessToken(user, userRole.Name),
            RefreshToken = refreshTokenValue
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Pogrešan email ili lozinka.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Pogrešan email ili lozinka.");

        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(user.RoleId, cancellationToken)
            ?? throw new InvalidOperationException("Rola korisnika nije pronađena.");

        var refreshTokenValue = await IssueRefreshTokenAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = GenerateAccessToken(user, role.Name),
            RefreshToken = refreshTokenValue
        };
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshRequestDto dto, CancellationToken cancellationToken = default)
    {
        var existingToken = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(HashToken(dto.RefreshToken), cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
            throw new UnauthorizedAccessException("Neispravan ili istekao refresh token.");

        // Rotation: the presented refresh token is single-use — revoke it and issue a new one.
        existingToken.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<RefreshToken>().UpdateAsync(existingToken);

        var newRefreshTokenValue = await IssueRefreshTokenAsync(existingToken.User, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = GenerateAccessToken(existingToken.User, existingToken.User.Role.Name),
            RefreshToken = newRefreshTokenValue
        };
    }

    public async Task RevokeAsync(RefreshRequestDto dto, CancellationToken cancellationToken = default)
    {
        var existingToken = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(HashToken(dto.RefreshToken), cancellationToken);

        if (existingToken is null || existingToken.RevokedAt is not null)
            return;

        existingToken.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<RefreshToken>().UpdateAsync(existingToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> IssueRefreshTokenAsync(User user, CancellationToken cancellationToken)
    {
        var refreshTokenValue = GenerateRefreshTokenValue();

        var refreshToken = new RefreshToken
        {
            TokenHash = HashToken(refreshTokenValue),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            User = user
        };

        await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken, cancellationToken);

        return refreshTokenValue;
    }

    private static string GenerateRefreshTokenValue()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private static string HashToken(string token)
    {
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    private string GenerateAccessToken(User user, string roleName)
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
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
