namespace Rocket.Libraries.Emailing.Models
{
    public class EmailingSettings
    {
        public string TemplatesDirectory { get; set; }
        public bool IsDevelopment { get; set; }
        public string DevelopmentEmail { get; set; }
    }
}