using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Thuc_hanh_WEB.Models;

namespace Thuc_hanh_WEB.Helpers
{
    public static class EmailHelper
    {
        private static string FromEmail =>
            ConfigurationManager.AppSettings["EmailFrom"] ?? string.Empty;

        private static string AppPassword =>
            ConfigurationManager.AppSettings["EmailPassword"] ?? string.Empty;

        private static string EmailHost =>
            ConfigurationManager.AppSettings["EmailHost"] ?? "smtp.gmail.com";

        private static int EmailPort
        {
            get
            {
                int port;
                return int.TryParse(ConfigurationManager.AppSettings["EmailPort"], out port) ? port : 587;
            }
        }

        // ================================================================
        // 1) Gửi mã xác thực đăng ký
        // ================================================================
        public static void Send(string toEmail, string code)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return;

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

        // ================================================================
        // 2) Gửi link đặt lại mật khẩu
        // ================================================================
        public static void Send(string toEmail, string subject, string resetUrl)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return;

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

        // ================================================================
        // 3) Gửi email xác nhận đơn hàng
        // ================================================================
        public static void SendOrderConfirmation(string toEmail, Order order)
        {
            if (string.IsNullOrWhiteSpace(toEmail) || order == null) return;

            string orderItemsHtml;

            if (order.OrderDetails != null && order.OrderDetails.Any())
            {
                orderItemsHtml = string.Join("", order.OrderDetails.Select(item => $@"
                    <tr>
                        <td style='padding:10px;border-bottom:1px solid #e2e8f0'>{(item.Book != null ? item.Book.Title : "Sách")}</td>
                        <td style='padding:10px;border-bottom:1px solid #e2e8f0;text-align:center'>{item.Quantity}</td>
                        <td style='padding:10px;border-bottom:1px solid #e2e8f0;text-align:right'>{item.UnitPrice:N0} đ</td>
                    </tr>"));
            }
            else
            {
                orderItemsHtml = @"
                    <tr>
                        <td colspan='3' style='padding:10px;border-bottom:1px solid #e2e8f0;color:#64748b'>
                            (Không có chi tiết sản phẩm)
                        </td>
                    </tr>";
            }

            string paymentMethod = string.IsNullOrWhiteSpace(order.PaymentMethod) ? "COD" : order.PaymentMethod;

            SendMail(
                toEmail,
                subject: $"Xác nhận đơn hàng #{order.OrderID} - BookStore",
                body: $@"
                <div style='font-family:Segoe UI,sans-serif;max-width:640px;margin:auto;padding:32px;background:#f8fafc;border-radius:12px;border:1px solid #e2e8f0'>
                    <h2 style='color:#1e40af;margin-bottom:8px'>📚 BookStore</h2>
                    <p style='color:#475569;margin-bottom:24px'>Cảm ơn bạn đã đặt hàng. Đơn hàng của bạn đã được ghi nhận thành công.</p>

                    <div style='background:#fff;border-radius:8px;padding:20px;border:1px solid #e2e8f0;margin-bottom:20px'>
                        <p style='margin:0 0 8px'><strong>Mã đơn hàng:</strong> #{order.OrderID}</p>
                        <p style='margin:0 0 8px'><strong>Khách hàng:</strong> {order.FullName}</p>
                        <p style='margin:0 0 8px'><strong>Số điện thoại:</strong> {order.Phone}</p>
                        <p style='margin:0 0 8px'><strong>Địa chỉ nhận hàng:</strong> {order.Address}</p>
                        <p style='margin:0 0 8px'><strong>Phương thức thanh toán:</strong> {paymentMethod}</p>
                        <p style='margin:0'><strong>Tổng thanh toán:</strong> <span style='color:#dc2626;font-weight:700'>{order.TotalAmount:N0} đ</span></p>
                    </div>

                    <table style='width:100%;border-collapse:collapse;background:#fff;border:1px solid #e2e8f0;border-radius:8px;overflow:hidden'>
                        <thead>
                            <tr style='background:#eff6ff'>
                                <th style='padding:12px;text-align:left'>Sản phẩm</th>
                                <th style='padding:12px;text-align:center'>SL</th>
                                <th style='padding:12px;text-align:right'>Đơn giá</th>
                            </tr>
                        </thead>
                        <tbody>
                            {orderItemsHtml}
                        </tbody>
                    </table>

                    <p style='color:#94a3b8;font-size:12px;margin-top:24px;text-align:center'>
                        Nếu bạn cần hỗ trợ, vui lòng liên hệ BookStore.
                    </p>
                </div>",
                isHtml: true
            );
        }

        // ================================================================
        // Hàm gửi mail dùng chung
        // ================================================================
        private static void SendMail(string toEmail, string subject, string body, bool isHtml = false)
        {
            if (string.IsNullOrWhiteSpace(FromEmail))
                throw new InvalidOperationException("Missing EmailFrom in Web.config appSettings.");
            if (string.IsNullOrWhiteSpace(AppPassword))
                throw new InvalidOperationException("Missing EmailPassword in Web.config appSettings.");

            using (var mail = new MailMessage())
            {
                mail.To.Add(toEmail);
                mail.From = new MailAddress(FromEmail, "BookStore");
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = isHtml;

                using (var smtp = new SmtpClient(EmailHost, EmailPort))
                {
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(FromEmail, AppPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }
    }
}