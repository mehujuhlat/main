using client.Models;
using System.Diagnostics;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;


namespace client.App_Code
{

    public class VisitorCount
    {
        public int Vip { get; set; }
        public int Normal { get; set; }
        public int Child { get; set; }
    }


    public static class AppSecrets
    {
        public static string EmailPassword { get; private set; } = null!;

        public static string SiteKey { get; private set; } = null!;
        public static string SecretKey { get; private set; } = null!;

        public static void Initialize(IConfiguration config)
        {
            SiteKey = config["captchasitekey"] ?? throw new Exception("captchasitekey missing");
            SecretKey = config["captchasecretkey"] ?? throw new Exception("captchasecretkey missing");
            EmailPassword = config["emailpassword"] ?? throw new Exception("emailpassword missing");
        }
    }


    public static class Helper
    {
            private static IWebHostEnvironment _env;

            public static void Initialize(IWebHostEnvironment env)
            {
            _env = env;
            }

        //public static string appUrl => _env.IsDevelopment() ? "https://localhost:7137" : "https://mehujuhlat-gmdyasgqb9ddbucj.swedencentral-01.azurewebsites.net";
        public static string appUrl => _env.IsDevelopment() ? "https://localhost:7137" : "https://porvoonmehujuhlat-crdqfrazafa8fseb.swedencentral-01.azurewebsites.net";


        public static bool IsNullableBitTrue(int? nullableInt, int bitPosition)
        {
            if (!nullableInt.HasValue || bitPosition < 0 || bitPosition > 31)
                return false;
            int value = nullableInt.Value;
            return (value & (1 << bitPosition)) != 0;
        }

        /*
        public static string GetSecret(string secretName)
        {
            var kvUri = $"https://mehujuhlat.vault.azure.net";
            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            KeyVaultSecret secret = client.GetSecret(secretName);
            return  secret.Value;
        }*/

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            var stringBuilder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(chars.Length);
                stringBuilder.Append(chars[index]);
            }

            return stringBuilder.ToString();
        }

        public static string GenerateRandomStringU(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            var stringBuilder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(chars.Length);
                stringBuilder.Append(chars[index]);
            }

            return stringBuilder.ToString();
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool sendMail(string email, string subject, string body)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient("mail.tupitek.fi", 587);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential("tuomas.kokki@tupitek.fi", AppSecrets.EmailPassword);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("tuomas.kokki@tupitek.fi");
                mailMessage.To.Add(email);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                smtpClient.Send(mailMessage);
                mailMessage.Dispose();
                Debug.WriteLine("sähköposti lähetetty " + email);
                return true;
            }
            catch (SmtpException ex)
            {
                Debug.WriteLine($"SMTP Virhe: {ex.StatusCode} - {ex.Message}");
                if (ex.InnerException != null)
                    Debug.WriteLine($"Sisäinen virhe: {ex.InnerException.Message}");
                return false;
            }

        }


    }

}
