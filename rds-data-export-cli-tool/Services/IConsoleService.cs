namespace RdsDataExportCliTool.Services {
    internal interface IConsoleService {
        void Write(string message);
        void WriteLine(string message);
        void WriteLine();
        string ReadPassword(string prompt);
    }
}
