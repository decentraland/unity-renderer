mergeInto(LibraryManager.library, {
  CachedXHRExtensions_SearchCache: function(url) { CachedXHRExtensions.searchCache(url); },
  CachedXHRExtensions_CheckStatus: function(url) { return CachedXHRExtensions.checkStatus(url); },
  CachedXHRExtensions_CleanCache: function() {
    try {
      var self = CachedXMLHttpRequest.cache;
      self.db.transaction([self.store], "readwrite").objectStore(self.store).clear().onerror = function(){
        e.preventDefault();
      };
    } catch(e) {}
  }
});
