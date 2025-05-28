using System.Text;
using System.Security.Cryptography;
using SuikaiLauncher.Core.Override;
using System.Net;

namespace SuikaiLauncher.Core.Base
{
    public class FileIO
    {
        public static async Task<string> ReadAsString(Stream dataStream)
        {
            try
            {
                using var reader = new StreamReader(dataStream, leaveOpen: false);
                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "[System] 读取流时出错");
                return string.Empty;
            }
        }

        public static async Task<string> ReadAsString(HttpResponseMessage response)
        {
            try
            {
                await using var respStream = await response.Content.ReadAsStreamAsync();
                return await ReadAsString(respStream);
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "[System] 读取网络流时出错");
                return string.Empty;
            }
        }
        public static async Task<string> ReadAsString(WebResponse Response)
        {
            using (Stream WebStream = Response.GetResponseStream())
            {
                return await ReadAsString(WebStream);
            }
        }

        public static async Task WriteData(Stream dataStream, string filePath, bool append = true, CancellationToken? token = null)
        {
            if (token is not null)
            {
                token.Value.Register(() => throw new TaskCanceledException("操作已取消"));
            }

            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!directory.IsNullOrWhiteSpaceF() && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var fileMode = append ? FileMode.Append : FileMode.Create;
                await using var fileStream = new FileStream(filePath, fileMode, FileAccess.Write, FileShare.None, 8192, useAsync: true);
                await dataStream.CopyToAsync(fileStream, 8192, token ?? CancellationToken.None);
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "[System] 写入数据时出错");
            }
        }

        public static async Task WriteData(HttpResponseMessage response, string filePath, CancellationToken? token = null)
        {
            try
            {
                await using var respStream = await response.Content.ReadAsStreamAsync();
                await WriteData(respStream, filePath, append: true, token: token);
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "[System] 写入数据时出错");
            }
        }

        public static string GetDataHash(string data, string algorithm = "sha1")
        {
            using var hasher = CreateHashAlgorithm(algorithm);
            var bytes = Encoding.UTF8.GetBytes(data);
            var hash = hasher.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        public static async Task<byte[]> ReadBytes(Stream DataStream)
        {
            using (MemoryStream memoryStream = new())
            {
                await DataStream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
        public static async Task<byte[]> ReadBytes(string FilePath)
        {
            if (!File.Exists(FilePath)) throw new FileNotFoundException("未找到目标文件");
            using (FileStream fileStream = new(FilePath, FileMode.Open, FileAccess.Read, FileShare.None, 8192, true))
            {
                return await ReadBytes(fileStream);
            }
        }
        public static 

        public static async Task<string> GetFileHashAsync(string filePath, string algorithm = "sha1")
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("找不到指定的文件", filePath);

            using var hasher = CreateHashAlgorithm(algorithm);
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            var hash = await Task.Run(() => hasher.ComputeHash(stream));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static HashAlgorithm CreateHashAlgorithm(string algorithm)
        {
            return algorithm.ToLowerInvariant() switch
            {
                "sha1" or "sha128" => SHA1.Create(),
                "sha2" or "sha256" => SHA256.Create(),
                "sha512" => SHA512.Create(),
                "md5" => MD5.Create(),
                _ => throw new ArgumentException($"不支持的哈希算法: {algorithm}")
            };
        }
    }
}