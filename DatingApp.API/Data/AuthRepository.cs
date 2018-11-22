using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthReposotory
    {
        private readonly DataContext _context;
        public AuthRepository(DataContext context)
        {
            _context = context;

        }
        public async Task<User> Login(string UserName, string Password)
        {
             var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == UserName);
             if (user == null) return null;
             if (!DecryptHash(Password, user.PasswordHash, user.PasswordSalt)) {
                 return null;
             }
             return user;
        }

        private void GenerateHash(string Password, out byte[] PasswordHash, out byte[] PasswordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                PasswordSalt = hmac.Key;
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Password));
            }
        }
        private bool DecryptHash (string password, byte[] PassowrdHash, byte[] PasswordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512(PasswordSalt)) {
                var ComputedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i< ComputedHash.Length; i++) {
                    if (PassowrdHash[i] != ComputedHash[i]) return false;
                }

            }
            return true;
          
        }
        public async Task<User>  Register(User user, string Password)
        {
            byte[] PasswordHash, PasswordSalt;
            GenerateHash(Password, out PasswordHash, out PasswordSalt);

            user.PasswordHash = PasswordHash;
            user.PasswordSalt = PasswordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;

        }

        public async Task<bool> UserExists(string userName)
        {
           if (await _context.Users.AnyAsync(x => x.UserName == userName)) {
               return true;
           } else {
               return false;
           }

        }
    }
}