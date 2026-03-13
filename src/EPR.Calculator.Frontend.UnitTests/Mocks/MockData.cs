using System.Globalization;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

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
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-UK", ParameterValue = "250.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-ENG", ParameterValue = "250.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-WLS", ParameterValue = "250.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-SCT", ParameterValue = "250.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-NIR", ParameterValue = "250.00" },
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
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-AL-R", ParameterValue = "70.55" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-AL", ParameterValue = "70.55" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-AL", ParameterValue = "70.55" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-FC-R", ParameterValue = "80.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-FC", ParameterValue = "80.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-FC-G", ParameterValue = "80.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-GL-R", ParameterValue = "60.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-GL", ParameterValue = "60.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-GL-G", ParameterValue = "60.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-PC-R", ParameterValue = "50.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-PC", ParameterValue = "50.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-PC-G", ParameterValue = "50.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-PL-R", ParameterValue = "40.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-PL", ParameterValue = "40.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-PL-G", ParameterValue = "40.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-ST-R", ParameterValue = "30.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-ST", ParameterValue = "30.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-ST-G", ParameterValue = "30.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-WD-R", ParameterValue = "20.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-WD", ParameterValue = "20.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-WD-G", ParameterValue = "20.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-OT-R", ParameterValue = "0.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-OT", ParameterValue = "0.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-OT-G", ParameterValue = "0.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "BADEBT-P", ParameterValue = "5.25" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "MATT-AI", ParameterValue = "5000.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "MATT-AD", ParameterValue = "-1000.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "MATT-PI", ParameterValue = "2.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "MATT-PD", ParameterValue = "-1.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "TONT-AI", ParameterValue = "50.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "TONT-AD", ParameterValue = "-10.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "TONT-PI", ParameterValue = "2.00" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "TONT-PD", ParameterValue = "-0.50" },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "REDM-RF", ParameterValue = "1.200" },
            ]);
            return schemeParameterTemplateValues;
        }

        public static string GetSchemeParametersFileContent()
        {
            return string.Concat([
                "Parameter Unique Ref,Parameter Type, Parameter Category,Valid Range From,Valid Range To,Parameter Value, Instructions,,,",
                "COMC-AL,Communication costs by material, Aluminium,£0.00,£999,999,999.99,£2,210.45,1.Update Parameter_Values,,,",
                "COMC-FC,Communication costs by material, Fibre composite,£0.00,£999,999,999.99,£2,210.00,2.File,,,",
                "COMC-GL,Communication costs by material, Glass,£0.00,£999,999,999.99,£2,210.00,3.Save a Copy,,",
                "COMC-PC,Communication costs by material, Paper or card,£0.00,£999,999,999.99,£2,210.00,4.Save as type:  CSV UTF-8(Comma delimited)(*.csv),,",
                "COMC-PL,Communication costs by material, Plastic,£0.00,£999,999,999.99,£2,210.00,,,",
                "COMC-ST,Communication costs by material, Steel,£0.00,£999,999,999.99,£2,210.00,,,",
                "COMC-WD,Communication costs by material, Wood,£0.00,£999,999,999.99,£2,210.00,,,",
                "COMC-OT,Communication costs by material, other,£0.00,£999,999,999.99,£0.00,,,",
                "COMC-UK,Communication costs by country, United Kingdom,£0.00,£999,999,999.99,£0.00,,,",
                "COMC-ENG,Communication costs by country, England,£0.00,£999,999,999.99,£0.00,,,",
                "COMC-WLS,Communication costs by country, Wales,£0.00,£999,999,999.99,£0.00,,,",
                "COMC-SCT,Communication costs by country, Scotland,£0.00,£999,999,999.99,£0.00,,,",
                "COMC-NIR,Communication costs by country, Northern Ireland,£0.00,£999,999,999.99,£0.00,,,",
                "SAOC-ENG,Scheme administrator operating costs, England,£0.00,£999,999,999.99,£500.00,,https://www.gov.uk/government/publications/packaging-data-how-to-create-your-file-for-extended-producer-responsibility/packaging-data-file-specification-for-extended-producer-responsibility#packaging-material-codes,",
                "SAOC-WLS,Scheme administrator operating costs, Wales,£0.00,£999,999,999.99,£140.55,,Code,Name",
                "SAOC-SCT,Scheme administrator operating costs, Scotland,£0.00,£999,999,999.99,£170.00,,AL,Aluminium",
                "SAOC-NIR,Scheme administrator operating costs, Northern Ireland,£0.00,£999,999,999.99,£90.00,,FC,Fibre composite",
                "LAPC-ENG,Local authority data preparation costs,England,£0.00,£999,999,999.99,£115.45,,GL,Glass",
                "LAPC-WLS,Local authority data preparation costs,Wales,£0.00,£999,999,999.99,£114.00,,PC,Paper or card",
                "LAPC-SCT,Local authority data preparation costs,Scotland,£0.00,£999,999,999.99,£117.00,,PL,Plastic",
                "LAPC-NIR,Local authority data preparation costs,Northern Ireland,£0.00,£999,999,999.99,£19.00,,ST,Steel",
                "SCSC-ENG,Scheme setup costs,England,£0.00,£999,999,999.99,£325.55,,WD,Wood",
                "SCSC-WLS,Scheme setup costs,Wales,£0.00,£999,999,999.99,£314.00,,OT,Other",
                "SCSC-SCT,Scheme setup costs,Scotland,£0.00,£999,999,999.99,£317.00,,,",
                "SCSC-NIR,Scheme setup costs,Northern Ireland,£0.00,£999,999,999.99,£391.10,,,",
                "LRET-AL-R,Late reporting tonnage,Aluminium-R,0.000,999,999,999.000,70.550,,,",
                "LRET-AL,Late reporting tonnage,Aluminium,0.000,999,999,999.000,70.550,,,",
                "LRET-AL-G,Late reporting tonnage,Aluminium-G,0.000,999,999,999.000,70.550,,,",
                "LRET-FC-R,Late reporting tonnage,Glass-R,0.000,999,999,999.000,80.000,,Use consistent country codes -GOV.UK(www.gov.uk)",
                "LRET-FC,Late reporting tonnage,Glass,0.000,999,999,999.000,80.000,,Use consistent country codes -GOV.UK(www.gov.uk)",
                "LRET-FC-G,Late reporting tonnage,Glass-G,0.000,999,999,999.000,80.000,,Use consistent country codes -GOV.UK(www.gov.uk)",
                "LRET-GL-R,Late reporting tonnage,Plastic-R,0.000,999,999,999.000,60.000,,Code,Name",
                "LRET-GL,Late reporting tonnage,Plastic,0.000,999,999,999.000,60.000,,Code,Name",
                "LRET-GL-G,Late reporting tonnage,Plastic-G,0.000,999,999,999.000,60.000,,Code,Name",
                "LRET-PC-R,Late reporting tonnage,Steel-R,0.000,999,999,999.000,50.000,,ENG,England",
                "LRET-PC,Late reporting tonnage,Steel,0.000,999,999,999.000,50.000,,ENG,England",
                "LRET-PC-G,Late reporting tonnage,Steel-G,0.000,999,999,999.000,50.000,,ENG,England",
                "LRET-PL-R,Late reporting tonnage,Wood-R,0.000,999,999,999.000,40.000,,WLS,Wales",
                "LRET-PL,Late reporting tonnage,Wood,0.000,999,999,999.000,40.000,,WLS,Wales",
                "LRET-PL-G,Late reporting tonnage,Wood-G,0.000,999,999,999.000,40.000,,WLS,Wales",
                "LRET-ST-R,Late reporting tonnage,Paper or card-R,0.000,999,999,999.000,30.000,,SCT,Scotland",
                "LRET-ST,Late reporting tonnage,Paper or card,0.000,999,999,999.000,30.000,,SCT,Scotland",
                "LRET-ST-G,Late reporting tonnage,Paper or card-G,0.000,999,999,999.000,30.000,,SCT,Scotland",
                "LRET-WD-R,Late reporting tonnage,Fibre composite-R,0.000,999,999,999.000,20.000,,NIR,Northern Ireland",
                "LRET-WD,Late reporting tonnage,Fibre composite,0.000,999,999,999.000,20.000,,NIR,Northern Ireland",
                "LRET-WD-G,Late reporting tonnage,Fibre composite-G,0.000,999,999,999.000,20.000,,NIR,Northern Ireland",
                "LRET-OT-R,Late reporting tonnage,Other-R,0.000,999,999,999.000,0.000,,,",
                "LRET-OT,Late reporting tonnage,Other,0.000,999,999,999.000,0.000,,,",
                "LRET-OT-G,Late reporting tonnage,Other-G,0.000,999,999,999.000,0.000,,,",
                "BADEBT-P,Bad debt provision percentage, BadDebt,0.00 %,100000.00 %,5.25 %,,,",
                "MATT-AI,Materiality threshold, Amount Increase,£0.00,£999,999,999.99,£5,000.00,,,",
                "MATT-AD,Materiality threshold, Amount Decrease,£0.00,-£999,999,999.99,-£1,000.00,,,",
                "MATT-PI,Materiality threshold, Percent Increase,0.00 %,1000.00 %,2.00 %,,,",
                "MATT-PD,Materiality threshold, Percent Decrease,0.00 %,-1000.00 %,-1.00 %,,,",
                "TONT-AI,Tonnage change threshold,Amount Increase,£0.00,£999,999,999.99,£50.00,,,",
                "TONT-DI,Tonnage change threshold,Amount Decrease,£0.00,-£999,999,999.99,-£10.00,,,",
                "TONT-PI,Tonnage change threshold,Percent Increase,0.00 %,1000.00 %,2.00 %,,,",
                "TONT-PD,Tonnage change threshold,Percent Decrease,0.00 %,-1000.00 %,-0.50 %,,,",
                "REDM-RF,Red modulation factor,Modulation Factor,1.000,2.000,1.200,,,",
                "Parameter upload version v1.2,,,,,,,,"
            ]);
        }

        public static IEnumerable<DefaultSchemeParameters> GetDefaultParameterValues()
        {
            var defaultParameterValues = new List<DefaultSchemeParameters>();
            defaultParameterValues.AddRange([
            new DefaultSchemeParameters { ParameterUniqueRef = "COMC-AL", ParameterValue = 2210.45m, ParameterCategory = "Aluminium", ParameterType = "Communication costs by material", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 5, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new (2024, 5, 28, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-FC", ParameterValue = 2210.00m, ParameterCategory = "Fibre composite", ParameterType = "Communication costs by material", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 5, 29, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 5, 29, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-GL", ParameterValue = 2210.00m, ParameterCategory = "Glass", ParameterType = "Communication costs by material", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 5, 30, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 5, 30, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-PC", ParameterValue = 2210.00m, ParameterCategory = "Paper or card", ParameterType = "Communication costs by material", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 5, 30, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "OMC-PL", ParameterValue = 2210.00m, ParameterCategory = "Plastic", ParameterType = "Communication costs by material", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-ST", ParameterValue = 2210.00m, ParameterCategory = "Steel", ParameterType = "Communication costs by material", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-WD", ParameterValue = 2210.00m, ParameterCategory = "Wood", ParameterType = "Communication costs by material", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-OT", ParameterValue = 0.00m, ParameterCategory = "Other", ParameterType = "Communication costs by material", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-UK", ParameterValue = 0.00m, ParameterCategory = "United Kingdom", ParameterType = "Communication costs by country", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-ENG", ParameterValue = 115.45m, ParameterCategory = "England", ParameterType = "Communication costs by country", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-WLS", ParameterValue = 114.00m, ParameterCategory = "Wales", ParameterType = "Communication costs by country", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-SCT", ParameterValue = 117.00m, ParameterCategory = "Scotland", ParameterType = "Communication costs by country", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "COMC-NIR", ParameterValue = 19.00m, ParameterCategory = "Northern Ireland", ParameterType = "Communication costs by country", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "SAOC-ENG", ParameterValue = 500.00m, ParameterCategory = "England", ParameterType = "Scheme administrator operating costs", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 1, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "SAOC-WLS", ParameterValue = 140.55m, ParameterCategory = "Wales", ParameterType = "Scheme administrator operating costs", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 2, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 2, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "SAOC-SCT", ParameterValue = 170.00m, ParameterCategory = "Scotland", ParameterType = "Scheme administrator operating costs", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 3, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 3, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "SAOC-NIR", ParameterValue = 90.00m, ParameterCategory = "Northern Ireland", ParameterType = "Scheme administrator operating costs", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 4, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 4, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LAPC-ENG", ParameterValue = 115.45m, ParameterCategory = "England", ParameterType = "Local authority data preparation costs", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 5, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 5, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LAPC-WLS", ParameterValue = 114.00m, ParameterCategory = "Wales", ParameterType = "Local authority data preparation costs", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 9, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 9, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LAPC-SCT", ParameterValue = 117.00m, ParameterCategory = "Scotland", ParameterType = "Local authority data preparation costs", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 16, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 16, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LAPC-NIR", ParameterValue = 19.00m, ParameterCategory = "Northern Ireland", ParameterType = "Local authority data preparation costs", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 18, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 18, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "SCSC-ENG", ParameterValue = 325.55m, ParameterCategory = "England", ParameterType = "Scheme setup costs", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 19, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 19, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "SCSC-WLS", ParameterValue = 314.00m, ParameterCategory = "Wales", ParameterType = "Scheme setup costs", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 23, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 23, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "SCSC-SCT", ParameterValue = 317.00m, ParameterCategory = "Scotland", ParameterType = "Scheme setup costs", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 6, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 6, 28, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "SCSC-NIR", ParameterValue = 391.10m, ParameterCategory = "Northern Ireland", ParameterType = "Scheme setup costs", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 10, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 10, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-AL-R", ParameterValue = 170.55m, ParameterCategory = "Aluminium-R", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 12, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 12, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-AL", ParameterValue = 70.55m, ParameterCategory = "Aluminium-A", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 12, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 12, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-AL-G", ParameterValue = 270.55m, ParameterCategory = "Aluminium-G", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 12, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 12, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-FC-R", ParameterValue = 180.00m, ParameterCategory = "Fibre composite-R", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 14, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 14, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-FC", ParameterValue = 80.00m, ParameterCategory = "Fibre composite-A", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 14, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 14, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-FC-G", ParameterValue = 280.00m, ParameterCategory = "Fibre composite-G", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 14, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 14, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-GL-R", ParameterValue = 160.00m, ParameterCategory = "Glass-R", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 16, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 16, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-GL", ParameterValue = 60.00m, ParameterCategory = "Glass-A", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 16, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 16, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-GL-G", ParameterValue = 260.00m, ParameterCategory = "Glass-G", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 16, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 16, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-PC-R", ParameterValue = 150.00m, ParameterCategory = "Paper or card-R", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 16, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 16, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-PC", ParameterValue = 50.00m, ParameterCategory = "Paper or card-A", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 16, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 16, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-PC-G", ParameterValue = 250.00m, ParameterCategory = "Paper or card-G", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 16, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 16, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-PL-R", ParameterValue = 140.00m, ParameterCategory = "Plastic-R", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 18, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 18, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-PL", ParameterValue = 40.00m, ParameterCategory = "Plastic-A", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 18, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 18, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-PL-G", ParameterValue = 240.00m, ParameterCategory = "Plastic-G", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 18, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 18, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-ST-R", ParameterValue = 130.00m, ParameterCategory = "Steel-R", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 20, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 20, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-ST", ParameterValue = 30.00m, ParameterCategory = "Steel-A", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 20, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 20, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-ST-G", ParameterValue = 230.00m, ParameterCategory = "Steel-G", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 20, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 20, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-WD-R", ParameterValue = 120.00m, ParameterCategory = "Wood-R", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 21, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 21, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-WD", ParameterValue = 20.00m, ParameterCategory = "Wood-A", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 21, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 21, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-WD-G", ParameterValue = 220.00m, ParameterCategory = "Wood-G", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 21, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 21, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-OT-R", ParameterValue = 1.00m, ParameterCategory = "Other-R", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 23, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 23, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-OT", ParameterValue = 0.00m, ParameterCategory = "Other-A", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 23, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 23, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "LRET-OT-G", ParameterValue = 2.00m, ParameterCategory = "Other-G", ParameterType = "Late reporting tonnage", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 7, 23, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 7, 23, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "BADEBT-P", ParameterValue = 5.25m, ParameterCategory = "Percentage", ParameterType = "Bad debt provision", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 24, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 24, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "MATT-AI", ParameterValue = 5000.00m, ParameterCategory = "Amount Increase", ParameterType = "Materiality threshold", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 26, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 26, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "MATT-AD", ParameterValue = -1000.00m, ParameterCategory = "Amount Decrease", ParameterType = "Materiality threshold", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 27, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 27, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "MATT-PI", ParameterValue = 2.00m, ParameterCategory = "Percent Increase", ParameterType = "Materiality threshold", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 10, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 10, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "MATT-PD", ParameterValue = -1.00m, ParameterCategory = "Percent Decrease", ParameterType = "Materiality threshold", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 12, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 12, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "TONT-AI", ParameterValue = 50.00m, ParameterCategory = "Amount Increase", ParameterType = "Tonnage change threshold", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 13, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 13, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "TONT-AD", ParameterValue = -10.00m, ParameterCategory = "Amount Decrease", ParameterType = "Tonnage change threshold", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 15, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 15, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "TONT-PI", ParameterValue = 2.00m, ParameterCategory = "Percent Increase", ParameterType = "Tonnage change threshold", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 17, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 17, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "TONT-PD", ParameterValue = -0.50m, ParameterCategory = "Percent Decrease", ParameterType = "Tonnage change threshold", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 18, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 18, 10, 12, 30, DateTimeKind.Utc) },
             new DefaultSchemeParameters { ParameterUniqueRef = "REDM-RF", ParameterValue = 1.200m, ParameterCategory = "Modulation Factor", ParameterType = "Red modulation factor", RelativeYear =  new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 18, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 18, 10, 12, 30, DateTimeKind.Utc) },
            ]);

            return defaultParameterValues;
        }

        public static CalculatorRunDto GetCalculatorRun()
        {
            return new CalculatorRunDto
            {
                RunId = 1,
                RunName = "Test Run",
                RunClassificationId = 3,
                RunClassificationStatus = "UNCLASSIFIED",
                FileExtension = "CSV",
                CreatedAt = DateTime.Parse("21/06/2024 12:09:00", new CultureInfo("en-GB")),
                RelativeYear = new RelativeYear(2024),
            };
        }

        public static CalculatorRunDto GetInitialRunCalculatorRun()
        {
            return new CalculatorRunDto
            {
                RunId = 1,
                RunName = "Test Run",
                RunClassificationId = 8,
                RunClassificationStatus = "INITIAL RUN",
                FileExtension = "CSV",
                CreatedAt = DateTime.Parse("21/06/2024 12:09:00", new CultureInfo("en-GB")),
                RelativeYear = new RelativeYear(2024),
            };
        }

        public static CalculatorRunDto GetCalculatorRunWithInitialRun()
        {
            return new CalculatorRunDto
            {
                RunId = 1,
                RunName = "Test Run",
                RunClassificationId = 8,
                RunClassificationStatus = "INITIAL RUN CLASSIFIED",
                FileExtension = "CSV",
                CreatedAt = DateTime.Parse("21/06/2024 12:09:00", new CultureInfo("en-GB")),
                RelativeYear = new RelativeYear(2024),
            };
        }

        public static CalculatorRunDto GetRunningCalculatorRun()
        {
            return new CalculatorRunDto
            {
                RunId = 1,
                RunName = "Test Run",
                RunClassificationId = 2,
                RunClassificationStatus = "RUNNING",
                FileExtension = "CSV",
                CreatedAt = DateTime.Parse("21/06/2024 12:09:00", new CultureInfo("en-GB")),
                RelativeYear = new RelativeYear(2024),
            };
        }

        public static IEnumerable<CalculationRun> GetCalculationRuns()
        {
            var calculationRuns = new List<CalculationRun>();

            calculationRuns.AddRange([
                new CalculationRun { Id = 1, CalculatorRunClassificationId = Enums.RunClassification.QUEUE, Name = "Default cettings check", CreatedAt = DateTime.Parse("28/06/2025 10:01:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", RelativeYear = new RelativeYear(2024) },
                new CalculationRun { Id = 2, CalculatorRunClassificationId = Enums.RunClassification.RUNNING, Name = "Alteration check", CreatedAt = DateTime.Parse("28/06/2025 12:19:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", RelativeYear = new RelativeYear(2024) },
                new CalculationRun { Id = 3, CalculatorRunClassificationId = Enums.RunClassification.UNCLASSIFIED, Name = "Test 10", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", RelativeYear = new RelativeYear(2024) },
                new CalculationRun { Id = 4, CalculatorRunClassificationId = Enums.RunClassification.TEST_RUN, Name = "June check", CreatedAt = DateTime.Parse("11/06/2025 09:14:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", RelativeYear = new RelativeYear(2024) },
                new CalculationRun { Id = 5, CalculatorRunClassificationId = Enums.RunClassification.TEST_RUN, Name = "Pre June check", CreatedAt = DateTime.Parse("13/06/2025 11:18:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", RelativeYear = new RelativeYear(2024) },
                new CalculationRun { Id = 6, CalculatorRunClassificationId = Enums.RunClassification.TEST_RUN, Name = "Local Authority data check 5", CreatedAt = DateTime.Parse("10/06/2025 08:13:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", RelativeYear = new RelativeYear(2024) },
                new CalculationRun { Id = 7, CalculatorRunClassificationId = Enums.RunClassification.TEST_RUN, Name = "Local Authority data check 4", CreatedAt = DateTime.Parse("10/06/2025 10:14:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", RelativeYear = new RelativeYear(2024) },
                new CalculationRun { Id = 8, CalculatorRunClassificationId = Enums.RunClassification.TEST_RUN, Name = "Local Authority data check 3", CreatedAt = DateTime.Parse("08/06/2025 10:00:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", RelativeYear = new RelativeYear(2024) },
                new CalculationRun { Id = 9, CalculatorRunClassificationId = Enums.RunClassification.TEST_RUN, Name = "Local Authority data check 2", CreatedAt = DateTime.Parse("06/06/2025 11:20:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", RelativeYear = new RelativeYear(2024) },
                new CalculationRun { Id = 10, CalculatorRunClassificationId = Enums.RunClassification.TEST_RUN, Name = "Local Authority data check", CreatedAt = DateTime.Parse("02/06/2025 12:02:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", RelativeYear = new RelativeYear(2024) },
                new CalculationRun { Id = 11, CalculatorRunClassificationId = Enums.RunClassification.ERROR, Name = "Fee adjustment check", CreatedAt = DateTime.Parse("01/06/2025 09:12:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", RelativeYear = new RelativeYear(2024) },
                new CalculationRun { Id = 12, CalculatorRunClassificationId = Enums.RunClassification.DELETED, Name = "Deleted Run", CreatedAt = DateTime.Parse("01/06/2025 09:12:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", RelativeYear = new RelativeYear(2024) }
            ]);

            return calculationRuns;
        }

        public static IEnumerable<LocalAuthorityDisposalCost> GetLocalAuthorityDisposalCosts()
        {
            var localAuthorityDisposalCosts = new List<LocalAuthorityDisposalCost>();

            localAuthorityDisposalCosts.AddRange([
                new LocalAuthorityDisposalCost { Id = 1, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "ENG-AL", Country = "England", Material = "Aluminium", TotalCost = 2210.45m },
                new LocalAuthorityDisposalCost { Id = 2, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "ENG-FC", Country = "England", Material = "Fibre composite", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 3, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "ENG-GL", Country = "England", Material = "Glass", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 4, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "ENG-PC", Country = "England", Material = "Paper or card", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 5, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "ENG-PL", Country = "England", Material = "Plastic", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 6, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "ENG-ST", Country = "England", Material = "Steel", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 7, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "ENG-WD", Country = "England", Material = "Wood", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 8, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "ENG-OT", Country = "England", Material = "Other", TotalCost = 0m },
                new LocalAuthorityDisposalCost { Id = 9, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "NI-AL", Country = "NI", Material = "Aluminium", TotalCost = 10m },
                new LocalAuthorityDisposalCost { Id = 10, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "NI-FC", Country = "NI", Material = "Fibre composite", TotalCost = 11m },
                new LocalAuthorityDisposalCost { Id = 11, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "NI-GL", Country = "NI", Material = "Glass", TotalCost = 12m },
                new LocalAuthorityDisposalCost { Id = 12, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "NI-PC", Country = "NI", Material = "Paper or card", TotalCost = 13m },
                new LocalAuthorityDisposalCost { Id = 13, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "NI-PL", Country = "NI", Material = "Plastic", TotalCost = 14m },
                new LocalAuthorityDisposalCost { Id = 14, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "NI-ST", Country = "NI", Material = "Steel", TotalCost = 15m },
                new LocalAuthorityDisposalCost { Id = 15, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "NI-WD", Country = "NI", Material = "Wood", TotalCost = 16m },
                new LocalAuthorityDisposalCost { Id = 16, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "NI-OT", Country = "NI", Material = "Other", TotalCost = 17m },
                new LocalAuthorityDisposalCost { Id = 17, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "SCT-AL", Country = "Scotland", Material = "Aluminium", TotalCost = 20.01m },
                new LocalAuthorityDisposalCost { Id = 18, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "SCT-FC", Country = "Scotland", Material = "Fibre composite", TotalCost = 21.01m },
                new LocalAuthorityDisposalCost { Id = 19, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "SCT-GL", Country = "Scotland", Material = "Glass", TotalCost = 22.01m },
                new LocalAuthorityDisposalCost { Id = 20, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "SCT-PC", Country = "Scotland", Material = "Paper or card", TotalCost = 23.01m },
                new LocalAuthorityDisposalCost { Id = 21, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "SCT-PL", Country = "Scotland", Material = "Plastic", TotalCost = 24.01m },
                new LocalAuthorityDisposalCost { Id = 22, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "SCT-ST", Country = "Scotland", Material = "Steel", TotalCost = 25.01m },
                new LocalAuthorityDisposalCost { Id = 23, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "SCT-WD", Country = "Scotland", Material = "Wood", TotalCost = 26.01m },
                new LocalAuthorityDisposalCost { Id = 24, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "SCT-OT", Country = "Scotland", Material = "Other", TotalCost = 27.01m },
                new LocalAuthorityDisposalCost { Id = 25, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "WLS-AL", Country = "Wales", Material = "Aluminium", TotalCost = 30.01m },
                new LocalAuthorityDisposalCost { Id = 26, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "WLS-FC", Country = "Wales", Material = "Fibre composite", TotalCost = 30.02m },
                new LocalAuthorityDisposalCost { Id = 27, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "WLS-GL", Country = "Wales", Material = "Glass", TotalCost = 30.03m },
                new LocalAuthorityDisposalCost { Id = 28, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "WLS-PC", Country = "Wales", Material = "Paper or card", TotalCost = 30.04m },
                new LocalAuthorityDisposalCost { Id = 29, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "WLS-PL", Country = "Wales", Material = "Plastic", TotalCost = 30.05m },
                new LocalAuthorityDisposalCost { Id = 30, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "WLS-ST", Country = "Wales", Material = "Steel", TotalCost = 30.06m },
                new LocalAuthorityDisposalCost { Id = 31, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "WLS-WD", Country = "Wales", Material = "Wood", TotalCost = 30.07m },
                new LocalAuthorityDisposalCost { Id = 32, RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapTempUniqueRef = "WLS-OT", Country = "Wales", Material = "Other", TotalCost = 30.08m },
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
