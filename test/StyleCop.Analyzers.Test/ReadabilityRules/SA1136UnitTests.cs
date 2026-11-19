// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Test.ReadabilityRules
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Testing;
    using StyleCop.Analyzers.ReadabilityRules;
    using StyleCop.Analyzers.Test.Helpers;
    using Xunit;
    using static StyleCop.Analyzers.Test.Verifiers.StyleCopCodeFixVerifier<
        StyleCop.Analyzers.ReadabilityRules.SA1136EnumValuesShouldBeOnSeparateLines,
        StyleCop.Analyzers.ReadabilityRules.SA1136CodeFixProvider>;

    /// <summary>
    /// Unit tests for the <see cref="SA1136EnumValuesShouldBeOnSeparateLines"/> analyzer.
    /// </summary>
    public class SA1136UnitTests
    {
        /// <summary>
        /// Verifies that an enum declaration with all values on the same line will return the expected diagnostics.
        /// This also verifies that an enum declaration with all values on separate lines will not produce diagnostics.
        /// </summary>
        /// <param name="lineEnding">The line ending to use in the test code.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [MemberData(nameof(CommonData.EndOfLineSequences), MemberType = typeof(CommonData))]
        public async Task TestSingleLineEnumDeclarationAsync(string lineEnding)
        {
            var testCode = @"
public enum TestEnum
{
    FirstValue, {|#0:SecondValue|}, {|#1:ThirdValue|}
}
".ReplaceLineEndings(lineEnding);

            var fixedTestCode = @"
public enum TestEnum
{
    FirstValue,
    SecondValue,
    ThirdValue
}
".ReplaceLineEndings(lineEnding);

            DiagnosticResult[] expected =
            {
                Diagnostic().WithLocation(0),
                Diagnostic().WithLocation(1),
            };

            await VerifyCSharpFixAsync(testCode, expected, fixedTestCode).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that comments after a enum value declaration will not produce diagnostics.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestEndOfLineCommentsAsync()
        {
            var testCode = @"
public enum TestEnum
{
    FirstValue, /* comment 1 */
    SecondValue, // comment 2
}
";

            await VerifyCSharpDiagnosticAsync(testCode, DiagnosticResult.EmptyDiagnosticResults).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that inline comments between enum value declarations are handled properly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestInlineCommentsAsync()
        {
            var testCode = @"
public enum TestEnum
{
    FirstValue, /* comment */ SecondValue,
}
";

            var fixedTestCode = @"
public enum TestEnum
{
    FirstValue, /* comment */
    SecondValue,
}
";

            DiagnosticResult[] expected =
            {
                Diagnostic().WithLocation(4, 31),
            };

            await VerifyCSharpFixAsync(testCode, expected, fixedTestCode).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that values on the same line within a directive trivia are handled correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestDirectiveTriviaAreHandledProperlyAsync()
        {
            var testCode = @"
public enum TestEnum
{
    FirstValue,
#if !TEST
    SecondValue, ThirdValue,
#endif
    FourthValue
}
";

            var fixedTestCode = @"
public enum TestEnum
{
    FirstValue,
#if !TEST
    SecondValue,
    ThirdValue,
#endif
    FourthValue
}
";

            DiagnosticResult[] expected =
            {
                Diagnostic().WithLocation(6, 18),
            };

            await VerifyCSharpFixAsync(testCode, expected, fixedTestCode).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that an enum declaration without a block is handled correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestEnumWithoutBlockAsync()
        {
            var testCode = @"
public enum TestEnum
";

            DiagnosticResult[] expected =
            {
                DiagnosticResult.CompilerError("CS1513").WithMessage("} expected").WithLocation(2, 21),
                DiagnosticResult.CompilerError("CS1514").WithMessage("{ expected").WithLocation(2, 21),
            };

            await VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that an enum member whose name is missing, as while typing a second comma or an attribute, is not
        /// reported, and that a member that does share a line is still reported and fixed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestMissingMemberNameAsync()
        {
            var testCode = @"using System;

public enum TestEnum1
{
    One, {|#0:Two|},,
}

public enum TestEnum2
{
    One,
    Two, [Obsolete]
}
";

            var fixedCode = @"using System;

public enum TestEnum1
{
    One,
    Two,,
}

public enum TestEnum2
{
    One,
    Two, [Obsolete]
}
";

            var test = new CSharpTest
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            test.ExpectedDiagnostics.Add(Diagnostic().WithLocation(0));
            test.ExpectedDiagnostics.Add(DiagnosticResult.CompilerError("CS1001").WithLocation(5, 14));
            test.ExpectedDiagnostics.Add(DiagnosticResult.CompilerError("CS1001").WithLocation(11, 20));
            test.FixedState.ExpectedDiagnostics.Add(DiagnosticResult.CompilerError("CS1001").WithLocation(6, 9));
            test.FixedState.ExpectedDiagnostics.Add(DiagnosticResult.CompilerError("CS1001").WithLocation(12, 20));

            await test.RunAsync().ConfigureAwait(false);
        }
    }
}
