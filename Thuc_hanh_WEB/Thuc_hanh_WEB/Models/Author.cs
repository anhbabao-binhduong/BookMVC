using System;
using System.Collections.Generic;

namespace Thuc_hanh_WEB.Models
{
    public class Author
    {
        public int AuthorID { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Country { get; set; }

        public virtual ICollection<Book> Books { get; set; }
    }
}
