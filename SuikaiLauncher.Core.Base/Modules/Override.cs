using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics.CodeAnalysis;
namespace SuikaiLauncher.Core.Override
{
    public static class StringExtensions
    {
        /// <summary>
        /// 检查某个字符串是否包含给定的字符串
        /// </summary>
        /// <param name="value">给定字符串</param>
        /// <param name="IgnoreCase">是否忽略大小写</param>
        /// <returns>bool</returns>
        public static bool ContainsF(this string? content, string value, bool IgnoreCase = true)
        {
            if (content is null || value is null) return false;
            if (IgnoreCase) return content.ToLower().Contains(value.ToLower());
            return content.Contains(value);
        }
        /// <summary>
        /// 验证字符串是否为空、<see langword="null"/> 或者只包含空格 
        /// </summary>
        public static bool IsNullOrWhiteSpaceF([NotNullWhen(false)] this string? content)
        {
            return string.IsNullOrWhiteSpace(content);
        }
        /// <summary>
        /// 将单词中的首字母大写
        /// </summary>
        public static string Capitalize(this string content)
        {
            if (!content.Contains(" ")) return content.Substring(0, 1).ToUpper() + content.Substring(1, content.Length - 1).ToLower();
            return string.Join(" ", content.Split(" ").Select((string Content) =>
            {
                return Content.Substring(0, 1).ToUpper() + Content.Substring(1, Content.Length - 1);
            }));
        }
        /// <summary>
        /// 正则表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>一个包含所有匹配文本的列表</returns>
        public static List<string> Regular(this string content, string expression)
        {
            var Match = Regex.Matches(content, expression);
            List<string> Result = new();
            foreach (string Value in Match)
            {
                Result.Add(Value);
            }
            return Result;
        }
        public static string Base64Encode(this string? content)
        {
            if (content.IsNullOrWhiteSpaceF()) throw new NullReferenceException("");
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
        }
        public static string Base64Decode(this string? content)
        {
            if (content.IsNullOrWhiteSpaceF()) throw new NullReferenceException("");
            return Encoding.UTF8.GetString(Convert.FromBase64String(content));
        }
        public static bool IsAscii(this string content)
        {
            if (content == string.Empty) return true;
            foreach (char c in content)
            {
                if (!char.IsAscii(c))
                {
                    return false;
                }
            }
            return true;
        }
    }
}