using System.Text.RegularExpressions;

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
        public static bool ContainsF(this string content, string value, bool IgnoreCase = true)
        {
            if (content is null || value is null) return false;
            if (IgnoreCase) return content.ToLower().Contains(value.ToLower());
            return content.Contains(value);
        }
        public static bool IsNullOrWhiteSpaceF(this string? content){
            return string.IsNullOrWhiteSpace(content);
        }
        /// <summary>
        /// 将单词中的首字母大写
        /// </summary>
        public static string Capitalize(this string content)
        {
            return content.Substring(0, 1).ToUpper() + content.Substring(1, content.Length - 1).ToLower();
        }
        public static List<string> Regular(this string content,string expression){
            var Match = Regex.Matches(content,expression);
            List<string> Result = new();
            foreach (string Value in Match){
                Result.Add(Value);
            }
            return Result;
        }
    }
}