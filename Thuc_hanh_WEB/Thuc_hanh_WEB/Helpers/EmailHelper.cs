using System.Net;
using System.Net.Mail;

public class EmailHelper
{
    private const string FromEmail = "sidat3241@gmail.com";
    private const string AppPassword = "orzs unia sklp muxf";

    // Overload 1: Gửi mã xác thực đăng ký
    public static void Send(string toEmail, string code)
    {
        SendMail(
            toEmail,
            subject: "Mã xác nhận đăng ký BookStore",
            body: $@"
                <div style='font-family:Segoe UI,sans-serif;max-width:480px;margin:auto;padding:32px;background:#f8fafc;border-radius:12px;border:1px solid #e2e8f0'>
                    <h2 style='color:#1e40af;margin-bottom:8px'>📚 BookStore</h2>
                    <p style='color:#475569;margin-bottom:24px'>Xác nhận đăng ký tài khoản</p>
                    <div style='background:#fff;border-radius:8px;padding:24px;text-align:center;border:1px solid #e2e8f0'>
                        <p style='color:#64748b;font-size:14px;margin-bottom:12px'>Mã xác nhận của bạn là:</p>
                        <div style='font-size:36px;font-weight:700;letter-spacing:8px;color:#1e40af'>{code}</div>
                        <p style='color:#94a3b8;font-size:12px;margin-top:12px'>Mã có hiệu lực trong 5 phút</p>
                    </div>
                    <p style='color:#94a3b8;font-size:12px;margin-top:24px;text-align:center'>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này.</p>
                </div>",
            isHtml: true
        );
    }

    // Overload 2: Gửi link đặt lại mật khẩu
    public static void Send(string toEmail, string subject, string resetUrl)
    {
        SendMail(
            toEmail,
            subject: subject,
            body: $@"
                <div style='font-family:Segoe UI,sans-serif;max-width:480px;margin:auto;padding:32px;background:#f8fafc;border-radius:12px;border:1px solid #e2e8f0'>
                    <h2 style='color:#1e40af;margin-bottom:8px'>📚 BookStore</h2>
                    <p style='color:#475569;margin-bottom:24px'>Yêu cầu đặt lại mật khẩu</p>
                    <div style='background:#fff;border-radius:8px;padding:24px;text-align:center;border:1px solid #e2e8f0'>
                        <p style='color:#64748b;font-size:14px;margin-bottom:20px'>Nhấn vào nút bên dưới để đặt lại mật khẩu của bạn:</p>
                        <a href='{resetUrl}'
                           style='display:inline-block;padding:12px 32px;background:#1e40af;color:#fff;text-decoration:none;border-radius:8px;font-weight:600;font-size:15px'>
                            Đặt lại mật khẩu
                        </a>
                        <p style='color:#94a3b8;font-size:12px;margin-top:16px'>Link có hiệu lực trong 15 phút</p>
                    </div>
                    <p style='color:#94a3b8;font-size:12px;margin-top:24px;text-align:center'>Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.</p>
                </div>",
            isHtml: true
        );
    }

    // Hàm gửi mail dùng chung
    private static void SendMail(string toEmail, string subject, string body, bool isHtml = false)
    {
        var mail = new MailMessage();
        mail.To.Add(toEmail);
        mail.From = new MailAddress(FromEmail, "BookStore");
        mail.Subject = subject;
        mail.Body = body;
        mail.IsBodyHtml = isHtml;

        var smtp = new SmtpClient("smtp.gmail.com", 587);
        smtp.Credentials = new NetworkCredential(FromEmail, AppPassword);
        smtp.EnableSsl = true;
        smtp.Send(mail);
    }
}