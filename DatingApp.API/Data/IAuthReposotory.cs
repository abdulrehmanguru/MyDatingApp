using System.Threading.Tasks;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public interface IAuthReposotory
    {
         Task<User> Register (User user, string Password);
         Task<User> Login (string UserName, string Password);
         Task<bool> UserExists (string UserNmae);
    }
}