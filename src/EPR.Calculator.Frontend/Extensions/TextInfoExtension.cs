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

            string lowercaseText = textInfo.ToLower(text);
            char firstChar = textInfo.ToUpper(lowercaseText[0].ToString())[0];

            return firstChar + lowercaseText.Substring(1);
        }
    }
}