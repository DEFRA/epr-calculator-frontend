using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers.Csv;
using Microsoft.AspNetCore.Http;

namespace EPR.Calculator.Frontend.UnitTests.Helpers.Csv;

[TestClass]
public class LapcapCsvFileHelperTests
{
    [TestMethod]
    public async Task Parse_NullFile_ReturnsFileErrorsOnly()
    {
        var result = await LapcapCsvFileHelper.Parse(null, CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { ErrorMessages.FileNotSelected }, result.FileErrors.ToArray());
        Assert.AreEqual(0, result.ContentErrors.Count);
        Assert.AreEqual(0, result.Records.Count);
    }

    [TestMethod]
    public async Task Parse_ValidCsv_ParsesRecords()
    {
        var csv = """
            country,material,total_cost,projection_year
            England,Aluminium,2210.45,2024
            Wales,Glass,"£1,234.50",2024
            """;
        var file = CreateFormFile("lapcap.csv", csv);

        var result = await LapcapCsvFileHelper.Parse(file, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Records.Count);
        Assert.AreEqual("England", result.Records[0].Country);
        Assert.AreEqual("Aluminium", result.Records[0].Material);
        Assert.AreEqual(2210.45m, result.Records[0].TotalCost);
        Assert.AreEqual("Wales", result.Records[1].Country);
        Assert.AreEqual("Glass", result.Records[1].Material);
        Assert.AreEqual(1234.50m, result.Records[1].TotalCost);
    }

    [TestMethod]
    public async Task Parse_NormalizesHeaderNames()
    {
        var csv = """
            Country, Material , Total Cost
            England,Aluminium,10
            """;
        var file = CreateFormFile("lapcap.csv", csv);

        var result = await LapcapCsvFileHelper.Parse(file, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Records.Count);
        Assert.AreEqual(10m, result.Records[0].TotalCost);
    }

    [TestMethod]
    public async Task Parse_SkipsBlankRows()
    {
        var csv = """
            country,material,total_cost
            England,Aluminium,10

            Wales,Glass,20
            """;
        var file = CreateFormFile("lapcap.csv", csv);

        var result = await LapcapCsvFileHelper.Parse(file, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Records.Count);
    }

    [TestMethod]
    public async Task Parse_HeaderOnlyCsv_IsSuccessWithNoRecords()
    {
        var file = CreateFormFile("lapcap.csv", "country,material,total_cost\n");

        var result = await LapcapCsvFileHelper.Parse(file, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Records.Count);
    }

    [TestMethod]
    public async Task Parse_MissingRequiredHeader_ReturnsContentError()
    {
        var file = CreateFormFile("lapcap.csv", "country,material\nEngland,Aluminium\n");

        var result = await LapcapCsvFileHelper.Parse(file, CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(0, result.FileErrors.Count);
        Assert.AreEqual(0, result.Records.Count);
        Assert.IsTrue(result.ContentErrors.Any(e => e.Contains("total_cost", StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(result.ContentErrors.Any(e => e.Contains("missing", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task Parse_InvalidTotalCost_ReturnsFriendlyContentErrorAndContinues()
    {
        var csv = """
            country,material,total_cost
            England,Aluminium,not-a-number
            Wales,Glass,20
            """;
        var file = CreateFormFile("lapcap.csv", csv);

        var result = await LapcapCsvFileHelper.Parse(file, CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(1, result.Records.Count);
        Assert.AreEqual("Wales", result.Records[0].Country);
        Assert.AreEqual(20m, result.Records[0].TotalCost);
        Assert.AreEqual(1, result.ContentErrors.Count);
        StringAssert.Contains(
            result.ContentErrors[0],
            "The total cost for Aluminium in England is invalid");
    }

    [TestMethod]
    public async Task Parse_MultipleInvalidRows_CollectsAllContentErrors()
    {
        var csv = """
            country,material,total_cost
            England,Aluminium,abc
            Wales,Glass,def
            Scotland,Steel,30
            """;
        var file = CreateFormFile("lapcap.csv", csv);

        var result = await LapcapCsvFileHelper.Parse(file, CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(1, result.Records.Count);
        Assert.AreEqual(2, result.ContentErrors.Count);
        StringAssert.Contains(result.ContentErrors[0], "Aluminium in England");
        StringAssert.Contains(result.ContentErrors[1], "Glass in Wales");
    }

    private static FormFile CreateFormFile(string fileName, string content)
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(contentBytes);
        return new FormFile(stream, 0, contentBytes.Length, "fileUpload", fileName);
    }
}
