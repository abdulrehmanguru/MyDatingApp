using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Models;
using DatingApp.API.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthReposotory _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthReposotory rep, IConfiguration config)
        {
            _config = config;
            _repo = rep;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDTO udto)
        {
            var userName = udto.username.ToLower();

            if (await _repo.UserExists(userName))
                return BadRequest("username already exists");

            var userToRegister = new User
            {
                UserName = userName
            };

            var newUser = await _repo.Register(userToRegister, udto.password);
            if (newUser != null)
            {
                return StatusCode(201, newUser);
            }
            return StatusCode(401, "Invalid Requres");
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(userLoginDTO loginDto) {
            var loginUser = await _repo.Login(loginDto.username.ToLower(), loginDto.password);
            if (loginUser == null) return Unauthorized();

            var claims = new[] {
               new Claim(ClaimTypes.NameIdentifier, loginUser.Id.ToString()),
               new Claim(ClaimTypes.Name, loginUser.UserName)
           };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDesc = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims),
                Expires = System.DateTime.Now.AddDays(1),
                SigningCredentials = creds,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDesc);

            return Ok(new { token = tokenHandler.WriteToken(token) });
        }

    }
}