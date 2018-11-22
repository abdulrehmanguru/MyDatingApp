using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.ViewModels
{
    public class UserDTO
    {
        [Required(ErrorMessage = "username is required, you know")]
        public string username { get; set; }
        [Required(ErrorMessage = "password is required, you know")]
        public string password { get; set; }
    }
}