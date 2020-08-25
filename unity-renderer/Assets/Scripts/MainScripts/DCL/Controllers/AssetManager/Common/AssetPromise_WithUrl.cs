namespace DCL
{
    public abstract class AssetPromise_WithUrl<T> : AssetPromise<T> where T : Asset, new()
    {
        public string contentUrl;
        public string hash;

        public AssetPromise_WithUrl(string contentUrl, string hash)
        {
            this.contentUrl = contentUrl;
            this.hash = hash;
        }

        public override object GetId()
        {
            return hash;
        }
    }
}