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
                var options = new JsonSerializerOptions
                {
                    TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()
                };
                return JsonSerializer.Deserialize<T>(jsonText,options)
                       ?? throw new JsonException($"反序列化 Json 失败：无法转换为 {typeof(T).Name}");
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "无效的 Json 结构或转换失败");
                throw;
            }
        }
    }
}