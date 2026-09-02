namespace RdsDataExportCliTool.Models {
    internal class DataModelListModel {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public int? CommandTimeout { get; set; }
    }
}
