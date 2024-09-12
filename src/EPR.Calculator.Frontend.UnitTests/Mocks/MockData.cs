using System.Globalization;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.UnitTests.Mocks
{
    public static class MockData
    {
        public static IEnumerable<SchemeParameterTemplateValue> GetSchemeParameterTemplateValues()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValue>();

            schemeParameterTemplateValues.AddRange([
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-AL", ParameterValue = "2210.45" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-FC", ParameterValue = "2210.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-GL", ParameterValue = "2210.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-PC", ParameterValue = "2210.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "OMC-PL", ParameterValue = "2210.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-ST", ParameterValue = "2210.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-WD", ParameterValue = "2210.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-OT", ParameterValue = "0.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SAOC-ENG", ParameterValue = "500.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SAOC-WLS", ParameterValue = "140.55" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SAOC-SCT", ParameterValue = "170.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SAOC-NIR", ParameterValue = "90.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LAPC-ENG", ParameterValue = "115.45" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LAPC-WLS", ParameterValue = "114.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LAPC-SCT", ParameterValue = "117.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LAPC-NIR", ParameterValue = "19.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SCSC-ENG", ParameterValue = "325.55" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SCSC-WLS", ParameterValue = "314.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SCSC-SCT", ParameterValue = "317.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SCSC-NIR", ParameterValue = "391.10" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-AL", ParameterValue = "70.55" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-FC", ParameterValue = "80.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-GL", ParameterValue = "60.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-PC", ParameterValue = "50.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-PL", ParameterValue = "40.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-ST", ParameterValue = "30.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-WD", ParameterValue = "20.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-OT", ParameterValue = "0.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "BADEBT-P", ParameterValue = "5.25" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "MATT-AI", ParameterValue = "5000.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "MATT-AD", ParameterValue = "-1000.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "MATT-PI", ParameterValue = "2.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "MATT-PD", ParameterValue = "-1.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "TONT-AI", ParameterValue = "50.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "TONT-AD", ParameterValue = "-10.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "TONT-PI", ParameterValue = "2.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "TONT-PD", ParameterValue = "-0.50" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LEVY-ENG", ParameterValue = "115.45" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LEVY-WLS", ParameterValue = "114.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LEVY-SCT", ParameterValue = "117.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LEVY-NIR", ParameterValue = "19.00" },
            ]);
            return schemeParameterTemplateValues;
        }

        public static string GetSchemeParametersFileContent()
        {
            return string.Concat([
                "Parameter Unique Ref,Parameter Type, Parameter Category,Valid Range From,Valid Range To,Parameter Value, Instructions,,,",
                "COMC-AL,Communication costs, Aluminium,£0.00,£999,999,999.99,£2,210.45,1.Update Parameter_Values,,,",
                "COMC - FC,Communication costs, Fibre composite,£0.00,£999,999,999.99,£2,210.00,2.File,,,",
                "COMC - GL,Communication costs, Glass,£0.00,£999,999,999.99,£2,210.00,3.Save a Copy,,",
                "COMC-PC,Communication costs, Paper or card,£0.00,£999,999,999.99,£2,210.00,4.Save as type:  CSV UTF-8(Comma delimited)(*.csv),,",
                "COMC - PL,Communication costs, Plastic,£0.00,£999,999,999.99,£2,210.00,,,",
                "COMC - ST,Communication costs, Steel,£0.00,£999,999,999.99,£2,210.00,,,",
                "COMC - WD,Communication costs, Wood,£0.00,£999,999,999.99,£2,210.00,,,",
                "COMC - OT,Communication costs, Other,£0.00,£999,999,999.99,£0.00,,,",
                "SAOC - ENG,Scheme administrator operating costs, England,£0.00,£999,999,999.99,£500.00,,https://www.gov.uk/government/publications/packaging-data-how-to-create-your-file-for-extended-producer-responsibility/packaging-data-file-specification-for-extended-producer-responsibility#packaging-material-codes,",
                "SAOC - WLS,Scheme administrator operating costs, Wales,£0.00,£999,999,999.99,£140.55,,Code,Name",
                "SAOC - SCT,Scheme administrator operating costs, Scotland,£0.00,£999,999,999.99,£170.00,,AL,Aluminium",
                "SAOC - NIR,Scheme administrator operating costs, Northern Ireland,£0.00,£999,999,999.99,£90.00,,FC,Fibre composite",
                "LAPC - ENG,Local authority data preparation costs,England,£0.00,£999,999,999.99,£115.45,,GL,Glass",
                "LAPC - WLS,Local authority data preparation costs,Wales,£0.00,£999,999,999.99,£114.00,,PC,Paper or card",
                "LAPC - SCT,Local authority data preparation costs,Scotland,£0.00,£999,999,999.99,£117.00,,PL,Plastic",
                "LAPC - NIR,Local authority data preparation costs,Northern Ireland,£0.00,£999,999,999.99,£19.00,,ST,Steel",
                "SCSC - ENG,Scheme setup costs,England,£0.00,£999,999,999.99,£325.55,,WD,Wood",
                "SCSC - WLS,Scheme setup costs,Wales,£0.00,£999,999,999.99,£314.00,,OT,Other",
                "SCSC - SCT,Scheme setup costs,Scotland,£0.00,£999,999,999.99,£317.00,,,",
                "SCSC - NIR,Scheme setup costs,Northern Ireland,£0.00,£999,999,999.99,£391.10,,,",
                "LRET - AL,Late reporting tonnage,Aluminium,0.000,999,999,999.000,70.550,,,",
                "LRET - FC,Late reporting tonnage,Glass,0.000,999,999,999.000,80.000,,Use consistent country codes -GOV.UK(www.gov.uk)",
                "LRET - GL,Late reporting tonnage,Plastic,0.000,999,999,999.000,60.000,,Code,Name",
                "LRET - PC,Late reporting tonnage,Steel,0.000,999,999,999.000,50.000,,ENG,England",
                "LRET - PL,Late reporting tonnage,Wood,0.000,999,999,999.000,40.000,,WLS,Wales",
                "LRET - ST,Late reporting tonnage,Paper or card,0.000,999,999,999.000,30.000,,SCT,Scotland",
                "LRET - WD,Late reporting tonnage,Fibre composite,0.000,999,999,999.000,20.000,,NIR,Northern Ireland",
                "LRET - OT,Late reporting tonnage,Other,0.000,999,999,999.000,0.000,,,",
                "BADEBT - P,Bad debt provision percentage, BadDebt,0.00 %,100000.00 %,5.25 %,,,",
                "MATT - AI,Materiality threshold, Amount Increase,£0.00,£999,999,999.99,£5,000.00,,,",
                "MATT - AD,Materiality threshold, Amount Decrease,£0.00,-£999,999,999.99,-£1,000.00,,,",
                "MATT - PI,Materiality threshold, Percent Increase,0.00 %,1000.00 %,2.00 %,,,",
                "MATT - PD,Materiality threshold, Percent Decrease,0.00 %,-1000.00 %,-1.00 %,,,",
                "TONT - AI,Tonnage change threshold,Amount Increase,£0.00,£999,999,999.99,£50.00,,,",
                "TONT - DI,Tonnage change threshold,Amount Decrease,£0.00,-£999,999,999.99,-£10.00,,,",
                "TONT - PI,Tonnage change threshold,Percent Increase,0.00 %,1000.00 %,2.00 %,,,",
                "TONT - PD,Tonnage change threshold,Percent Decrease,0.00 %,-1000.00 %,-0.50 %,,,",
                "LEVY - ENG,Levy,England,£0.00,£999,999,999.99,£115.45,,,",
                "LEVY - WLS,Levy,Wales,£0.00,£999,999,999.99,£114.00,,,",
                "LEVY - SCT,Levy,Scotland,£0.00,£999,999,999.99,£117.00,,,",
                "LEVY - NIR,Levy,Northern Ireland,£0.00,£999,999,999.99,£19.00,,,",
                "Parameter upload version v1.0,,,,,,,,"
            ]);
        }

        public static IEnumerable<DefaultSchemeParameters> GetDefaultParameterValues()
        {
            var defaultParameterValues = new List<DefaultSchemeParameters>();
            defaultParameterValues.AddRange([
            new DefaultSchemeParameters { ParameterUniqueRef = "COMC-AL", ParameterValue = 2210.45m, ParameterType = "Aluminium", ParameterCategory = "Communication costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-FC", ParameterValue = 2210.00m, ParameterType = "Fibre composite", ParameterCategory = "Communication costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-GL", ParameterValue = 2210.00m, ParameterType = "Glass", ParameterCategory = "Communication costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-PC", ParameterValue = 2210.00m, ParameterType = "Paper or card", ParameterCategory = "Communication costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "OMC-PL", ParameterValue = 2210.00m, ParameterType = "Plastic", ParameterCategory = "Communication costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-ST", ParameterValue = 2210.00m, ParameterType = "Steel", ParameterCategory = "Communication costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-WD", ParameterValue = 2210.00m, ParameterType = "Wood", ParameterCategory = "Communication costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-OT", ParameterValue = 0.00m, ParameterType = "Other", ParameterCategory = "Communication costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "SAOC-ENG", ParameterValue = 500.00m, ParameterType = "England", ParameterCategory = "Scheme administrator operating costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "SAOC-WLS", ParameterValue = 140.55m, ParameterType = "Wales", ParameterCategory = "Scheme administrator operating costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "SAOC-SCT", ParameterValue = 170.00m, ParameterType = "Scotland", ParameterCategory = "Scheme administrator operating costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "SAOC-NIR", ParameterValue = 90.00m, ParameterType = "Northern Ireland", ParameterCategory = "Scheme administrator operating costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LAPC-ENG", ParameterValue = 115.45m, ParameterType = "England", ParameterCategory = "Local authority data preparation costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LAPC-WLS", ParameterValue = 114.00m, ParameterType = "Wales", ParameterCategory = "Local authority data preparation costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LAPC-SCT", ParameterValue = 117.00m, ParameterType = "Scotland", ParameterCategory = "Local authority data preparation costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LAPC-NIR", ParameterValue = 19.00m, ParameterType = "Northern Ireland", ParameterCategory = "Local authority data preparation costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "SCSC-ENG", ParameterValue = 325.55m, ParameterType = "England", ParameterCategory = "Scheme setup costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "SCSC-WLS", ParameterValue = 314.00m, ParameterType = "Wales", ParameterCategory = "Scheme setup costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "SCSC-SCT", ParameterValue = 317.00m, ParameterType = "Scotland", ParameterCategory = "Scheme setup costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "SCSC-NIR", ParameterValue = 391.10m, ParameterType = "Northern Ireland", ParameterCategory = "Scheme setup costs", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-AL", ParameterValue = 70.55m, ParameterType = "Aluminium", ParameterCategory = "Late reporting tonnage", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-FC", ParameterValue = 80.00m, ParameterType = "Fibre composite", ParameterCategory = "Late reporting tonnage", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-GL", ParameterValue = 60.00m, ParameterType = "Glass", ParameterCategory = "Late reporting tonnage", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-PC", ParameterValue = 50.00m, ParameterType = "Paper or card", ParameterCategory = "Late reporting tonnage", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-PL", ParameterValue = 40.00m, ParameterType = "Plastic", ParameterCategory = "Late reporting tonnage", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-ST", ParameterValue = 30.00m, ParameterType = "Steel", ParameterCategory = "Late reporting tonnage", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-WD", ParameterValue = 20.00m, ParameterType = "Wood", ParameterCategory = "Late reporting tonnage", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-OT", ParameterValue = 0.00m, ParameterType = "Other", ParameterCategory = "Late reporting tonnage", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "BADEBT-P", ParameterValue = 5.25m, ParameterType = "Percentage", ParameterCategory = "Bad debt provision", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "MATT-AI", ParameterValue = 5000.00m, ParameterType = "Amount Increase", ParameterCategory = "Materiality threshold", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "MATT-AD", ParameterValue = -1000.00m, ParameterType = "Amount Decrease", ParameterCategory = "Materiality threshold", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "MATT-PI", ParameterValue = 2.00m, ParameterType = "Percent Increase", ParameterCategory = "Materiality threshold", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "MATT-PD", ParameterValue = -1.00m, ParameterType = "Percent Decrease", ParameterCategory = "Materiality threshold", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "TONT-AI", ParameterValue = 50.00m, ParameterType = "Amount Increase", ParameterCategory = "Tonnage change threshold", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "TONT-AD", ParameterValue = -10.00m, ParameterType = "Amount Decrease", ParameterCategory = "Tonnage change threshold", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "TONT-PI", ParameterValue = 2.00m, ParameterType = "Percent Increase", ParameterCategory = "Tonnage change threshold", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "TONT-PD", ParameterValue = -0.50m, ParameterType = "Percent Decrease", ParameterCategory = "Tonnage change threshold", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LEVY-ENG", ParameterValue = 115.45m, ParameterType = "England", ParameterCategory = "Levy", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LEVY-WLS", ParameterValue = 114.00m, ParameterType = "Wales", ParameterCategory = "Levy", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LEVY-SCT", ParameterValue = 117.00m, ParameterType = "Scotland", ParameterCategory = "Levy", ParameterYear = "2024-25" },
             new DefaultSchemeParameters { ParameterUniqueRef = "LEVY-NIR", ParameterValue = 19.00m, ParameterType = "Northern Ireland", ParameterCategory = "Levy", ParameterYear = "2024-25" },
             ]);

            return defaultParameterValues;
        }

        public static IEnumerable<CalculationRun> GetCalculationRuns()
        {
            var calculationRuns = new List<CalculationRun>();

            calculationRuns.AddRange([
                new CalculationRun { Id = 1, CalculatorRunClassificationId = 1, Name = "Default cettings check", CreatedAt = DateTime.Parse("28/06/2025 10:01:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.InTheQueue },
                new CalculationRun { Id = 2, CalculatorRunClassificationId = 2, Name = "Alteration check", CreatedAt = DateTime.Parse("28/06/2025 12:19:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Running },
                new CalculationRun { Id = 3, CalculatorRunClassificationId = 3, Name = "Test 10", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Unclassified },
                new CalculationRun { Id = 4, CalculatorRunClassificationId = 4, Name = "June check", CreatedAt = DateTime.Parse("11/06/2025 09:14:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play },
                new CalculationRun { Id = 5, CalculatorRunClassificationId = 4, Name = "Pre June check", CreatedAt = DateTime.Parse("13/06/2025 11:18:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play },
                new CalculationRun { Id = 6, CalculatorRunClassificationId = 4, Name = "Local Authority data check 5", CreatedAt = DateTime.Parse("10/06/2025 08:13:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play },
                new CalculationRun { Id = 7, CalculatorRunClassificationId = 4, Name = "Local Authority data check 4", CreatedAt = DateTime.Parse("10/06/2025 10:14:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play },
                new CalculationRun { Id = 8, CalculatorRunClassificationId = 4, Name = "Local Authority data check 3", CreatedAt = DateTime.Parse("08/06/2025 10:00:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play },
                new CalculationRun { Id = 9, CalculatorRunClassificationId = 4, Name = "Local Authority data check 2", CreatedAt = DateTime.Parse("06/06/2025 11:20:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play },
                new CalculationRun { Id = 10, CalculatorRunClassificationId = 4, Name = "Local Authority data check", CreatedAt = DateTime.Parse("02/06/2025 12:02:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play },
                new CalculationRun { Id = 11, CalculatorRunClassificationId = 5, Name = "Fee adjustment check", CreatedAt = DateTime.Parse("01/06/2025 09:12:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Error }
            ]);

            return calculationRuns;
        }

        public static IEnumerable<LocalAuthorityDisposalCost> GetLocalAuthorityDisposalCosts()
        {
            var localAuthorityDisposalCosts = new List<LocalAuthorityDisposalCost>();

            localAuthorityDisposalCosts.AddRange([
                new LocalAuthorityDisposalCost { Id = 1, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-AL", Country = "England", Material = "Aluminium", TotalCost = 2210.45m },
                new LocalAuthorityDisposalCost { Id = 2, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-FC", Country = "England", Material = "Fibre composite", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 3, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-GL", Country = "England", Material = "Glass", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 4, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-PC", Country = "England", Material = "Paper or card", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 5, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-PL", Country = "England", Material = "Plastic", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 6, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-ST", Country = "England", Material = "Steel", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 7, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-WD", Country = "England", Material = "Wood", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 8, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-OT", Country = "England", Material = "Other", TotalCost = 0m },
                new LocalAuthorityDisposalCost { Id = 9, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-AL", Country = "NI", Material = "Aluminium", TotalCost = 10m },
                new LocalAuthorityDisposalCost { Id = 10, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-FC", Country = "NI", Material = "Fibre composite", TotalCost = 11m },
                new LocalAuthorityDisposalCost { Id = 11, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-GL", Country = "NI", Material = "Glass", TotalCost = 12m },
                new LocalAuthorityDisposalCost { Id = 12, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-PC", Country = "NI", Material = "Paper or card", TotalCost = 13m },
                new LocalAuthorityDisposalCost { Id = 13, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-PL", Country = "NI", Material = "Plastic", TotalCost = 14m },
                new LocalAuthorityDisposalCost { Id = 14, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-ST", Country = "NI", Material = "Steel", TotalCost = 15m },
                new LocalAuthorityDisposalCost { Id = 15, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-WD", Country = "NI", Material = "Wood", TotalCost = 16m },
                new LocalAuthorityDisposalCost { Id = 16, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-OT", Country = "NI", Material = "Other", TotalCost = 17m },
                new LocalAuthorityDisposalCost { Id = 17, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-AL", Country = "Scotland", Material = "Aluminium", TotalCost = 20.01m },
                new LocalAuthorityDisposalCost { Id = 18, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-FC", Country = "Scotland", Material = "Fibre composite", TotalCost = 21.01m },
                new LocalAuthorityDisposalCost { Id = 19, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-GL", Country = "Scotland", Material = "Glass", TotalCost = 22.01m },
                new LocalAuthorityDisposalCost { Id = 20, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-PC", Country = "Scotland", Material = "Paper or card", TotalCost = 23.01m },
                new LocalAuthorityDisposalCost { Id = 21, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-PL", Country = "Scotland", Material = "Plastic", TotalCost = 24.01m },
                new LocalAuthorityDisposalCost { Id = 22, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-ST", Country = "Scotland", Material = "Steel", TotalCost = 25.01m },
                new LocalAuthorityDisposalCost { Id = 23, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-WD", Country = "Scotland", Material = "Wood", TotalCost = 26.01m },
                new LocalAuthorityDisposalCost { Id = 24, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-OT", Country = "Scotland", Material = "Other", TotalCost = 27.01m },
                new LocalAuthorityDisposalCost { Id = 25, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-AL", Country = "Wales", Material = "Aluminium", TotalCost = 30.01m },
                new LocalAuthorityDisposalCost { Id = 26, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-FC", Country = "Wales", Material = "Fibre composite", TotalCost = 30.02m },
                new LocalAuthorityDisposalCost { Id = 27, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-GL", Country = "Wales", Material = "Glass", TotalCost = 30.03m },
                new LocalAuthorityDisposalCost { Id = 28, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-PC", Country = "Wales", Material = "Paper or card", TotalCost = 30.04m },
                new LocalAuthorityDisposalCost { Id = 29, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-PL", Country = "Wales", Material = "Plastic", TotalCost = 30.05m },
                new LocalAuthorityDisposalCost { Id = 30, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-ST", Country = "Wales", Material = "Steel", TotalCost = 30.06m },
                new LocalAuthorityDisposalCost { Id = 31, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-WD", Country = "Wales", Material = "Wood", TotalCost = 30.07m },
                new LocalAuthorityDisposalCost { Id = 32, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-OT", Country = "Wales", Material = "Other", TotalCost = 30.08m },
            ]);

            return localAuthorityDisposalCosts;
        }

        public static IEnumerable<LapcapDataTemplateValueDto> GetLocalAuthorityDisposalCostsToUpload()
        {
            var localAuthorityDisposalCosts = new List<LapcapDataTemplateValueDto>();

            localAuthorityDisposalCosts.AddRange([
                new LapcapDataTemplateValueDto { CountryName = "England", Material = "Aluminium", TotalCost = "2210.45" },
                new LapcapDataTemplateValueDto { CountryName = "England", Material = "Fibre composite", TotalCost = "2210" },
                new LapcapDataTemplateValueDto { CountryName = "England", Material = "Glass", TotalCost = "2210.45" },
                new LapcapDataTemplateValueDto { CountryName = "England", Material = "Paper or card", TotalCost = "2210" },
                new LapcapDataTemplateValueDto { CountryName = "England", Material = "Plastic", TotalCost = "2210" },
                new LapcapDataTemplateValueDto { CountryName = "England", Material = "Steel", TotalCost = "2210" },
                new LapcapDataTemplateValueDto { CountryName = "England", Material = "Wood", TotalCost = "2210" },
                new LapcapDataTemplateValueDto { CountryName = "England", Material = "Other", TotalCost = "2210" },
                new LapcapDataTemplateValueDto { CountryName = "NI", Material = "Aluminium", TotalCost = "10" },
                new LapcapDataTemplateValueDto { CountryName = "NI", Material = "Fibre composite", TotalCost = "11" },
                new LapcapDataTemplateValueDto { CountryName = "NI", Material = "Glass", TotalCost = "12" },
                new LapcapDataTemplateValueDto { CountryName = "NI", Material = "Paper or card", TotalCost = "13" },
                new LapcapDataTemplateValueDto { CountryName = "NI", Material = "Plastic", TotalCost = "14" },
                new LapcapDataTemplateValueDto { CountryName = "NI", Material = "Steel", TotalCost = "15" },
                new LapcapDataTemplateValueDto { CountryName = "NI", Material = "Wood", TotalCost = "16" },
                new LapcapDataTemplateValueDto { CountryName = "NI", Material = "Other", TotalCost = "17" },
                new LapcapDataTemplateValueDto { CountryName = "Scotland", Material = "Aluminium", TotalCost = "20.1" },
                new LapcapDataTemplateValueDto { CountryName = "Scotland", Material = "Fibre composite", TotalCost = "21.1" },
                new LapcapDataTemplateValueDto { CountryName = "Scotland", Material = "Glass", TotalCost = "22.1" },
                new LapcapDataTemplateValueDto { CountryName = "Scotland", Material = "Paper or card", TotalCost = "23.1" },
                new LapcapDataTemplateValueDto { CountryName = "Scotland", Material = "Plastic", TotalCost = "24.1" },
                new LapcapDataTemplateValueDto { CountryName = "Scotland", Material = "Steel", TotalCost = "25.1" },
                new LapcapDataTemplateValueDto { CountryName = "Scotland", Material = "Wood", TotalCost = "26.1" },
                new LapcapDataTemplateValueDto { CountryName = "Scotland", Material = "Other", TotalCost = "27.1" },
                new LapcapDataTemplateValueDto { CountryName = "Wales", Material = "Aluminium", TotalCost = "30.01" },
                new LapcapDataTemplateValueDto { CountryName = "Wales", Material = "Fibre composite", TotalCost = "30.02" },
                new LapcapDataTemplateValueDto { CountryName = "Wales", Material = "Glass", TotalCost = "30.03" },
                new LapcapDataTemplateValueDto { CountryName = "Wales", Material = "Paper or card", TotalCost = "30.04" },
                new LapcapDataTemplateValueDto { CountryName = "Wales", Material = "Plastic", TotalCost = "30.05" },
                new LapcapDataTemplateValueDto { CountryName = "Wales", Material = "Steel", TotalCost = "30.06" },
                new LapcapDataTemplateValueDto { CountryName = "Wales", Material = "Wood", TotalCost = "30.07" },
                new LapcapDataTemplateValueDto { CountryName = "Wales", Material = "Other", TotalCost = "30.08" },
            ]);

            return localAuthorityDisposalCosts;
        }
    }
}
