namespace EPR.Calculator.Frontend.Constants
{
    /// <summary>
    /// List of static helpers.
    /// </summary>
    public static class StaticHelpers
    {
        /// <summary>
        /// Static helper for alphanumeric.
        /// </summary>
        public const string AlphaNumeric = "^[a-zA-Z0-9 ]*$";

        /// <summary>
        /// Static helper for file name.
        /// </summary>
        public const string FileName = "test.csv";

        /// <summary>
        /// Static helper for maximum file size.
        /// </summary>
        public const long MaxFileSize = 50 * 1024; // 50 KB

        /// <summary>
        /// Static helper for media type.
        /// </summary>
        public const string MediaType = "application/json";

        /// <summary>
        /// Static helper for mime type.
        /// </summary>
        public const string MimeType = "test/csv";

        /// <summary>
        /// Static helper for path.
        /// </summary>
        public const string Path = @"wwwroot/assets/SchemeParameterTemplates/SchemeParameterTemplate.v1.1.xlsx";
    }
}
