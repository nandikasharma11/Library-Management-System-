using System.ComponentModel.DataAnnotations;

namespace LMSystem.Models
{
    public class LoginModel
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string? username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string? password { get; set; }
    }
}
