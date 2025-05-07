using System.Linq.Expressions;
using System.Text.Json.Nodes;
namespace SuikaiLauncher.Core
{
    public class Json
    {
        public static JsonNode? GetJson(string JsonText)
        {
            try
            {
                return JsonNode.Parse(JsonText);
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "无效的 Json 结构");
                throw;
            }



        }
    }
}