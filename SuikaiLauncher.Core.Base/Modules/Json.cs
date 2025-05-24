#pragma warning disable CS8604

using System.Text.Json;
using System.Text.Json.Nodes;
using SuikaiLauncher.Core.Override;
namespace SuikaiLauncher.Core
{
    public class Json
    {
        public static T GetJson<T>(string? jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
                throw new ArgumentNullException(nameof(jsonText), "参数不能为 null 或空");

            try
            {
                
                var node = JsonNode.Parse(jsonText)
                           ?? throw new ArgumentException("格式化 Json 失败", nameof(jsonText));

                
                return node.Deserialize<T>()
                       ?? throw new JsonException($"无法将 JSON 反序列化为 {typeof(T).Name}");
            }
            catch (Exception ex) when (
                ex is JsonException || ex is ArgumentException || ex is ArgumentNullException)
            {
                Logger.Log(ex, "无效的 Json 结构或转换失败");
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