setTimeout(function(){
  var enabled = typeof CachedXMLHttpRequest !== "undefined" && !!CachedXMLHttpRequest.cache;
  console.log("CachedXHRExtensions initialized, enabled=" + enabled);
  var CachedXHRExtensions = function() {
    this._cacheStates = {};
  };

  CachedXHRExtensions.prototype.searchCache = function(url) {
    if (!enabled) return;
    var self = this;
    url = typeof url === "string" ? url : Pointer_stringify(url);
    delete this._cacheStates[url];

    CachedXMLHttpRequest.cache.get(url, function(err, result) {
      if (err || !result || !result.meta) {
        self._cacheStates[url] = false;
      } else {
        self._cacheStates[url] = true;
      }
    });
  };

  CachedXHRExtensions.prototype.checkStatus = function(url) {
    if (!enabled) return -1;

    url = typeof url === "string" ? url : Pointer_stringify(url);
    if (this._cacheStates[url] === undefined) return 0;
    return this._cacheStates[url] ? 1 : -1;
  };

  window.CachedXHRExtensions = new CachedXHRExtensions();
}, 0);
