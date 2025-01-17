using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreApp.Data;
using BookStoreApp.Models;
using BookStoreApp.Helpers;
using BookStoreApp.ViewModels;

namespace BookStoreApp.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> About()
        {
            // Fetch all books from the database asynchronously
            var books = await _context.Books.ToListAsync();

            // Pass the books to the AboutViewModel
            var viewModel = new AboutViewModel
            {
                Books = books
            };

            return View(viewModel);
        }

        // GET: Books
        public async Task<IActionResult> Index(string query, int page = 1, int pageSize = 8)
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                books = books.Where(b => b.Title.Contains(query) || b.Author.Contains(query));
            }

            var paginatedBooks = await PaginatedList<Book>.CreateAsync(books, page, pageSize);

            var viewModel = new PaginatedBooksViewModel
            {
                Books = paginatedBooks,
                SearchQuery = query
            };

            return View(viewModel);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null) return NotFound();

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Author,Description")] Book book, IFormFile uploadedFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (uploadedFile != null && uploadedFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine("wwwroot", "uploads", "books");
                        Directory.CreateDirectory(uploadsFolder);

                        var sanitizedTitle = string.Join("_", book.Title.Split(Path.GetInvalidFileNameChars()));
                        var fileName = $"{sanitizedTitle}{Path.GetExtension(uploadedFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadedFile.CopyToAsync(stream);
                        }

                        book.FilePath = $"/uploads/books/{fileName}";
                        book.UploadDate = DateTime.Now;
                        book.DownloadCount = 0;

                        _context.Add(book);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("uploadedFile", "Please upload a valid file.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Author,Description")] Book updatedBook)
        {
            if (!ModelState.IsValid) return View(updatedBook);

            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null) return NotFound();

                book.Title = updatedBook.Title;
                book.Author = updatedBook.Author;
                book.Description = updatedBook.Description;

                _context.Update(book);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id)) return NotFound();
                throw;
            }
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                // Delete the file from the filesystem
                if (!string.IsNullOrEmpty(book.FilePath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", book.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Remove the book from the database
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Books/Download/5
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound("Book not found.");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", book.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath)) return NotFound("File not found.");

            book.DownloadCount++;
            await _context.SaveChangesAsync();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", Path.GetFileName(filePath));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
    }
}
