using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; }
}
