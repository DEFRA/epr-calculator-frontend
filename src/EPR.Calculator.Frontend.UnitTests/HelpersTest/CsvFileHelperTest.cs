using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Http;

namespace EPR.Calculator.Frontend.UnitTests.HelpersTest
{
    [TestClass]
    public class CsvFileHelperTest
    {
        [TestMethod]
        public void CsvFileHelperTest_Validate_CSV_Test()
        {
            var content = MockData.GetSchemeParametersFileContent();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "SchemeParameters.csv");

            var test = CsvFileHelper.ValidateCSV(file);
            Assert.IsNotNull(test);
        }

        [TestMethod]
        public void CsvFileHelperTest_Upload_No_File_Test()
        {
            var content = MockData.GetSchemeParametersFileContent();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "SchemeParameters.csv");

            var result = CsvFileHelper.ValidateCSV(null);
            Assert.IsNotNull(result);
            Assert.AreEqual(StaticHelpers.FileNotSelected, result.ErrorMessage);
        }

        [TestMethod]
        public void CsvFileHelperTest_Upload_File_Max_Size_Test()
        {
            var content = MockData.GetSchemeParametersFileContent();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            for (int i = 0; i < 15; i++)
            {
                writer.Write(content);
            }

            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "SchemeParameters.csv");

            var result = CsvFileHelper.ValidateCSV(file);
            Assert.IsNotNull(result);
            Assert.AreEqual(StaticHelpers.FileNotExceed50KB, result.ErrorMessage);
        }

        [TestMethod]
        public void CsvFileHelperTest_Upload_Not_CSV_Test()
        {
            var content = MockData.GetSchemeParametersFileContent();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "SchemeParameters.xlsx");

            var result = CsvFileHelper.ValidateCSV(file);
            Assert.IsNotNull(result);
            Assert.AreEqual(StaticHelpers.FileMustBeCSV, result.ErrorMessage);
        }

        [TestMethod]
        public void CsvFileHelperTest_Prepare_Upload__CSV_Test()
        {
            var content = MockData.GetSchemeParametersFileContent();
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(content));
            var writer = new StreamWriter(stream);
            writer.Write(content);
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "SchemeParameters.csv");

            var result = CsvFileHelper.PrepareDataForUpload(null);
            Assert.IsNotNull(result);
        }
    }
}
