function CachedXMLHttpRequest() {
  var self = this,
    xhr = new CachedXMLHttpRequest.XMLHttpRequest(),
    cache = {};
  function send() {
    var onload = xhr.onload;
    xhr.onload = function(e) {
      var meta = {
        requestURL: cache.requestURL,
        responseURL: xhr.responseURL,
        responseType: xhr.responseType,
        lastModified: xhr.getResponseHeader("Last-Modified"),
        eTag: xhr.getResponseHeader("ETag")
      };
      if (xhr.status == 200 && (meta.lastModified || meta.eTag)) {
        meta.size = xhr.response.byteLength;
        CachedXMLHttpRequest.cache.put(
          cache.requestURL,
          meta,
          xhr.response,
          function(err) {
            CachedXMLHttpRequest.log(
              "'" +
                cache.requestURL +
                "' downloaded successfully (" +
                (xhr.response && xhr.response.byteLength) +
                " bytes) " +
                (err
                  ? "but not stored in indexedDB cache due to error."
                  : "and stored in indexedDB cache.")
            );
            if (onload) onload(e);
          }
        );
      } else {
        if (xhr.status == 304) {
          cache.override = true;
          CachedXMLHttpRequest.log(
            "'" +
              cache.requestURL +
              "' served from indexedDB cache (" +
              cache.response.byteLength +
              " bytes)."
          );
        }
        if (onload) onload(e);
      }
    };
    return xhr.send.apply(xhr, arguments);
  }
  function revalidateCrossOriginRequest(meta, self, sendArguments) {
    var headXHR = new CachedXMLHttpRequest.XMLHttpRequest();
    headXHR.open("HEAD", meta.requestURL, true);
    headXHR.onload = function() {
      cache.override = meta.lastModified
        ? meta.lastModified == headXHR.getResponseHeader("Last-Modified")
        : meta.eTag && meta.eTag == headXHR.getResponseHeader("ETag");
      if (!cache.override) return send.apply(self, sendArguments);
      CachedXMLHttpRequest.log(
        "'" +
          cache.requestURL +
          "' served from indexedDB cache (" +
          cache.response.byteLength +
          " bytes)."
      );
      if (xhr.onload) xhr.onload();
    };
    headXHR.send();
  }
  Object.defineProperty(self, "open", {
    value: function(method, url, async) {
      cache = {
        method: method,
        requestURL: CachedXMLHttpRequest.cache.requestURL(url),
        async: async
      };
      return xhr.open.apply(xhr, arguments);
    }
  });
  Object.defineProperty(self, "setRequestHeader", {
    value: function() {
      cache.customHeaders = true;
      return xhr.setRequestHeader.apply(xhr, arguments);
    }
  });
  Object.defineProperty(self, "send", {
    value: function(data) {
      var sendArguments = arguments;
      var absoluteUrlMatch = cache.requestURL.match("^https?://[^/]+/");
      if (
        !absoluteUrlMatch ||
        cache.customHeaders ||
        data ||
        cache.method != "GET" ||
        !cache.async ||
        xhr.responseType != "arraybuffer"
      )
        return xhr.send.apply(xhr, sendArguments);
      CachedXMLHttpRequest.cache.get(cache.requestURL, function(err, result) {
        if (
          err ||
          !result ||
          !result.meta ||
          result.meta.responseType != xhr.responseType
        )
          return send.apply(self, sendArguments);
        cache.status = 200;
        cache.statusText = "OK";
        cache.response = result.response;
        cache.responseURL = result.meta.responseURL;
        if (window.location.href.lastIndexOf(absoluteUrlMatch[0], 0))
          return revalidateCrossOriginRequest(result.meta, self, sendArguments);
        if (result.meta.lastModified)
          xhr.setRequestHeader("If-Modified-Since", result.meta.lastModified);
        else if (result.meta.eTag)
          xhr.setRequestHeader("If-None-Match", result.meta.eTag);
        xhr.setRequestHeader("Cache-Control", "no-cache");
        return send.apply(self, sendArguments);
      });
    }
  });
  [
    "abort",
    "getAllResponseHeaders",
    "getResponseHeader",
    "overrideMimeType",
    "addEventListener"
  ].forEach(function(method) {
    Object.defineProperty(self, method, {
      value: function() {
        return xhr[method].apply(xhr, arguments);
      }
    });
  });
  [
    "readyState",
    "response",
    "responseText",
    "responseType",
    "responseURL",
    "responseXML",
    "status",
    "statusText",
    "timeout",
    "upload",
    "withCredentials",
    "onloadstart",
    "onprogress",
    "onabort",
    "onerror",
    "onload",
    "ontimeout",
    "onloadend",
    "onreadystatechange"
  ].forEach(function(property) {
    Object.defineProperty(self, property, {
      get: function() {
        return cache.override && cache[property]
          ? cache[property]
          : xhr[property];
      },
      set: function(value) {
        xhr[property] = value;
      }
    });
  });
}
CachedXMLHttpRequest.XMLHttpRequest = window.XMLHttpRequest;
CachedXMLHttpRequest.log = function(message) {
  if (Module.CachedXMLHttpRequestSilent !== true)
    console.log("[CachedXMLHttpRequest] " + message);
};
CachedXMLHttpRequest.cache = {
  database: "CachedXMLHttpRequest",
  version: 1,
  store: "cache",
  indexedDB:
    window.indexedDB ||
    window.mozIndexedDB ||
    window.webkitIndexedDB ||
    window.msIndexedDB,
  link: document.createElement("a"),
  requestURL: function(url) {
    this.link.href = url;
    return this.link.href;
  },
  id: function(requestURL) {
    return encodeURIComponent(requestURL);
  },
  queue: [],
  processQueue: function() {
    var self = this;
    self.queue.forEach(function(queued) {
      self[queued.action].apply(self, queued.arguments);
    });
    self.queue = [];
  },
  init: function() {
    var self = this,
      onError = function(e) {
        CachedXMLHttpRequest.log(
          "can not open indexedDB database: " + e.message
        );
        self.indexedDB = null;
        self.processQueue();
        if (e.preventDefault) e.preventDefault();
      };
    if (!self.indexedDB)
      return CachedXMLHttpRequest.log("indexedDB is not available");
    var openDB;
    try {
      openDB = indexedDB.open(self.database, self.version);
    } catch (e) {
      return onError(new Error("indexedDB access denied"));
    }
    openDB.onupgradeneeded = function(e) {
      var db = e.target.result;
      var transaction = e.target.transaction;
      var objectStore;
      if (db.objectStoreNames.contains(self.store)) {
        objectStore = transaction.objectStore(self.store);
      } else {
        objectStore = db.createObjectStore(self.store, { keyPath: "id" });
        objectStore.createIndex("meta", "meta", { unique: false });
      }
      objectStore.clear();
    };
    openDB.onerror = onError;
    openDB.onsuccess = function(e) {
      self.db = e.target.result;
      self.processQueue();
    };
  },
  put: function(requestURL, meta, response, callback) {
    var self = this;
    if (!self.indexedDB)
      return callback(new Error("indexedDB is not available"));
    if (!self.db)
      return self.queue.push({ action: "put", arguments: arguments });
    meta.version = self.version;
    var putDB = self.db
      .transaction([self.store], "readwrite")
      .objectStore(self.store)
      .put({ id: self.id(requestURL), meta: meta, response: response });
    putDB.onerror = function(e) {
      e.preventDefault();
      callback(new Error("failed to put request into indexedDB cache"));
    };
    putDB.onsuccess = function() {
      callback(null);
    };
  },
  get: function(requestURL, callback) {
    var self = this;
    if (!self.indexedDB)
      return callback(new Error("indexedDB is not available"));
    if (!self.db)
      return self.queue.push({ action: "get", arguments: arguments });
    var getDB = self.db
      .transaction([self.store], "readonly")
      .objectStore(self.store)
      .get(self.id(requestURL));
    getDB.onerror = function(e) {
      e.preventDefault();
      callback(new Error("failed to get request from indexedDB cache"));
    };
    getDB.onsuccess = function(e) {
      callback(null, e.target.result);
    };
  }
};
CachedXMLHttpRequest.cache.init();
CachedXMLHttpRequest.wrap = function(func) {
  return function() {
    var realXMLHttpRequest = XMLHttpRequest,
      result;
    XMLHttpRequest = CachedXMLHttpRequest;
    try {
      result = func.apply(this, arguments);
    } catch (e) {
      XMLHttpRequest = realXMLHttpRequest;
      throw e;
    }
    XMLHttpRequest = realXMLHttpRequest;
    return result;
  };
};
if (Module.CachedXMLHttpRequestDisable !== true) {
  if (Module.CachedXMLHttpRequestLoader === true) {
    if (typeof LoadCompressedFile == "function")
      LoadCompressedFile = CachedXMLHttpRequest.wrap(LoadCompressedFile);
    if (typeof DecompressAndLoadFile == "function")
      DecompressAndLoadFile = CachedXMLHttpRequest.wrap(DecompressAndLoadFile);
  }
  Object.defineProperty(Module, "asmLibraryArg", {
    get: function() {
      return Module.realAsmLibraryArg;
    },
    set: function(value) {
      if (
        typeof value == "object" &&
        typeof value._JS_WebRequest_Create == "function"
      )
        value._JS_WebRequest_Create = CachedXMLHttpRequest.wrap(
          value._JS_WebRequest_Create
        );
      Module.realAsmLibraryArg = value;
    }
  });
}
