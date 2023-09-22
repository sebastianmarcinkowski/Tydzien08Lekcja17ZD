using Cipher;
using EmailSender;
using ReportServiceSMS.Core;
using ReportServiceSMS.Core.Repositories;
using SMSSender;
using System;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;

namespace ReportServiceSMS
{
    public partial class ReportServiceSMS : ServiceBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private int _sendHour;
        private static int _intervalInMinutes;
        private Timer _timer;
        private ErrorRepository _errorRepository = new ErrorRepository();
        private Email _email;
        private GenerateHtmlEmail _htmlEmail = new GenerateHtmlEmail();
        private string _emailReciver;
        private StringCipher _stringCipher = new StringCipher("5E0DE73E-14FA-49F5-92FF-B3284B645E48");
        private const string NonEncryptedPrefix = "encrypt:";
        private string smsApiKey;
        private SMS _sms;
        private string _smsSenderName;
        private string _smsReciverNumber;

        public ReportServiceSMS()
        {
            InitializeComponent();

            try
            {
                smsApiKey = DecryptSmsApiKey();
                _sms = new SMS(smsApiKey);

                _smsSenderName = ConfigurationManager.AppSettings["SMSSenderName"];
                _smsReciverNumber = ConfigurationManager.AppSettings["SMSReciverNumber"];

                _sendHour = Convert.ToInt32(ConfigurationManager.AppSettings["SendHours"]);
                _intervalInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalInMinutes"]);
                _timer = new Timer(_intervalInMinutes * 60000);

                _emailReciver = ConfigurationManager.AppSettings["ReciverEmail"];

                _email = new Email(new EmailParams
                {
                    HostSmtp = ConfigurationManager.AppSettings["HostSmtp"],
                    Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]),
                    EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]),
                    SenderName = ConfigurationManager.AppSettings["SenderName"],
                    SenderEmail = ConfigurationManager.AppSettings["SenderEmail"],
                    SenderEmailPassword = DecryptSenderEmailPassword()
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private string DecryptSenderEmailPassword()
        {
            var encryptedPassword = ConfigurationManager.AppSettings["SenderEmailPassword"];

            if (encryptedPassword.StartsWith(NonEncryptedPrefix))
            {
                encryptedPassword = _stringCipher
                    .Encrypt(encryptedPassword.Replace(
                        NonEncryptedPrefix, ""));

                var configFile = ConfigurationManager
                    .OpenExeConfiguration(ConfigurationUserLevel.None);

                configFile.AppSettings.Settings["SenderEmailPassword"].Value = encryptedPassword;

                configFile.Save();
            }

            return _stringCipher.Decrypt(encryptedPassword);
        }

        private string DecryptSmsApiKey()
        {
            var encryptedSmsApiKey = ConfigurationManager.AppSettings["SMSApiKey"];

            if (encryptedSmsApiKey.StartsWith(NonEncryptedPrefix))
            {
                encryptedSmsApiKey = _stringCipher
                    .Encrypt(encryptedSmsApiKey.Replace(
                        NonEncryptedPrefix, ""));

                var configFile = ConfigurationManager
                    .OpenExeConfiguration(ConfigurationUserLevel.None);

                configFile.AppSettings.Settings["SMSApiKey"].Value = encryptedSmsApiKey;

                configFile.Save();
            }

            return _stringCipher.Decrypt(encryptedSmsApiKey);
        }

        protected override void OnStart(string[] args)
        {
            _timer.Elapsed += DoWork;
            _timer.Start();
            Logger.Info("Service started...");
        }

        public async void DoWork(object sender, ElapsedEventArgs e)
        {
            try
            {
                await SendError();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private async Task SendError()
        {
            var errors = _errorRepository.GetLasErrors(_intervalInMinutes);

            if (errors == null || !errors.Any())
                return;

            await _email.Send(
                "Błędy w aplikacji",
                _htmlEmail.GenerateErrors(errors, _intervalInMinutes),
                _emailReciver);

            Logger.Info("Error sent (E-mail).");

            _sms.Send(
                _smsSenderName,
                _smsReciverNumber,
                "Wystąpił błąd w apliacji, sprawdź wiadomość na poczcie e-mail.");

            Logger.Info("Error info sent (SMS).");
        }

        protected override void OnStop()
        {
            Logger.Info("Service stopped...");
        }
    }
}