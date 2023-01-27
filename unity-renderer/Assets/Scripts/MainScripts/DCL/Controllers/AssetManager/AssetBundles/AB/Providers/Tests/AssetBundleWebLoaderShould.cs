using NUnit.Framework;
using UnityEngine;

namespace DCL.Providers.Tests
{
    [TestFixture]
    public class AssetBundleWebLoaderShould
    {
        [Test]
        public void ComputeVersionedHash()
        {
            const string CONTENT_URL = "https://content-assets-as-bundle.decentraland.org/v125/";
            const string HASH = "QmfNvE3nKmahA5emnBnXN2LzydpYncHVz4xy4piw84Er1D";

            Assert.AreEqual(Hash128.Compute("v125QmfNvE3nKmahA5emnBnXN2LzydpYncHVz4xy4piw84Er1D"), AssetBundleWebLoader.ComputeHash(CONTENT_URL, HASH));
        }

        [Test]
        public void ComputeUnversionedHash()
        {
            const string CONTENT_URL = "https://content-assets-as-bundle.decentraland.org/";
            const string HASH = "QmfNvE3nKmahA5emnBnXN2LzydpYncHVz4xy4piw84Er1D";

            Assert.AreEqual(Hash128.Compute("QmfNvE3nKmahA5emnBnXN2LzydpYncHVz4xy4piw84Er1D"), AssetBundleWebLoader.ComputeHash(CONTENT_URL, HASH));
        }
    }
}
