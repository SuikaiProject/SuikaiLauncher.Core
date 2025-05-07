namespace SuikaiLauncher.Core.Base{
    public class FileIO{
        public async static Task<string> ReadAsString(Stream DataStream){
            try{
                using(StreamReader Reader = new(DataStream)){
                    return await Reader.ReadToEndAsync();
                }
            }catch (Exception ex){
                Logger.Log(ex,"[System] 读取流时出错");
                return string.Empty;
            }
        }
        public async static Task<string> ReadAsString(HttpResponseMessage Response){
            try{
                using(Stream RespStream = await Response.Content.ReadAsStreamAsync()){
                    return await ReadAsString(RespStream);
                }
            }catch(Exception ex){
                Logger.Log(ex,"[System] 读取网络流时出错");
                return string.Empty;
            }
        }
        public async static Task WriteData(Stream DataStream, string FilePath, bool Append)
        {
            byte[] buffer = new byte[16384]; // 16KB 缓冲区
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
        public async static Task WriteData(HttpResponseMessage Response,string FilePath){
            if (File.Exists(FilePath)){
                using(Stream RespStream = await Response.Content.ReadAsStreamAsync()){
                    
                }
            }
        }
    }
}