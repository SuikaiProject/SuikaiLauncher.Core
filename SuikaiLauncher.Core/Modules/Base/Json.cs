#pragma warning disable CS8604

using System.Text.Json.Nodes;
using SuikaiLauncher.Core.Override;
namespace SuikaiLauncher.Core
{
    public class Json
    {
        public static JsonNode GetJson(string? JsonText)
        {
            try
            {
                if(JsonText.IsNullOrWhiteSpaceF()) throw new ArgumentNullException("参数不能为 null 或空");
                var Data = JsonNode.Parse(JsonText);
                if (Data is not null) return Data;
                throw new ArgumentException("格式化 Json 失败");
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "无效的 Json 结构");
                throw;
            }
        }
        public static List<string> GetJsonArray(JsonNode? jsonNode){
            List<string> DataList = new();
            try{
                
                if (jsonNode is null) return DataList;
                foreach (var Data in jsonNode.AsArray()){
                    if(Data is null) continue;
                    DataList.Add(Data.ToString());
                }
                return DataList;
            }catch{
                if(jsonNode is not null) DataList.Add(jsonNode.ToString());
                return DataList;
            }
        }
    }
}