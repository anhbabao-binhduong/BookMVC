using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thuc_hanh_WEB.Models
{
    [Table("ShoppingCart")]
    public class ShoppingCart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartID { get; set; }

        public int UserID { get; set; }
        public int BookID { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }

        [ForeignKey("BookID")]
        public virtual Book Book { get; set; }
    }
}