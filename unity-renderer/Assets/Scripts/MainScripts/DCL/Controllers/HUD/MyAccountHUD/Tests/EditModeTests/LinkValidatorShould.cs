using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.MyAccount
{
    public class LinkValidatorShould
    {
        [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]
        [TestCase("https://alink.com?aVar=aValue&anotherVar=anotherValue", true)]
        [TestCase("https://alink.com?someVar=some%20value", true)]
        [TestCase("https://alink.com", true)]
        [TestCase("http://alink.com'", true)]
        [TestCase("javascript:window%5b%22ale%22%2b%22rt%22%5d(document.domain)//.com", false)]
        public void IsValid(string url, bool expectedResult)
        {
            Assert.True(LinkValidator.IsValid(url) == expectedResult);
        }
    }
}
