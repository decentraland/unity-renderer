CachedXMLHttpRequest implements automatic caching of WWW or WebRequest responses in the indexedDB in Unity WebGL. It has been designed for caching downloaded asset bundles and supports response caching for GET requests without custom headers or request body. CachedXMLHttpRequest can be used as a more memory efficient solution for caching asset bundles in comparison with WWW.LoadFromCacheOrDownload.

The difference between caching asset bundles with LoadFromCacheOrDownload and CachedXMLHttpRequest is the following:

All the files previously cached with LoadFromCacheOrDownload are loaded from the indexedDB into the memory file system on application startup. This might include cached files that will be used at some point in the future or might not be used at all, therefore wasting the main memory. LoadFromCacheOrDownload requires a version number provided by the application.

When downloading asset bundles with WWW or WebRequest while CachedXMLHttpRequest is enabled, only the currently requested file will be copied from the indexedDB into the main memory (or downloaded), after the file content is transferred to the application, this memory will get garbage collected, therefore no main memory is wasted. CachedXMLHttpRequest file versioning is based on the Last-Modified and ETag headers, provided by the server.

In addition, starting from Unity 5.4, CachedXMLHttpRequest can be used to cache the initially loaded .js, .data and .mem files in the indexedDB (it may serve as replacement for the "Data caching" build option which caches all the files in the build).


CachedXMLHttpRequest can be configured using the following Module variables:

  Set Module.CachedXMLHttpRequestDisable to true in order to fully disable indexedDB caching.
  Set Module.CachedXMLHttpRequestSilent to true in order to disable cache logs in the console.
  Set Module.CachedXMLHttpRequestLoader to true in order to enable caching of the initially loaded .js, .data and .mem files (applies to Unity 5.4 and above)
  Set Module.CachedXMLHttpRequestBlacklist to an array of RegExp or string objects to disable caching for matching URLs (useful to prevent caching of API endpoints, etc)
  Set Module.CachedXMLHttpRequestRevalidateBlacklist to an array of RegExp or string objects to disable re-validation for matching URLs (helpful if you use explicit versioning on your asset bundles)

  Module variables can be initialized directly in the index.html in the following way (note that initialization is optional):

    var Module = {
      TOTAL_MEMORY: 268435456,
      CachedXMLHttpRequestLoader: true,
      CachedXMLHttpRequestCacheBlacklist: [/\.xml/, /\.php/, 'xhr_nocache'],
      CachedXMLHttpRequestRevalidateBlacklist: [/\.unity3d/]
      ...
    };
