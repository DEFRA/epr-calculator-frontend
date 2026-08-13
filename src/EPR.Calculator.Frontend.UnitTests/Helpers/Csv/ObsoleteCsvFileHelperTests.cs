using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers.Csv;
using Microsoft.AspNetCore.Http;

namespace EPR.Calculator.Frontend.UnitTests.Helpers.Csv;

[TestClass]
public class ObsoleteCsvFileHelperTests
{
    [TestMethod]
    public void TryValidateFile_NullFile_ReturnsFileNotSelected()
    {
        var isValid = CsvFileHelper.TryValidateFile(null, out var errors);

        Assert.IsFalse(isValid);
        CollectionAssert.AreEqual(new[] { ErrorMessages.FileNotSelected }, errors.ToArray());
    }

    [TestMethod]
    public void TryValidateFile_NonCsvExtension_ReturnsFileMustBeCsv()
    {
        var file = CreateFormFile("data.txt", "country,material,total_cost\n");

        var isValid = CsvFileHelper.TryValidateFile(file, out var errors);

        Assert.IsFalse(isValid);
        CollectionAssert.AreEqual(new[] { ErrorMessages.FileMustBeCSV }, errors.ToArray());
    }

    [TestMethod]
    public void TryValidateFile_CsvExtensionIsCaseInsensitive()
    {
        var file = CreateFormFile("data.CSV", "country,material,total_cost\n");

        var isValid = CsvFileHelper.TryValidateFile(file, out var errors);

        Assert.IsTrue(isValid);
        Assert.AreEqual(0, errors.Count);
    }

    [TestMethod]
    public void TryValidateFile_FileTooLarge_ReturnsSizeError()
    {
        var content = new string('x', (int)StaticHelpers.MaxFileSize + 1);
        var file = CreateFormFile("data.csv", content);

        var isValid = CsvFileHelper.TryValidateFile(file, out var errors);

        Assert.IsFalse(isValid);
        CollectionAssert.AreEqual(new[] { ErrorMessages.FileNotExceed50KB }, errors.ToArray());
    }

    [TestMethod]
    public void TryValidateFile_NonCsvAndTooLarge_ReturnsBothErrors()
    {
        var content = new string('x', (int)StaticHelpers.MaxFileSize + 1);
        var file = CreateFormFile("data.txt", content);

        var isValid = CsvFileHelper.TryValidateFile(file, out var errors);

        Assert.IsFalse(isValid);
        CollectionAssert.AreEqual(
            new[] { ErrorMessages.FileMustBeCSV, ErrorMessages.FileNotExceed50KB },
            errors.ToArray());
    }

    [TestMethod]
    public void TryValidateFile_ValidCsv_ReturnsTrue()
    {
        var file = CreateFormFile("data.csv", "country,material,total_cost\n");

        var isValid = CsvFileHelper.TryValidateFile(file, out var errors);

        Assert.IsTrue(isValid);
        Assert.AreEqual(0, errors.Count);
    }

    private static FormFile CreateFormFile(string fileName, string content)
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(contentBytes);
        return new FormFile(stream, 0, contentBytes.Length, "fileUpload", fileName);
    }
}
