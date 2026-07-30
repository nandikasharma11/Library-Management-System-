using System.Collections.Generic;

namespace LMSystem.Models
{
    public class StudentIndexViewModel
    {
        public string? SearchTerm { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 5;
        public List<StudentModel> Students { get; set; } = new List<StudentModel>();
    }
}
