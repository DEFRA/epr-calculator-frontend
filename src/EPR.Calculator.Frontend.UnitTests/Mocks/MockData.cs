﻿using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.UnitTests.Mocks
{
    public static class MockData
    {
        public static IEnumerable<SchemeParameterTemplateValue> GetSchemeParameterTemplateValues()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValue>();

            schemeParameterTemplateValues.AddRange([
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-AL", ParameterValue = 2210.45m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-FC", ParameterValue = 2210.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-GL", ParameterValue = 2210.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-PC", ParameterValue = 2210.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "OMC-PL", ParameterValue = 2210.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-ST", ParameterValue = 2210.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-WD", ParameterValue = 2210.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "COMC-OT", ParameterValue = 0.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SAOC-ENG", ParameterValue = 500.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SAOC-WLS", ParameterValue = 140.55m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SAOC-SCT", ParameterValue = 170.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SAOC-NIR", ParameterValue = 90.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LAPC-ENG", ParameterValue = 115.45m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LAPC-WLS", ParameterValue = 114.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LAPC-SCT", ParameterValue = 117.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LAPC-NIR", ParameterValue = 19.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SCSC-ENG", ParameterValue = 325.55m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SCSC-WLS", ParameterValue = 314.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SCSC-SCT", ParameterValue = 317.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "SCSC-NIR", ParameterValue = 391.10m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-AL", ParameterValue = 70.55m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-FC", ParameterValue = 80.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-GL", ParameterValue = 60.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-PC", ParameterValue = 50.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-PL", ParameterValue = 40.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-ST", ParameterValue = 30.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-WD", ParameterValue = 20.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LRET-OT", ParameterValue = 0.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "BADEBT-P", ParameterValue = 5.25m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "MATT-AI", ParameterValue = 5000.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "MATT-AD", ParameterValue = -1000.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "MATT-PI", ParameterValue = 2.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "MATT-PD", ParameterValue = -1.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "TONT-AI", ParameterValue = 50.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "TONT-AD", ParameterValue = -10.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "TONT-PI", ParameterValue = 2.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "TONT-PD", ParameterValue = -0.50m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LEVY-ENG", ParameterValue = 115.45m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LEVY-WLS", ParameterValue = 114.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LEVY-SCT", ParameterValue = 117.00m },
                new SchemeParameterTemplateValue { ParameterUniqueReferenceId = "LEVY-NIR", ParameterValue = 19.00m },
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
    }
}
