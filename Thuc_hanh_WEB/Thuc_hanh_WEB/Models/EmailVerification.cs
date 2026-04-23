using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thuc_hanh_WEB.Models
{
    [Table("EmailVerifications")]
    public class EmailVerification
    {
        [Key]
        [Column("Email")]
        [StringLength(200)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string VerifyCode { get; set; }

        public int FailedCount { get; set; }

        public DateTime ExpiredAt { get; set; }
    }
}
