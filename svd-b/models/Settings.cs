namespace SVD.Models
{
    public class Settings
    {
        public int AppId { get; set; }
        public string AppGuid { get; set; }
        public string AppSigla { get; set; }
        public string AppNombre { get; set; }
        public string TokenIssuer { get; set; }
        public string TokenAudience { get; set; }
        public string TokenSigningKey { get; set; }
    }
}
