var WebInfo = {
    GetUserAgent: function () {
        var ua = navigator.userAgent;
        var bufferSize = lengthBytesUTF8(ua) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(ua, buffer, bufferSize);
        return buffer;
    },
};

mergeInto(LibraryManager.library, WebInfo);
