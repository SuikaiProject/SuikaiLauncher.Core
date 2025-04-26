namespace SuikaiLauncher.Core.Override
{
    public static class StringExtensions
    {
        public static bool ContainsF(this string content, string value, bool IgnoreCase = true)
        {
            if (content is null || value is null) return false;
            string compstr = content;
            string compstr2 = value;
            if (IgnoreCase) return compstr.ToLower().Contains(value.ToLower());
            return compstr.Contains(value);
        }
    }
}