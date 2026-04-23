using System.Collections.Generic;

namespace Thuc_hanh_WEB.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Book> Books { get; set; }
    }
}
