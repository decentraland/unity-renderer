using System.Collections.Generic;
using DCL;
using NUnit.Framework;

public class ContentProviderShould
{
    private ContentProvider contentProvider;

    [SetUp]
    public void SetUp() { contentProvider = new ContentProvider(); }

    [Test]
    public void NotAddFilesWithTheSameCaseInsensitiveName()
    {
        contentProvider.contents = new List<ContentServerUtils.MappingPair>()
        {
            new ContentServerUtils.MappingPair { file = "file1", hash = "file1Hash" },
            new ContentServerUtils.MappingPair { file = "File1", hash = "File1Hash" },
            new ContentServerUtils.MappingPair { file = "file2", hash = "file2Hash" },
            new ContentServerUtils.MappingPair { file = "File2", hash = "File2Hash" },
        };

        contentProvider.BakeHashes();

        Assert.IsTrue(contentProvider.fileToHash.ContainsKey("file1"));
        Assert.AreEqual(contentProvider.fileToHash["file1"], "file1Hash");
        Assert.IsFalse(contentProvider.fileToHash.ContainsKey("File1"));

        Assert.IsTrue(contentProvider.fileToHash.ContainsKey("file2"));
        Assert.AreEqual(contentProvider.fileToHash["file2"], "file2Hash");
        Assert.IsFalse(contentProvider.fileToHash.ContainsKey("File2"));
    }
}