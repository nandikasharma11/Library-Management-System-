using System;
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
                _context.Add(publication);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { type = publication.Type.ToString() });
            }
            ViewData["CurrentType"] = publication.Type.ToString();
            return View(publication);
        }

        // GET: Publications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var publication = await _context.Publications.FindAsync(id);
            if (publication == null) return NotFound();
            ViewData["CurrentType"] = publication.Type.ToString();
            return View(publication);
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
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Publications.Any(e => e.Id == publication.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
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
            var publication = await _context.Publications.FirstOrDefaultAsync(m => m.Id == id);
            if (publication == null) return NotFound();
            return View(publication);
        }

        // POST: Publications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publication = await _context.Publications.FindAsync(id);
            if (publication != null)
            {
                _context.Publications.Remove(publication);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { type = publication?.Type.ToString() ?? "Newspaper" });
        }
    }
}
