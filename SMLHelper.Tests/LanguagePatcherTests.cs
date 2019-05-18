namespace SMLHelper.Tests
{
    using NUnit.Framework;
    using SMLHelper.V2.Patchers;
    using System;
    using System.Collections.Generic;

    [TestFixture]
    public class LanguagePatcherTests
    {
        private static readonly IEnumerable<string> CustomValues1 = new string[]
        {
            "CustomValue1", "CustomValue1{0}", "{0}CustomValue1", "{0}CustomValue1{1}",
            "{0}Custom{1}Value1{2}", "Custom{0}Value1", "Custom\nValue1", "\nCustomValue1",
            "CustomValue1\n", "\nCustom\nValue1\n", "Custom{0}\n{1}Value1",
            "Custom-Value1\n", "\nCustom_Value1\n", "Custom{0}:{1}Value1",
        };

        private static readonly IEnumerable<string> CustomValues2 = new string[]
        {
            "2CustomValue", "2CustomValue{0}", "{0}2CustomValue", "{0}2CustomValue{1}",
            "{0}2Custom{1}Value{2}", "2Custom{0}Value", "2Custom\nValue", "\n2CustomValue",
            "2CustomValue\n", "2\nCustom\nValue\n", "2Custom{0}\n{1}Value",
            "2Custom-Value\n", "2\nCustom_Value\n", "2Custom{0}:{1}Value",
        };

        [TestCaseSource(nameof(CustomValues1))]
        public void ExtractCustomLinesFromText_WhenTextIsValid_SingleEntry_KeyIsKnown_Overrides(string customValue)
        {
            var originalLines = new Dictionary<string, string>
            {
                { "Key", "OriginalValue" }
            };

            string text = "Key:{" + customValue + "}";

            Console.WriteLine("TestText");
            Console.WriteLine(text);
            int overridesApplied = LanguagePatcher.ExtractCustomLinesFromText("Test1", text, originalLines);

            Assert.AreEqual(1, overridesApplied);
            Assert.AreEqual(customValue, LanguagePatcher.GetCustomLine("Key"));
        }


        [Test, Combinatorial]
        public void ExtractCustomLinesFromText_WhenTextIsValid_MultipleEntries_KeyIsKnown_Overrides(
            [Values("\n", "\r\n")] string endOfLine,
            [ValueSource(nameof(CustomValues1))] string customValue1,
            [ValueSource(nameof(CustomValues2))] string customValue2)
        {
            var originalLines = new Dictionary<string, string>
            {
                { "Key1", "OriginalValue1" },
                { "Key2", "OriginalValue2" },
            };

            string text = "Key1:{" + customValue1 + "}" + endOfLine +
                          "Key2:{" + customValue2 + "}" + endOfLine;
            Console.WriteLine("TestText");
            Console.WriteLine(text);
            int overridesApplied = LanguagePatcher.ExtractCustomLinesFromText("Test1", text, originalLines);

            Assert.AreEqual(2, overridesApplied);
            Assert.AreEqual(customValue1, LanguagePatcher.GetCustomLine("Key1"));
            Assert.AreEqual(customValue2, LanguagePatcher.GetCustomLine("Key2"));
        }
    }
}
