using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using LMSystem.Models;

namespace LMSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config)
        {
            _config = config;
        }

        private string GetConnectionString()
        {
            return _config.GetConnectionString("DefaultConnection") ?? 
                "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LMS;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        }

        // GET: Student
        public IActionResult Index(string? searchTerm, int page = 1)
        {
            var viewModel = new StudentIndexViewModel
            {
                SearchTerm = searchTerm,
                CurrentPage = page < 1 ? 1 : page
            };

            var students = new List<StudentModel>();
            int totalRecords = 0;

            try
            {
                using var con = new SqlConnection(GetConnectionString());
                con.Open();

                // 1. Build Dynamic Search Query Components
                string searchCondition = "";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchCondition = " WHERE Student_Name LIKE @Search OR Email LIKE @Search OR Phone_Number LIKE @Search";
                }

                // 2. Query Total Count for Pagination Bounds
                string countQuery = $"SELECT COUNT(*) FROM Students{searchCondition}";
                using (var countCmd = new SqlCommand(countQuery, con))
                {
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        countCmd.Parameters.AddWithValue("@Search", $"%{searchTerm}%");
                    }
                    totalRecords = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                viewModel.TotalPages = (int)Math.Ceiling((double)totalRecords / viewModel.PageSize);
                if (viewModel.CurrentPage > viewModel.TotalPages && viewModel.TotalPages > 0)
                {
                    viewModel.CurrentPage = viewModel.TotalPages;
                }

                // 3. Fetch Paginated Segment using OFFSET-FETCH
                int offset = (viewModel.CurrentPage - 1) * viewModel.PageSize;
                string dataQuery = $@"SELECT * FROM Students{searchCondition}
                                      ORDER BY StudentId 
                                      OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (var dataCmd = new SqlCommand(dataQuery, con))
                {
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        dataCmd.Parameters.AddWithValue("@Search", $"%{searchTerm}%");
                    }
                    dataCmd.Parameters.AddWithValue("@Offset", offset);
                    dataCmd.Parameters.AddWithValue("@PageSize", viewModel.PageSize);

                    using var reader = dataCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        students.Add(new StudentModel
                        {
                            StudentId = Convert.ToInt32(reader["StudentId"]),
                            StudentName = reader["Student_Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            Phone = reader["Phone_Number"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading paginated students: " + ex.Message);
                // Seed dummy data for local preview on macOS
                students.Add(new StudentModel { StudentId = 2, StudentName = "Bob Smith", Email = "bob.smith@email.com", Phone = "555-0102" });
                students.Add(new StudentModel { StudentId = 3, StudentName = "Charlie Brown", Email = "charlie.b@email.com", Phone = "555-0103" });
                students.Add(new StudentModel { StudentId = 4, StudentName = "Diana Prince", Email = "diana.p@email.com", Phone = "555-0104" });
                students.Add(new StudentModel { StudentId = 5, StudentName = "Evan Wright", Email = "evan.w@email.com", Phone = "555-0105" });
                viewModel.TotalPages = 1;
            }

            viewModel.Students = students;
            return View(viewModel);
        }

        // GET: Student/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StudentModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using var con = new SqlConnection(GetConnectionString());
                var cmd = new SqlCommand("INSERT INTO Students (Student_Name, Email, Phone_Number) VALUES (@Name, @Email, @Phone)", con);
                cmd.Parameters.AddWithValue("@Name", model.StudentName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", model.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", model.Phone ?? (object)DBNull.Value);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting student: " + ex.Message);
            }
            return RedirectToAction("Index");
        }

        // GET: Student/Edit/5
        public IActionResult Edit(int id)
        {
            var student = new StudentModel();
            try
            {
                using var con = new SqlConnection(GetConnectionString());
                var cmd = new SqlCommand("SELECT * FROM Students WHERE StudentId=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    student.StudentId = Convert.ToInt32(reader["StudentId"]);
                    student.StudentName = reader["Student_Name"].ToString();
                    student.Email = reader["Email"].ToString();
                    student.Phone = reader["Phone_Number"].ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading student details: " + ex.Message);
                // Return dummy for visual editing
                student = new StudentModel { StudentId = id, StudentName = "Mock Name", Email = "mock@email.com", Phone = "555-1234" };
            }
            return View(student);
        }

        // POST: Student/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(StudentModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using var con = new SqlConnection(GetConnectionString());
                var cmd = new SqlCommand("UPDATE Students SET Student_Name=@Name, Email=@Email, Phone_Number=@Phone WHERE StudentId=@id", con);
                cmd.Parameters.AddWithValue("@Name", model.StudentName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", model.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", model.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@id", model.StudentId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating student: " + ex.Message);
            }
            return RedirectToAction("Index");
        }

        // GET: Student/Delete/5
        public IActionResult Delete(int id)
        {
            try
            {
                using var con = new SqlConnection(GetConnectionString());
                var cmd = new SqlCommand("DELETE FROM Students WHERE StudentId=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting student: " + ex.Message);
            }
            return RedirectToAction("Index");
        }
    }
}
