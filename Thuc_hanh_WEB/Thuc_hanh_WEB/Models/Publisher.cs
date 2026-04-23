using System.Collections.Generic;

namespace Thuc_hanh_WEB.Models
{
    public class Publisher
    {
        public int PublisherID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public virtual ICollection<Book> Books { get; set; }
    }
}
