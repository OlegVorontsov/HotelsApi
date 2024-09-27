using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class TokenService : ITokenService
{
  private TimeSpan ExpireDuration = new TimeSpan(0, 30, 0);
  public string BuildToken(string key, string issuer, UserDto userDto)
  {
    var claims = new []
    {
      new Claim(ClaimTypes.Name, userDto.userName),
      new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
    };
      var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
      var credentials = new SigningCredentials(securityKey,
          SecurityAlgorithms.HmacSha256Signature);
      var tokenDescriptor = new JwtSecurityToken(issuer, issuer, claims,
          expires: DateTime.Now.Add(ExpireDuration), signingCredentials: credentials);
      return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
  }
}