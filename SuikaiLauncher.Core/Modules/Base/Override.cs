namespace SuikaiLauncher.Core.Override
{
    public static class StringExtensions
    {
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
    }
}