using System;

namespace Thuc_hanh_WEB.Models
{
    public class Book

    {
        public int? CategoryID { get; set; }
        public int? PublisherID { get; set; }
        public int BookID { get; set; }
        public string Title { get; set; }
        public int AuthorID { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ISBN { get; set; }
        public string Description { get; set; }
        public string CoverImage { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Author Author { get; set; }
        public virtual Category Category { get; set; }
        public virtual Publisher Publisher { get; set; }
    }
}
