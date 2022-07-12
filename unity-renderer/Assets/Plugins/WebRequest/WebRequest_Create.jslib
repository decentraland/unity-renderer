var LibraryWebRequestWebGL = {
    $wr: {
        requests: {},
        responses: {},
        abortControllers: {},
        timer: {},
        nextRequestId: 1
    },

    JS_WebRequest_Create__proxy: 'sync',
    JS_WebRequest_Create__sig: 'iii',
    JS_WebRequest_Create: function (url, method) {
        var http = new XMLHttpRequest();
        var _url = UTF8ToString(url);
        var _method = UTF8ToString(method);
        var abortController = new AbortController;

        http.open(_method, _url, true);
        http.responseType = 'arraybuffer';
        http.url = _url;
        http.init = { method: _method, signal: abortController.signal, headers: {} };

        wr.abortControllers[wr.nextRequestId] = abortController;
        wr.requests[wr.nextRequestId] = http;
        return wr.nextRequestId++;
    },
};

mergeInto(LibraryManager.library, LibraryWebRequestWebGL);

