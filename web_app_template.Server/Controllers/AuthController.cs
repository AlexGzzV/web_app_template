using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using web_app_template.Domain.Helpers;
using web_app_template.Domain.Models.Entities;
using web_app_template.Domain.Models.ViewModels.Auth;
using web_app_template.Domain.Models.ViewModels.Users;

namespace web_app_template.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly SignInManager<CustomIdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(IConfiguration config, ILogger<AuthController> logger, UserManager<CustomIdentityUser> userManager, SignInManager<CustomIdentityUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _config = config;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            try
            {
                string header = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(header) || !header.StartsWith("Basic "))
                    return Unauthorized();

                var encodedCredentials = header.Substring("Basic ".Length).Trim();
                var credentials = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials)).Split(':');

                if (credentials.Length != 2)
                    return Unauthorized();

                string username = credentials[0];
                string password = credentials[1];

                CustomIdentityUser user = new CustomIdentityUser();

                user = await _userManager.FindByNameAsync(username);

                if (user == null) return Unauthorized();

                var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
                if (!result.Succeeded) return Unauthorized();

                var accessToken = await GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    accessToken,
                    refreshToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] CreateUserViewModel model)
        {
            try
            {
                bool isAdmin = false;
                string role = "User";

                if (User.Identity.IsAuthenticated)
                {
                    isAdmin = User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin") ? true : false;
                    role = isAdmin ? "Admin" : "User";
                }

                if (!ModelState.IsValid) return BadRequest(ModelState);
                var user = new CustomIdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    MotherLastName = model.MotherLastName,
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded) return BadRequest(result.Errors);

                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

                await _userManager.AddToRoleAsync(user, role);

                if (model.ProfilePicture != null)
                {
                    using var memoryStream = model.ProfilePicture.OpenReadStream();
                    user.ProfilePicture = await FirebaseHelper.UploadFile(memoryStream, user.Id, "Users/ProfilePictures");
                    await _userManager.UpdateAsync(user);
                }

                return Ok(new { message = "User registered successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration.");
                return StatusCode(500, "Error during user registration.");
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);

                return Ok(new { message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenViewModel request)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(request.AccessToken);
                if (principal == null) return BadRequest("Invalid token.");

                var username = principal.Identity?.Name;
                var user = await _userManager.FindByNameAsync(username);
                if (user == null ||
                    user.RefreshToken != request.RefreshToken ||
                    user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return Unauthorized();
                }

                var newAccessToken = await GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    token = newAccessToken,
                    refreshToken = newRefreshToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh.");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<string> GenerateJwtToken(CustomIdentityUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("email", user.Email),
                new Claim("name", $"{user.FirstName} {user.LastName}{(!String.IsNullOrEmpty(user.MotherLastName) ? " " + user.MotherLastName : "")}"),
                new Claim("profilePicture", user.ProfilePicture)
            }
                .Union(userClaims)
                .Union(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false, // ❗ Importante: NO validar expiración aquí
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (securityToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
