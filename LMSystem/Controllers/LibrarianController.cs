using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using LMSystem.Models;

namespace LMSystem.Controllers
{
    public class LibrarianController : Controller
    {
        private readonly IConfiguration _config;

        public LibrarianController(IConfiguration config)
        {
            _config = config;
        }

        private string GetConnectionString()
        {
            return _config.GetConnectionString("DefaultConnection") ?? 
                "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LMS;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        }

        // GET: Librarian
        public IActionResult Index()
        {
            var librarians = new List<LibrarianModel>();
            try
            {
                using var con = new SqlConnection(GetConnectionString());
                var cmd = new SqlCommand("SELECT * FROM Librarians", con);
                con.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    librarians.Add(new LibrarianModel
                    {
                        LibrarianId = Convert.ToInt32(reader["LibrarianId"]),
                        Name = reader["Name"].ToString(),
                        Age = Convert.ToInt32(reader["Age"]),
                        Phone = reader["Phone"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading librarians: " + ex.Message);
                // Return dummy list for layout demonstration on Mac
                librarians.Add(new LibrarianModel { LibrarianId = 3, Name = "Michael Scott", Age = 45, Phone = "555-0203" });
                librarians.Add(new LibrarianModel { LibrarianId = 4, Name = "Ellen Ripley", Age = 39, Phone = "555-0204" });
                librarians.Add(new LibrarianModel { LibrarianId = 5, Name = "James Bond", Age = 40, Phone = "555-0205" });
            }
            return View(librarians);
        }

        // GET: Librarian/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Librarian/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LibrarianModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using var con = new SqlConnection(GetConnectionString());
                var cmd = new SqlCommand("INSERT INTO Librarians (Name, Age, Phone) VALUES (@Name, @Age, @Phone)", con);
                cmd.Parameters.AddWithValue("@Name", model.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Age", model.Age);
                cmd.Parameters.AddWithValue("@Phone", model.Phone ?? (object)DBNull.Value);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting librarian: " + ex.Message);
            }
            return RedirectToAction("Index");
        }

        // GET: Librarian/Edit/5
        public IActionResult Edit(int id)
        {
            var librarian = new LibrarianModel();
            try
            {
                using var con = new SqlConnection(GetConnectionString());
                var cmd = new SqlCommand("SELECT * FROM Librarians WHERE LibrarianId=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    librarian.LibrarianId = Convert.ToInt32(reader["LibrarianId"]);
                    librarian.Name = reader["Name"].ToString();
                    librarian.Age = Convert.ToInt32(reader["Age"]);
                    librarian.Phone = reader["Phone"].ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading librarian: " + ex.Message);
                librarian = new LibrarianModel { LibrarianId = id, Name = "Mock Librarian", Age = 35, Phone = "555-4321" };
            }
            return View(librarian);
        }

        // POST: Librarian/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(LibrarianModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using var con = new SqlConnection(GetConnectionString());
                var cmd = new SqlCommand("UPDATE Librarians SET Name=@Name, Age=@Age, Phone=@Phone WHERE LibrarianId=@id", con);
                cmd.Parameters.AddWithValue("@Name", model.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Age", model.Age);
                cmd.Parameters.AddWithValue("@Phone", model.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@id", model.LibrarianId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating librarian: " + ex.Message);
            }
            return RedirectToAction("Index");
        }

        // GET: Librarian/Delete/5
        public IActionResult Delete(int id)
        {
            try
            {
                using var con = new SqlConnection(GetConnectionString());
                var cmd = new SqlCommand("DELETE FROM Librarians WHERE LibrarianId=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting librarian: " + ex.Message);
            }
            return RedirectToAction("Index");
        }
    }
}
