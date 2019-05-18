namespace SMLHelper.Tests
{
    using NUnit.Framework;
    using SMLHelper.V2.Patchers;
    using System;
    using System.Collections.Generic;
    using System.Text;

    [TestFixture]
    public class LanguagePatcherTests
    {
        [TestCase("CustomValue")]
        [TestCase("CustomValue{0}")]
        [TestCase("{0}CustomValue")]
        [TestCase("{0}CustomValue{1}")]
        [TestCase("Custom{0}Value")]
        [TestCase("Custom\nValue")]
        [TestCase("Custom{0}\n{1}Value")] // <-- Currently failing
        public void ExtractCustomLinesFromText_WhenTextIsValid_SingleEntry_KeyIsKnown_Overrides(string customValue)
        {
            var originalLines = new Dictionary<string, string>
            {
                { "Key", "OriginalValue" }
            };

            string text = "Key:{" + customValue + "}";

            int overridesApplied = LanguagePatcher.ExtractCustomLinesFromText("Test1", text, originalLines);

            Assert.AreEqual(1, overridesApplied);
            Assert.AreEqual(customValue, LanguagePatcher.GetCustomLine("Key"));
        }
    }
}
