using EmailSender.Extensions;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace EmailSender
{
    public class Email
    {
        private SmtpClient _smtp;
        private MailMessage _mail;

        private string _hostSmtp;
        private bool _enableSsl;
        private int _port;
        private string _senderEmail;
        private string _senderEmailPassword;
        private string _senderName;

        public Email(EmailParams emailParams)
        {
            _hostSmtp = emailParams.HostSmtp;
            _enableSsl = emailParams.EnableSsl;
            _port = emailParams.Port;
            _senderEmail = emailParams.SenderEmail;
            _senderEmailPassword = emailParams.SenderEmailPassword;
            _senderName = emailParams.SenderName;
        }

        public async Task Send(string subject, string body, string to)
        {
            _mail = new MailMessage();
            _mail.SubjectEncoding = Encoding.UTF8;
            _mail.BodyEncoding = Encoding.UTF8;
            _mail.From = new MailAddress(_senderEmail, _senderName);
            _mail.To.Add(new MailAddress(to));
            _mail.Subject = subject;
            _mail.IsBodyHtml = true;

            _mail.AlternateViews.Add(
                AlternateView.CreateAlternateViewFromString(
                    body.StripHtml(),
                    null,
                    MediaTypeNames.Text.Plain));

            _mail.AlternateViews.Add(
                AlternateView.CreateAlternateViewFromString(
                    $@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <div style='font-size: 16px;
                                        padding: 10px;
                                        font-family: Arial;
                                        line-height: 1.4;'>
                                    {body}
                                </div>
                            </body>
                        </html>
                    ",
                    null,
                    MediaTypeNames.Text.Html));

            _smtp = new SmtpClient
            {
                Host = _hostSmtp,
                EnableSsl = _enableSsl,
                Port = _port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_senderEmail, _senderEmailPassword)
            };

            _smtp.SendCompleted += OnSendCompleted;

            await _smtp.SendMailAsync(_mail);

            _smtp.SendCompleted -= OnSendCompleted;
        }

        private void OnSendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            _smtp.Dispose();
            _mail.Dispose();
        }
    }
}
