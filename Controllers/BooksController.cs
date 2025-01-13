using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookStoreApp.Data;
using BookStoreApp.Models;

namespace BookStoreApp.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            return View(await _context.Books.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                        // Define the directory for storing files
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "books");

                        // Ensure the directory exists
                        Directory.CreateDirectory(uploadsFolder);

                        // Generate a unique file name based on the title
                        var sanitizedTitle = string.Join("_", book.Title.Split(Path.GetInvalidFileNameChars())); // Sanitize title
                        var fileName = sanitizedTitle + Path.GetExtension(uploadedFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Save the file to the server
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadedFile.CopyToAsync(fileStream);
                        }

                        // Set auto-generated properties
                        book.FilePath = $"/uploads/books/{fileName}"; // Store relative path
                        book.UploadDate = DateTime.Now; // Current date and time
                        book.DownloadCount = 0; // Initialize download count

                        // Add book to the database
                        _context.Add(book);
                        await _context.SaveChangesAsync();

                        // Redirect to the Index page
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("uploadedFile", "Please upload a valid file.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Add the full error message to the ModelState for debugging
                ModelState.AddModelError("", $"An unexpected error occurred: {ex.InnerException?.Message ?? ex.Message}");
            }

            // Return the view with error messages if something goes wrong
            return View(book);
        }



        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }


        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Author,Description")] Book updatedBook)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the original book from the database
                    var book = await _context.Books.FindAsync(id);
                    if (book == null)
                    {
                        return NotFound();
                    }

                    // Update only the editable fields
                    book.Title = updatedBook.Title;
                    book.Author = updatedBook.Author;
                    book.Description = updatedBook.Description;

                    // Save the changes to the database
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(updatedBook);
        }


        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

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
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            // Retrieve the book by its ID
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound("Book not found.");
            }

            // Get the full file path from the database
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", book.FilePath.TrimStart('/'));

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            // Increment the DownloadCount
            book.DownloadCount++;
            await _context.SaveChangesAsync();

            // Serve the file for download
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", Path.GetFileName(filePath));
        }

    }
}
