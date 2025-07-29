using System.Globalization;

namespace EPR.Calculator.Frontend.Extensions
{
    public static class TextInfoExtension
    {
        public static string ToFirstLetterCap(this TextInfo textInfo, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            if (text.Length <= 1)
            {
                return textInfo.ToUpper(text);
            }

            string lowercasetext = textInfo.ToLower(text);
            return $"{textInfo.ToUpper(lowercasetext.First().ToString())}{new string(lowercasetext.Skip(1).ToArray())}";
        }
    }
}