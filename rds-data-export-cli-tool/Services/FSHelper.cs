using System;
using System.IO;

namespace RdsDataExportCliTool.Services {
    internal static class FSHelper {
        private static readonly string[] ExportArtifacts = {
            "documents.json", "reports", "dashboards", "data-models", "users-and-roles"
        };

        public static void ClearExportArtifacts(string outputDir, IConsoleService? console = null) {
            foreach(string name in ExportArtifacts) {
                string path = Path.Combine(outputDir, name);
                try {
                    if(Directory.Exists(path))
                        Directory.Delete(path, recursive: true);
                    else if(File.Exists(path))
                        File.Delete(path);
                } catch(Exception ex) {
                    console?.WriteLine($"  [WARN] Could not delete '{name}': {ex.Message}");
                }
            }
        }

        public static void ClearDirectory(string dirPath, IConsoleService? console = null) {
            if(!Directory.Exists(dirPath)) return;
            var dir = new DirectoryInfo(dirPath);

            foreach(FileInfo file in dir.GetFiles()) {
                try { file.Delete(); } catch(Exception ex) {
                    console?.WriteLine($"  [WARN] Could not delete file '{file.Name}': {ex.Message}");
                }
            }

            foreach(DirectoryInfo subDir in dir.GetDirectories()) {
                try { subDir.Delete(recursive: true); } catch(Exception ex) {
                    console?.WriteLine($"  [WARN] Could not delete directory '{subDir.Name}': {ex.Message}");
                }
            }
        }

        public static void CopyDirectory(string sourceDir, string destinationDir) {
            var dir = new DirectoryInfo(sourceDir);
            if(!dir.Exists) return;

            Directory.CreateDirectory(destinationDir);

            foreach(FileInfo file in dir.GetFiles()) {
                file.CopyTo(Path.Combine(destinationDir, file.Name), overwrite: true);
            }

            foreach(DirectoryInfo subDir in dir.GetDirectories()) {
                CopyDirectory(subDir.FullName, Path.Combine(destinationDir, subDir.Name));
            }
        }

        public static string SanitizeFileName(string name) {
            foreach(char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

        public static string SanitizePathSegment(string segment) {
            foreach(char c in Path.GetInvalidFileNameChars())
                segment = segment.Replace(c, '_');
            return segment;
        }
    }
}
