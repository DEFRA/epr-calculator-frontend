namespace EPR.Calculator.Frontend.Constants
{
    public class StaticHelpers
    {
        public const string FileNotSelected = "Please select a file";
        public const string FileMustBeCSV = "Your file must have the extension .csv";
        public const string FileNotExceed50KB = "The CSV must be smaller than 50KB";
        public const string Path = @"wwwroot/assets/SchemeParameterTemplates/SchemeParameterTemplate.v1.1.xlsx";
        public const string FileName = "test.csv";
        public const string MimeType = "test/csv";
        public const long MaxFileSize = 50 * 1024; // 50 KB
        public const string MediaType = "application/json";
        public const long MaxUploadFileSize = 2 * 1024 * 1024; // 2MB
        public const string UploadFileNotExceed2MB = "The CSV must be smaller than 2MB";
    }
}
