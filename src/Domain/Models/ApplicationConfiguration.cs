namespace LetsWork.Domain.Models
{
    public class ConfigSettings
    {
        public JWT JWT { get; set; }
        public SQLProvider SQLProvider { get; set; }
        public AzureBlobStorage AzureBlobStorage { get; set; }
        public GoogleAuth GoogleAuth { get; set; }
        public GoogleSMTPSettings GoogleSMTPSettings { get; set; }
        public string Environment { get; set; }
        public URL URL { get; set; }
    }

    public class JWT
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
    }

    public class SQLProvider
    {
        public string ConnectionString { get; set; }
    }

    public class AzureBlobStorage
    {
        public string ConnectionString { get; set; }
    }

    public class GoogleAuth
    {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
    }

    public class GoogleSMTPSettings
    {
        public int Port { get; set; }
        public string Host { get; set; }
        public int Timeout { get; set; }
        public string FromEmail { get; set; }
        public string FromPassword { get; set; }
    }

    public class URL
    {
        public string ChangePasswordURL { get; set; }
        public string ForgotPasswordURL { get; set; }
        public string VerifyEmailURL { get; set; }
    }
}
