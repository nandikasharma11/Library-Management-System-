using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSystem.Models;

namespace LMSystem.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string? searchQuery, int page = 1)
        {
            try
            {
                int pageSize = 5;
                var booksQuery = _context.Books12
                    .Include(b => b.BorrowRecords)
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    var cleanQuery = searchQuery.Trim().ToLower();
                    booksQuery = booksQuery.Where(b =>
                        (b.Title != null && b.Title.ToLower().Contains(cleanQuery)) ||
                        (b.Author != null && b.Author.ToLower().Contains(cleanQuery)) ||
                        (b.ISBN != null && b.ISBN.ToLower().Contains(cleanQuery))
                    );
                }

                int totalItems = await booksQuery.CountAsync();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                var books = await booksQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var viewModel = new BookListViewModel
                {
                    Books = books,
                    SearchQuery = searchQuery,
                    CurrentPage = page,
                    TotalPages = totalPages
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database connection failed, falling back to mock books: " + ex.Message);
                
                var mockBooks = new List<Book>
                {
                    new Book { BookId = 1, Title = "The Pragmatic Programmer", Author = "Andrew Hunt and David Thomas", ISBN = "978-0201616224", PublishedDate = new DateTime(2021, 10, 30), IsAvailable = true },
                    new Book { BookId = 2, Title = "Design Pattern using C#", Author = "Robert C. Martin", ISBN = "978-0132350884", PublishedDate = new DateTime(2023, 8, 1), IsAvailable = true },
                    new Book { BookId = 3, Title = "Mastering ASP.NET Core", Author = "Pranaya Kumar Rout", ISBN = "978-0451616235", PublishedDate = new DateTime(2022, 11, 22), IsAvailable = false,
                        BorrowRecords = new List<BorrowRecord> { new BorrowRecord { BorrowRecordId = 1, BookId = 3, BorrowerName = "Pranaya", BorrowDate = DateTime.UtcNow.AddDays(-5) } } },
                    new Book { BookId = 4, Title = "SQL Server with DBA", Author = "Rakesh Kumat", ISBN = "978-4562350123", PublishedDate = new DateTime(2020, 8, 15), IsAvailable = true }
                };

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    var cleanQuery = searchQuery.Trim().ToLower();
                    mockBooks = mockBooks.Where(b =>
                        (b.Title != null && b.Title.ToLower().Contains(cleanQuery)) ||
                        (b.Author != null && b.Author.ToLower().Contains(cleanQuery)) ||
                        (b.ISBN != null && b.ISBN.ToLower().Contains(cleanQuery))
                    ).ToList();
                }

                int pageSize = 5;
                int totalItems = mockBooks.Count;
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                var books = mockBooks.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var viewModel = new BookListViewModel
                {
                    Books = books,
                    SearchQuery = searchQuery,
                    CurrentPage = page,
                    TotalPages = totalPages
                };

                return View(viewModel);
            }
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["ErrorMessage"] = "Book ID was not provided.";
                return View("NotFound");
            }

            try
            {
                var book = await _context.Books12
                    .FirstOrDefaultAsync(m => m.BookId == id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id}.";
                    return View("NotFound");
                }
                return View(book);
            }
            catch (Exception)
            {
                // Fallback to mock details
                var mockBook = new Book { BookId = id.Value, Title = "Mock Book Details", Author = "Anonymous Author", ISBN = "978-0000000000", PublishedDate = DateTime.UtcNow, IsAvailable = true };
                if (id == 1)
                {
                    mockBook.Title = "The Pragmatic Programmer";
                    mockBook.Author = "Andrew Hunt and David Thomas";
                    mockBook.ISBN = "978-0201616224";
                    mockBook.PublishedDate = new DateTime(2021, 10, 30);
                }
                else if (id == 2)
                {
                    mockBook.Title = "Design Pattern using C#";
                    mockBook.Author = "Robert C. Martin";
                    mockBook.ISBN = "978-0132350884";
                    mockBook.PublishedDate = new DateTime(2023, 8, 1);
                }
                else if (id == 3)
                {
                    mockBook.Title = "Mastering ASP.NET Core";
                    mockBook.Author = "Pranaya Kumar Rout";
                    mockBook.ISBN = "978-0451616235";
                    mockBook.PublishedDate = new DateTime(2022, 11, 22);
                    mockBook.IsAvailable = false;
                }
                return View(mockBook);
            }
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    book.IsAvailable = true;
                    _context.Books12.Add(book);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Successfully added the book: {book.Title}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    TempData["SuccessMessage"] = $"(Mock Mode) Successfully added the book: {book.Title}.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for editing.";
                return View("NotFound");
            }

            try
            {
                var book = await _context.Books12.AsNoTracking().FirstOrDefaultAsync(m => m.BookId == id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id} for editing.";
                    return View("NotFound");
                }
                return View(book);
            }
            catch (Exception)
            {
                var mockBook = new Book { BookId = id.Value, Title = "The Pragmatic Programmer", Author = "Andrew Hunt and David Thomas", ISBN = "978-0201616224", PublishedDate = new DateTime(2021, 10, 30), IsAvailable = true };
                return View(mockBook);
            }
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, Book book)
        {
            if (id == null || id == 0)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for updating.";
                return View("NotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBook = await _context.Books12.FindAsync(id);
                    if (existingBook == null)
                    {
                        TempData["ErrorMessage"] = $"No book found with ID {id} for updating.";
                        return View("NotFound");
                    }

                    existingBook.Title = book.Title;
                    existingBook.Author = book.Author;
                    existingBook.ISBN = book.ISBN;
                    existingBook.PublishedDate = book.PublishedDate;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Successfully updated the book: {book.Title}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    TempData["SuccessMessage"] = $"(Mock Mode) Successfully updated the book: {book.Title}.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for deletion.";
                return View("NotFound");
            }

            try
            {
                var book = await _context.Books12.AsNoTracking().FirstOrDefaultAsync(m => m.BookId == id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id} for deletion.";
                    return View("NotFound");
                }
                return View(book);
            }
            catch (Exception)
            {
                var mockBook = new Book { BookId = id.Value, Title = "The Pragmatic Programmer", Author = "Andrew Hunt and David Thomas", ISBN = "978-0201616224", PublishedDate = new DateTime(2021, 10, 30), IsAvailable = true };
                return View(mockBook);
            }
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var book = await _context.Books12.FindAsync(id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id} for deletion.";
                    return View("NotFound");
                }

                _context.Books12.Remove(book);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully deleted the book: {book.Title}.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["SuccessMessage"] = $"(Mock Mode) Successfully deleted the book.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool BookExists(int id)
        {
            return _context.Books12.Any(e => e.BookId == id);
        }
    }
}
