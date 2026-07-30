using System.ComponentModel.DataAnnotations;

namespace LMSystem.Models
{
    public class LibrarianModel
    {
        public int LibrarianId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Age is required.")]
        [Range(18, 100, ErrorMessage = "Age must be between 18 and 100.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        [Phone(ErrorMessage = "Invalid Phone Number.")]
        public string? Phone { get; set; }
    }
}
