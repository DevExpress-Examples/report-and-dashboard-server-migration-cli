namespace RdsDataExportCliTool.Models
{
    internal class ExportOptions
    {
        public string ServerUrl { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ExportPath { get; set; } = string.Empty;
        public bool ExportByCategory { get; set; }
        public bool IntegrateIntoTheExample { get; set; }
    }
}
