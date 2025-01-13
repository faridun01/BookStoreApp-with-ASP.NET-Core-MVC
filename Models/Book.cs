using System.ComponentModel.DataAnnotations;

namespace BookStoreApp.Models
{
    public class Book
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(50, ErrorMessage = "Author cannot exceed 50 characters.")]
        public string? Author { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        public string? FilePath { get; set; } // Path to the file stored statically

        public DateTime UploadDate { get; set; } = DateTime.Now; // Automatically set upload date

        [Range(0, int.MaxValue)]
        public int DownloadCount { get; set; } = 0; 
    }
}

