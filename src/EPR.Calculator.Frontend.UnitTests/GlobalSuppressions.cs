﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Minor Code Smell", "S6608:Prefer indexing instead of \"Enumerable\" methods on types implementing \"IList\"", Justification = "Using the .Net provided IEnumerable method instead of index", Scope = "member", Target = "~M:EPR.Calculator.Frontend.UnitTests.DashboardControllerTests.Should_Classify_CalculationRuns_And_Handle_DefaultValue")]
