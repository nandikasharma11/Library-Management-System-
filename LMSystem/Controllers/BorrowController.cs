using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSystem.Models;
using LMSystem.ViewModels;

namespace LMSystem.Controllers
{
    public class BorrowController : Controller
    {
        private readonly LibraryContext _context;

        public BorrowController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Borrow/Create?bookId=5
        public async Task<IActionResult> Create(int? bookId)
        {
            if (bookId == null || bookId == 0)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for borrowing.";
                return View("NotFound");
            }

            try
            {
                var book = await _context.Books12.FindAsync(bookId);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {bookId} to borrow.";
                    return View("NotFound");
                }

                if (!book.IsAvailable)
                {
                    TempData["ErrorMessage"] = $"The book '{book.Title}' is currently not available for borrowing.";
                    return View("NotAvailable");
                }

                var viewModel = new BorrowViewModel
                {
                    BookId = book.BookId,
                    BookTitle = book.Title
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the borrow form.";
                return View("Error");
            }
        }

        // POST: Borrow/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BorrowViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload book title for display
                var b = await _context.Books12.FindAsync(model.BookId);
                if (b != null)
                {
                    model.BookTitle = b.Title;
                }
                return View(model);
            }

            try
            {
                var book = await _context.Books12.FindAsync(model.BookId);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {model.BookId} to borrow.";
                    return View("NotFound");
                }

                if (!book.IsAvailable)
                {
                    TempData["ErrorMessage"] = $"The book '{book.Title}' is already borrowed.";
                    return View("NotAvailable");
                }

                var borrowRecord = new BorrowRecord
                {
                    BookId = book.BookId,
                    BorrowerName = model.BorrowerName,
                    BorrowerEmail = model.BorrowerEmail,
                    Phone = model.Phone,
                    BorrowDate = DateTime.UtcNow
                };

                // Update the book's availability
                book.IsAvailable = false;
                _context.BorrowRecords12.Add(borrowRecord);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully borrowed the book: {book.Title}.";
                return RedirectToAction("Index", "Books");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while processing the borrowing action.";
                return View("Error");
            }
        }

        // GET: Borrow/Return?borrowRecordId=5
        public async Task<IActionResult> Return(int? borrowRecordId)
        {
            if (borrowRecordId == null || borrowRecordId == 0)
            {
                TempData["ErrorMessage"] = "Borrow Record ID was not provided for returning.";
                return View("NotFound");
            }

            try
            {
                var borrowRecord = await _context.BorrowRecords12
                    .Include(br => br.Book)
                    .FirstOrDefaultAsync(br => br.BorrowRecordId == borrowRecordId);

                if (borrowRecord == null)
                {
                    TempData["ErrorMessage"] = $"No borrow record found with ID {borrowRecordId} to return.";
                    return View("NotFound");
                }

                if (borrowRecord.ReturnDate != null)
                {
                    TempData["ErrorMessage"] = $"The borrow record for '{borrowRecord.Book?.Title}' has already been returned.";
                    return View("AlreadyReturned");
                }

                var viewModel = new ReturnViewModel
                {
                    BorrowRecordId = borrowRecord.BorrowRecordId,
                    BookTitle = borrowRecord.Book?.Title,
                    BorrowerName = borrowRecord.BorrowerName,
                    BorrowDate = borrowRecord.BorrowDate
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the return confirmation.";
                return View("Error");
            }
        }

        // POST: Borrow/Return
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(ReturnViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var borrowRecord = await _context.BorrowRecords12
                    .Include(br => br.Book)
                    .FirstOrDefaultAsync(br => br.BorrowRecordId == model.BorrowRecordId);

                if (borrowRecord == null)
                {
                    TempData["ErrorMessage"] = $"No borrow record found with ID {model.BorrowRecordId} to return.";
                    return View("NotFound");
                }

                if (borrowRecord.ReturnDate != null)
                {
                    TempData["ErrorMessage"] = $"The book '{borrowRecord.Book?.Title}' has already been returned.";
                    return View("AlreadyReturned");
                }

                // Update the borrow record
                borrowRecord.ReturnDate = DateTime.UtcNow;
                
                // Update the book's availability
                if (borrowRecord.Book != null)
                {
                    borrowRecord.Book.IsAvailable = true;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully returned the book: {borrowRecord.Book?.Title}.";
                return RedirectToAction("Index", "Books");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while processing the return action.";
                return View("Error");
            }
        }
    }
}
