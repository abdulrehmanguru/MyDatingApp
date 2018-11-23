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
            // Contains classes that implement claims-based identity in the .NET Framework,
            // including classes that represent claims, claims-based identities, and claims-based principals
            var claims = new[] {
               new Claim(ClaimTypes.NameIdentifier, loginUser.Id.ToString()), // Knownd Claim Types
               new Claim(ClaimTypes.Name, loginUser.UserName)
           };
           //Symmetric-key algorithms are algorithms for cryptography that use the same cryptographic keys for both encryption
           // of plaintext and decryption of ciphertext. 
           //The keys may be identical or there may be a simple transformation to go between the two keys.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            // Create Digital Key
            // Represents the cryptographic key and security algorithms that are used to generate a digital signature.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            // SecurityTokenDescriptor
            // This is a place holder for all the attributes related to the issued token
            var tokenDesc = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims), // Gets or sets the output claims to be included in the issued token.
                Expires = System.DateTime.Now.AddDays(1),
                SigningCredentials = creds // Gets or sets the credentials that are used to sign the token.
            };
            // JwtSecurityTokenHandler
            // A SecurityTokenHandler designed for creating and validating JSON Web Tokens (JWT). 
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDesc); // Creates a JwtSecurityToken based on values found in the SecurityTokenDescriptor
            //System.Security.Claims Namespace
            // Contains classes that implement claims-based identity in the .NET Framework, including classes that represent claims,
            // claims-based identities, and claims-based principals.

            return Ok(new {token = tokenHandler.WriteToken(token)}); // Writes the JwtSecurityToken as a JSON Compact serialized format string.
        }

    }
}