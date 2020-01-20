This plugin provides additional functionality on top of the CachedXMLHttpRequest
addon. The classes referred to below are in the Kongregate namespace.

* Clearing the cache by calling CachedXHRExtensions.CleanCache()
* Asynchronously querying the cache to determine if an item exists:
  IEnumerator CheckIfAssetExists() {
    var query = new CacheEntryQuery("https://whatever.io/file.xml");
    yield return query;
    if (query.IsCached) {
      Debug.Log("Asset exists in cache!");
    }
  }
