using SuikaiLauncher.Core.Override;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Asn1;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Net;
using System.Formats.Tar;
using System.Security.Cryptography;
using System.Text;

namespace SuikaiLauncher.Core.Base
{

    public class Murmur2 : HashAlgorithm
    {
        public int seed = 1;
        const uint m = 0x5bd1e995;
        const int r = 24;
        private uint Result;
        public override void Initialize()
        {
            return;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            return;
        }

        protected override byte[] HashFinal()
        {
            return new byte[1];
        }
        public Murmur2 ComputeHash(Stream FileReadStream)
        {
            long length = FileReadStream.Length;
            uint h = (uint)(seed ^ uint.Parse(length.ToString()));

            byte[] buffer = new byte[4];
            int read;
            while ((read = FileReadStream.Read(buffer, 0, 4)) == 4)
            {
                uint k = BitConverter.ToUInt32(buffer, 0);

                k *= m;
                k ^= k >> r;
                k *= m;

                h *= m;
                h ^= k;
            }
            // Handle the last few bytes of the input array
            switch (length & 3)
            {
                case 3:
                    h ^= (uint)(buffer[2] << 16);
                    goto case 2;
                case 2:
                    h ^= (uint)(buffer[1] << 8);
                    goto case 1;
                case 1:
                    h ^= buffer[0];
                    h *= m;
                    break;
            }

            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;

            this.Result = h;
            return this;
        }
        public uint GetResult()
        {
            return this.Result;
        }
    }
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
            if (hasher is Murmur2)
            {
                
            }
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
       

        public static async Task<string> GetFileHashAsync(string filePath, string algorithm = "sha1")
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("找不到指定的文件", filePath);
            
            using var hasher = CreateHashAlgorithm(algorithm);
            
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            if (hasher is Murmur2)
            {
                ((Murmur2)hasher).ComputeHash(stream).GetResult();
            }
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
    /// <summary>
    /// 可以用 Using 的压缩文件管理器
    /// </summary>
    public class ArchiveFile : IDisposable
    {
        private bool _dispose;
        private string FilePath;

        private dynamic? Handler;
        private GZipStream? DataStream;
        public bool disposed
        {
            get { return _dispose; }
            set { throw new InvalidOperationException("不能为 disposed 属性赋值"); }
        }
        private readonly object ChangeLock = new object[1];
        ~ArchiveFile()
        {
            Dispose();
        }

        public void Dispose()
        {
            lock (ChangeLock)
            {
                _dispose = true;
            }
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FilePath"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public ArchiveFile(string FilePath)
        {
            if (!File.Exists(FilePath)) throw new FileNotFoundException("未找到指定文件");
            using (FileStream FileReadStream = new(FilePath,FileMode.Open,FileAccess.Read,FileShare.Read,8192,true)) {
                byte[] FileHeaders = new byte[8];
                // 读取文件的前 8 个字节
                FileReadStream.Read(FileHeaders,0,8);
                // 将文件流指针移动到开头避免因为缺失文件头导致解压失败
                FileReadStream.Seek(0, SeekOrigin.Begin);
                // zip，但是空文件
                if (FileHeaders[0] == 0x50 && FileHeaders[1] == 0x4b && FileHeaders[2] == 0x05 && FileHeaders[4] == 0x06) throw new InvalidDataException("此 ZIP 文件为空");
                // zip
                else if (FileHeaders[0] == 0x50 && FileHeaders[1] == 0x4b) Handler = new ZipArchive(FileReadStream);
                // gzip
                else if (FileHeaders[0] == 0x1f && FileHeaders[1] == 0x8b)
                {
                    GZipStream Gzip = new(FileReadStream, CompressionLevel.Fastest);
                    try {
                        // 验证是否是 tar.gz
                        Handler = new TarReader(Gzip);
                    }
                    catch
                    {
                        throw new NotSupportedException("不支持此压缩文件格式");
                    }
                }
                
            }
        }
        public async Task ReadFile(string ArchiveEntry,Stream OutputStream)
        {
            if (this.Handler is not null && this.Handler is ZipArchive)
            {
                using (Stream? ReadStream = ((ZipArchive)this.Handler).GetEntry(ArchiveEntry)?.Open()) 
                {
                    if (ReadStream is null) return;
                    await ReadStream.CopyToAsync(OutputStream);
                }
            }
            else if (this.Handler is not null && this.Handler is TarReader)
            {
                while (true)
                {
                    TarEntry? Entry = await ((TarReader)this.Handler).GetNextEntryAsync();
                    if (Entry is null) break;
                    if (Entry.Name == ArchiveEntry && Entry.DataStream is not null) await Entry.DataStream.CopyToAsync(OutputStream);
                }
            }
            

        }
        public async Task WriteFile(string ArchiveEntry,Stream FileReadStream)
        {
            if(this.Handler is not null && this.Handler is ZipArchive)
            {
                using(Stream WriteStream = ((ZipArchive)this.Handler).CreateEntry(ArchiveEntry).Open())
                {
                    await FileReadStream.CopyToAsync(WriteStream);
                }
            }
            else
            {
                if (DataStream is null) throw new InvalidOperationException("此流不可写，因为其实参为 null");
                using (TarWriter Writer = new TarWriter(this.DataStream!))
                {
                    UstarTarEntry Entry = new(TarEntryType.RegularFile, ArchiveEntry);
                    Entry.DataStream = new MemoryStream();
                    await FileReadStream.CopyToAsync(Entry.DataStream);
                    Writer.WriteEntry(Entry);
                }
            }
        }
    }
}