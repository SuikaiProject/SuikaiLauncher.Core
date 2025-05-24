using System.Text;
using System.Security.Cryptography;

namespace SuikaiLauncher.Core.Base{
    public class FileIO
    {
        public async static Task<string> ReadAsString(Stream DataStream)
        {
            try
            {
                using (DataStream)
                using (StreamReader Reader = new(DataStream))
                {
                    return await Reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "[System] 读取流时出错");
                return string.Empty;
            }
        }
        public async static Task<string> ReadAsString(HttpResponseMessage Response)
        {
            try
            {
                using (Stream RespStream = await Response.Content.ReadAsStreamAsync())
                {
                    return await ReadAsString(RespStream);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "[System] 读取网络流时出错");
                return string.Empty;
            }
        }
        public async static Task WriteData(Stream DataStream, string FilePath, bool Append = true, CancellationToken? Token = null)
        {
            if (Token is not null) Token.Value.Register(() => throw new TaskCanceledException("操作已取消"));
            byte[] buffer = new byte[16384]; // 缓冲区
            try
            {
                using (FileStream fileStream = new(FilePath, (Append) ? FileMode.Append : FileMode.Create))
                {
                    if (DataStream.CanRead)
                    {
                        int bytesRead;
                        while ((bytesRead = await DataStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {

                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "[System] 写入数据时出错");
            }
        }
        public async static Task WriteData(HttpResponseMessage Response, string FilePath, CancellationToken? Token = null)
        {
            if (File.Exists(FilePath))
            {
                using (Stream RespStream = await Response.Content.ReadAsStreamAsync())
                {
                    await WriteData(RespStream, FilePath, Token: Token);
                }
            }
        }
        public static string GetDataHash(string data, string algorithm = "sha1")
        {

            using HashAlgorithm hasher = CreateHashAlgorithm(algorithm);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            byte[] hash = hasher.ComputeHash(bytes);

            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        public static async Task<string> GetFileHashAsync(string filePath, string algorithm = "sha1")
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("找不到指定的文件", filePath);

            using HashAlgorithm hasher = CreateHashAlgorithm(algorithm);
            using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 
                                                    bufferSize: 4096, useAsync: true);
            
            byte[] hash = await Task.Run(() => hasher.ComputeHash(stream));
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