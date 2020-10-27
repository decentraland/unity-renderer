using NUnit.Framework;

namespace DCL.ABConverter {
public class ParseOptionShould
{
    readonly static string[] args = new string[] {"-unityRandomOption", "-testOption", "arg1", "arg2", "-garbage", "-garbage2"};

    [Test]
    public void FailWhenNoOptionsAreFound()
    {
        Assert.IsFalse(DCL.ABConverter.Utils.ParseOptionExplicit(args, null, 0, out string[] test));
        Assert.IsFalse(DCL.ABConverter.Utils.ParseOptionExplicit(args, "blah", 0, out string[] test2));
        Assert.IsTrue(test == null);
        Assert.IsTrue(test2 == null);
    }

    [Test]
    public void FailWhenTooManyArgumentsAreGiven()
    {
        Assert.IsFalse(DCL.ABConverter.Utils.ParseOptionExplicit(args, "testOption", 5, out string[] test5));
        Assert.IsFalse(DCL.ABConverter.Utils.ParseOptionExplicit(args, null, 5, out string[] test6));
        Assert.IsTrue(test5 == null);
        Assert.IsTrue(test6 == null);
    }

    [Test]
    [TestCase(null, null, -1)]
    [TestCase(null, "asdasdasd", -1)]
    [TestCase(null, "asdasdasd", int.MaxValue)]
    public void NotCrashWhenInvalidArgsAreGiven(string[] rawArgsList, string optionName, int expectedArgsQty)
    {
        Assert.IsFalse(DCL.ABConverter.Utils.ParseOptionExplicit(rawArgsList, optionName, expectedArgsQty, out _));
    }

    [Test]
    public void SucceedWhenOptionsAreFound()
    {
        Assert.IsTrue(DCL.ABConverter.Utils.ParseOptionExplicit(args, "testOption", 0, out string[] test));
        Assert.IsTrue(test == null);
    }

    [Test]
    public void SucceedExtractingArguments()
    {
        if (DCL.ABConverter.Utils.ParseOptionExplicit(args, "testOption", 1, out string[] test))
        {
            Assert.IsTrue(test != null);
            Assert.IsTrue(test.Length == 1);
            Assert.IsTrue(test[0] == "arg1");
        }

        if (DCL.ABConverter.Utils.ParseOptionExplicit(args, "testOption", 2, out string[] test2))
        {
            Assert.IsTrue(test2 != null);
            Assert.IsTrue(test2.Length == 2);
            Assert.IsTrue(test2[0] == "arg1");
            Assert.IsTrue(test2[1] == "arg2");
        }
    }
}
}