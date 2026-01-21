namespace MinimartApi.Configurations
{
    public class EmailOptions
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; }
        public bool EnableSSL { get; set; }
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string From { get; set; } = default!;
    }
}
