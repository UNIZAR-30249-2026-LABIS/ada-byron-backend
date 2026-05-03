using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AdaByron.Application.Ports.Out;
using AdaByron.Domain.Aggregates.PersonAggregate;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AdaByron.Infrastructure.Identity;

// Implementación del puerto ITokenService usando JWT (HS256).
// La clave secreta, issuer y audience se leen desde appsettings.json sección "Jwt".
public class TokenService(IConfiguration config) : ITokenService
{
    public string GenerarToken(Persona persona)
    {
        var clave = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                config["Jwt:Key"] ?? throw new InvalidOperationException("La clave JWT no está configurada.")));

        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, persona.Email),
            new(ClaimTypes.Name, persona.NombreCompleto),
            new("departamento", persona.Departamento.Nombre),
        };

        claims.AddRange(persona.Roles.Select(rol => new Claim(ClaimTypes.Role, rol.ToString())));

        var expiracionHoras = int.TryParse(config["Jwt:ExpirationHours"], out var h) ? h : 8;

        var token = new JwtSecurityToken(
            issuer:             config["Jwt:Issuer"],
            audience:           config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(expiracionHoras),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
