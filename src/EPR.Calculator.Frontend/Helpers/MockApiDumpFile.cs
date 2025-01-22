using System.Text.Json;
using Microsoft.Build.Framework;

namespace EPR.Calculator.Frontend.Helpers
{
    public static class MockApiDumpFile
    {
        static string path = "../MockApiCalls.txt";
        public static void WriteToFile(Object content)
        {

            var jsonstring = JsonSerializer.Serialize(content);
            if (!File.Exists(path))
            {
                File.Create(path);
            }

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("=======================================================");
                sw.WriteLine(jsonstring);
            }
        }
    }
}