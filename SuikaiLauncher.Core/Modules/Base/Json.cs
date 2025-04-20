using System.Text.Json;

namespace SuikaiLauncher.Core
{
    public class Json
    {
        public static JsonDocument GetJson(string content)
        {
            return JsonDocument.Parse(content);
        }
        public static string GetJsonText(dynamic content)
        {
            return JsonSerializer.Deserialize(content);
        }
        
    }
}