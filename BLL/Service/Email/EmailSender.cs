using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Rovaya.BLL.Service.Email
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // إعداد بيانات السيرفر والحساب
            var smtpServer = "smtp.gmail.com"; 
            var port = 587;
            var senderEmail = "mohmmadsabbah123@gmail.com"; // يجب أن يكون إيميل أوفيس/أوتلوك يطابق الحساب
            var password = "biml slqe mcce ajgm"; // كلمة المرور الخاص بك أو App Password

            using (var client = new SmtpClient(smtpServer, port))
            {
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(senderEmail, password);

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(senderEmail);
                    mailMessage.To.Add(email);
                    mailMessage.Subject = subject;
                    mailMessage.Body = htmlMessage;
                    mailMessage.IsBodyHtml = true; 

                    await client.SendMailAsync(mailMessage);
                }
            }
        }
    }
}