namespace IO.Swagger.Helpers
{
    public class NavWebServiceReferenceOptions
    {
        public string NavWebServiceReference { get; set; }
        public string Domain { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ClientCredentialType { get; set; } = "Windows";
    }
}
