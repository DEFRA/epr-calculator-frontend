// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1200:Using directives should be placed correctly", Justification = "Using the .Net environment default settings")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1208:System using directives should be placed before other using directives", Justification = "Using the .Net environment default settings")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:File should have header", Justification = "Using the .Net environment default settings")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1206:Declaration keywords should follow order", Justification = "Using the .Net environment default settings")]
[assembly: SuppressMessage("Minor Code Smell", "S6603:The collection-specific \"TrueForAll\" method should be used instead of the \"All\" extension", Justification = "<Not valid for CsvHelper library>", Scope = "member", Target = "~M:EPR.Calculator.Frontend.Helpers.CsvFileHelper.GetCsvConfiguration(EPR.Calculator.Frontend.Enums.UploadType)~CsvHelper.Configuration.CsvConfiguration")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1204:Static elements should appear before instance elements", Justification = "Statuc method positioning on top of controller file not required", Scope = "member", Target = "~M:EPR.Calculator.Frontend.Controllers.DashboardController.GetCalulationRunsData(System.Collections.Generic.List{EPR.Calculator.Frontend.Models.CalculationRun})~System.Collections.Generic.List{EPR.Calculator.Frontend.ViewModels.DashboardViewModel.CalculationRunViewModel}")]
