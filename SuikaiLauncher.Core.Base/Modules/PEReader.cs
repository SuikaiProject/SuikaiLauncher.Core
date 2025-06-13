using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;


namespace SuikaiLauncher.Core.Base
{
    // 通用 PE 头读取器
    public class PEReader
    {
        private PEHeaders? Headers;
        private FileStream? FileReadStream;
        public void OpenFile(string FilePath) 
        {
            try
            {
                if (!File.Exists(FilePath)) throw new FileNotFoundException("未找到指定文件");
                using (this.FileReadStream = new(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Headers = new PEHeaders(this.FileReadStream);
                }
            }catch (BadImageFormatException ex)
            {
                this.OnFailed(ex);
            }catch (InvalidOperationException ex)
            {
                this.OnFailed(ex);
            }
        }
        public void OnFailed(Exception ex)
        {
            Logger.Log(ex, "[Runtime] 读取文件 PE 头失败。");
            throw new TaskCanceledException("此 PE 文件的格式无效");
        }
    }
}
