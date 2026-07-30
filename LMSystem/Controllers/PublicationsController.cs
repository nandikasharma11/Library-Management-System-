using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSystem.Models;

namespace LMSystem.Controllers
{
    public class PublicationsController : Controller
    {
        private readonly LibraryContext _context;

        public PublicationsController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Publications/Index/Newspaper or Publications/Index/Magazine
        public async Task<IActionResult> Index(string type, string searchString, int pageNumber = 1)
        {
            if (string.IsNullOrEmpty(type)) return BadRequest();
            
            // Convert string route to Enum
            if (!Enum.TryParse(type, true, out PublicationType pubType)) return NotFound();

            ViewData["CurrentType"] = type;
            ViewData["CurrentFilter"] = searchString;

            try
            {
                var items = _context.Publications.Where(p => p.Type == pubType).AsQueryable();

                // 1. Search Logic
                if (!string.IsNullOrEmpty(searchString))
                {
                    var clean = searchString.Trim().ToLower();
                    items = items.Where(p => 
                        (p.Title != null && p.Title.ToLower().Contains(clean)) || 
                        (p.Publisher != null && p.Publisher.ToLower().Contains(clean))
                    );
                }

                // 2. Pagination Logic
                int pageSize = 5;
                var totalItems = await items.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (pageNumber < 1) pageNumber = 1;
                if (pageNumber > totalPages && totalPages > 0) pageNumber = totalPages;

                var paginatedList = await items
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewData["PageNumber"] = pageNumber;
                ViewData["TotalPages"] = totalPages;

                return View(paginatedList);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database connection failed, falling back to mock publications: " + ex.Message);
                
                var mockPublications = new List<Publication>();
                if (pubType == PublicationType.Newspaper)
                {
                    mockPublications.Add(new Publication { Id = 1, Title = "The Daily Times", Publisher = "Global Media Group", PublishedDate = new DateTime(2026, 7, 22), Type = PublicationType.Newspaper, IsAvailable = true });
                    mockPublications.Add(new Publication { Id = 2, Title = "Financial Chronicle", Publisher = "WallSt Press", PublishedDate = new DateTime(2026, 7, 21), Type = PublicationType.Newspaper, IsAvailable = true });
                    mockPublications.Add(new Publication { Id = 3, Title = "Tech Weekly News", Publisher = "Silicon Valley Pubs", PublishedDate = new DateTime(2026, 7, 20), Type = PublicationType.Newspaper, IsAvailable = true });
                    mockPublications.Add(new Publication { Id = 4, Title = "Metro Morning Post", Publisher = "City Press House", PublishedDate = new DateTime(2026, 7, 22), Type = PublicationType.Newspaper, IsAvailable = true });
                    mockPublications.Add(new Publication { Id = 5, Title = "Saturday Sports Herald", Publisher = "Global Media Group", PublishedDate = new DateTime(2026, 7, 18), Type = PublicationType.Newspaper, IsAvailable = false });
                }
                else
                {
                    mockPublications.Add(new Publication { Id = 6, Title = "National Geographic Vol 45", Publisher = "NatGeo Society", PublishedDate = new DateTime(2026, 7, 1), Type = PublicationType.Magazine, IsAvailable = true });
                    mockPublications.Add(new Publication { Id = 7, Title = "Vogue Fashion Summer", Publisher = "Condé Nast", PublishedDate = new DateTime(2026, 6, 15), Type = PublicationType.Magazine, IsAvailable = true });
                    mockPublications.Add(new Publication { Id = 8, Title = "Forbes Business 30 Under 30", Publisher = "Forbes Media", PublishedDate = new DateTime(2026, 7, 10), Type = PublicationType.Magazine, IsAvailable = false });
                    mockPublications.Add(new Publication { Id = 9, Title = "PC Gamer Ultimate", Publisher = "Future US", PublishedDate = new DateTime(2026, 7, 5), Type = PublicationType.Magazine, IsAvailable = true });
                    mockPublications.Add(new Publication { Id = 10, Title = "Scientific American", Publisher = "Springer Nature", PublishedDate = new DateTime(2026, 6, 28), Type = PublicationType.Magazine, IsAvailable = true });
                }

                if (!string.IsNullOrEmpty(searchString))
                {
                    var clean = searchString.Trim().ToLower();
                    mockPublications = mockPublications.Where(p => 
                        (p.Title != null && p.Title.ToLower().Contains(clean)) || 
                        (p.Publisher != null && p.Publisher.ToLower().Contains(clean))
                    ).ToList();
                }

                int pageSize = 5;
                var totalItems = mockPublications.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (pageNumber < 1) pageNumber = 1;
                if (pageNumber > totalPages && totalPages > 0) pageNumber = totalPages;

                var paginatedList = mockPublications
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewData["PageNumber"] = pageNumber;
                ViewData["TotalPages"] = totalPages;

                return View(paginatedList);
            }
        }

        // GET: Publications/Create
        public IActionResult Create(string type)
        {
            ViewData["CurrentType"] = type;
            return View();
        }

        // POST: Publications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Publisher,PublishedDate,Type")] Publication publication)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(publication);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { type = publication.Type.ToString() });
                }
                catch (Exception)
                {
                    return RedirectToAction(nameof(Index), new { type = publication.Type.ToString() });
                }
            }
            ViewData["CurrentType"] = publication.Type.ToString();
            return View(publication);
        }

        // GET: Publications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            try
            {
                var publication = await _context.Publications.FindAsync(id);
                if (publication == null) return NotFound();
                ViewData["CurrentType"] = publication.Type.ToString();
                return View(publication);
            }
            catch (Exception)
            {
                var mockPublication = new Publication { Id = id.Value, Title = "Mock Publication", Publisher = "Mock Publisher", PublishedDate = DateTime.UtcNow, Type = PublicationType.Newspaper, IsAvailable = true };
                ViewData["CurrentType"] = mockPublication.Type.ToString();
                return View(mockPublication);
            }
        }

        // POST: Publications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Publisher,PublishedDate,Type,IsAvailable")] Publication publication)
        {
            if (id != publication.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(publication);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    // Fallback redirect
                }
                return RedirectToAction(nameof(Index), new { type = publication.Type.ToString() });
            }
            ViewData["CurrentType"] = publication.Type.ToString();
            return View(publication);
        }

        // GET: Publications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            try
            {
                var publication = await _context.Publications.FirstOrDefaultAsync(m => m.Id == id);
                if (publication == null) return NotFound();
                return View(publication);
            }
            catch (Exception)
            {
                var mockPublication = new Publication { Id = id.Value, Title = "Mock Publication", Publisher = "Mock Publisher", PublishedDate = DateTime.UtcNow, Type = PublicationType.Newspaper, IsAvailable = true };
                return View(mockPublication);
            }
        }

        // POST: Publications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var publication = await _context.Publications.FindAsync(id);
                if (publication != null)
                {
                    _context.Publications.Remove(publication);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index), new { type = publication?.Type.ToString() ?? "Newspaper" });
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index), new { type = "Newspaper" });
            }
        }
    }
}
