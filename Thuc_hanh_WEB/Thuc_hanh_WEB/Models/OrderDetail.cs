using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thuc_hanh_WEB.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailID { get; set; }

        public int OrderID { get; set; }

        public int BookID { get; set; }

        public int Quantity { get; set; }

        [Column("Price")]
        public decimal UnitPrice { get; set; }

        // Navigation
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }

        [ForeignKey("BookID")]
        public virtual Book Book { get; set; }
    }
}