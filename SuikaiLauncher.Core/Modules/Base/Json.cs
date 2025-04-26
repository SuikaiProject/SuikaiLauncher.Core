using System.Text.Json.Nodes;
namespace SuikaiLauncher.Core
{
    public class Json
    {
        public static JsonNode GetJson(string JsonText)
        {
            return JsonNode.Parse(JsonText);
        }
        
        
        
    }
}