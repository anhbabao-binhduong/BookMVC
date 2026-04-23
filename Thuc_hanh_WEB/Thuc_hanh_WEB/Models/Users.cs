using System;
using System.ComponentModel.DataAnnotations;

namespace Thuc_hanh_WEB.Models
{
    public class Users
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string Role { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
