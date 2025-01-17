using BookStoreApp.Helpers;
using BookStoreApp.Models; 

namespace BookStoreApp.ViewModels
{
    public class PaginatedBooksViewModel
    {
        public PaginatedList<Book> Books { get; set; }
        public string SearchQuery { get; set; }
    }
}
