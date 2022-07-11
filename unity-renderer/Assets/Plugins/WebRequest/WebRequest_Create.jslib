var LibraryWebRequestWebGL = {
    $wr: {
        requestInstances: {},
        nextRequestId: 1
    },

    JS_WebRequest_Create__proxy: 'sync',
    JS_WebRequest_Create__sig: 'iii',
    JS_WebRequest_Create: function (url, method) {
        var http = new XMLHttpRequest();
        var _url = UTF8ToString(url);
        var _method = UTF8ToString(method);
        http.open(_method, _url, true);
        http.responseType = 'arraybuffer';
        wr.requestInstances[wr.nextRequestId] = http;
        return wr.nextRequestId++;
    },
};

mergeInto(LibraryManager.library, LibraryWebRequestWebGL);

