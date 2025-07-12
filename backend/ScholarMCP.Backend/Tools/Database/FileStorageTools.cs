using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace ScholarMCP.Tools.Database
{
    [McpServerToolType]
    public static class FileStorageTools
    {
        /// <summary>
        /// Save an uploaded file to a specified storage path
        /// 保存上传的文件到指定存储路径
        /// アップロードされたファイルを指定の保存先に保存
        /// </summary>
        /// <param name="fileName">File name (with extension)</param>
        /// <param name="base64Content">File content as base64 string</param>
        /// <param name="storageDir">Storage directory (default: ./rawfile)</param>
        /// <returns>Result JSON with file path or error</returns>
        [McpServerTool, Description("Save an uploaded file (base64) to a storage directory. Returns the saved file path.")]
        public static string SaveUploadedFile(
            [Description("File name, including extension (e.g. 'data.txt')")] string fileName,
            [Description("File content as base64 string")] string base64Content,
            [Description("Storage directory, default is './rawfile'")] string storageDir = "./rawfile")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(base64Content))
                {
                    return JsonSerializer.Serialize(new
                    {
                        success = false,
                        error = "File name and content must not be empty.",
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                    }, new JsonSerializerOptions { WriteIndented = true });
                }

                // Ensure storage directory exists
                if (!Directory.Exists(storageDir))
                    Directory.CreateDirectory(storageDir);

                var filePath = Path.Combine(storageDir, fileName);
                var fileBytes = Convert.FromBase64String(base64Content);
                File.WriteAllBytes(filePath, fileBytes);

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    filePath = Path.GetFullPath(filePath),
                    fileName,
                    size = fileBytes.Length,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                }, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = $"Failed to save file: {ex.Message}",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                }, new JsonSerializerOptions { WriteIndented = true });
            }
        }
    }
} 