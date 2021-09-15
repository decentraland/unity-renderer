var WebInfo = {
    GetUserAgent: function () {
        return navigator.userAgent;
    },
};
autoAddDeps(WebInfo, "$info");
mergeInto(LibraryManager.library, WebInfo);
