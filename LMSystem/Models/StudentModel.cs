using System.ComponentModel.DataAnnotations;

namespace LMSystem.Models
{
    public class StudentModel
    {
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Student Name is required.")]
        public string? StudentName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid Phone Number.")]
        public string? Phone { get; set; }
    }
}
