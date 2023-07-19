/*
* FileSaver.js
* A saveAs() FileSaver implementation.
*
* By Eli Grey, http://eligrey.com
* 
* License : https://github.com/eligrey/FileSaver.js/blob/master/LICENSE.md (MIT)
* source  : http://purl.eligrey.com/github/FileSaver.js
*/

/* 
* Edited by Nat for Unity Support. https://twitter.com/nateonus
* License : Still MIT
* Source  : Somewhere yet to be uploaded
*/

mergeInto(LibraryManager.library, {
    _global: null,
    init__deps: [
        '_global',
        'saveAs',
        'corsEnabled',
        'download',
        'click',
        'bom'
    ],
    init: function () {
        __global = typeof window === 'object' && window.window === window ? window : typeof self === 'object' && self.self === self ? self : typeof global === 'object' && global.global === global ? global : this;
        _saveAs = __global.saveAs || (typeof window !== 'object' || window !== __global ? function saveAs() {
        } : 'download' in HTMLAnchorElement.prototype ? function saveAs(blob, name, opts) {
            var URL = __global.URL || __global.webkitURL;
            var a = document.createElement('a');
            name = name || blob.name || 'download';
            a.download = name;
            a.rel = 'noopener';
            if (typeof blob === 'string') {
                a.href = blob;
                if (a.origin !== location.origin) {
                    _corsEnabled(a.href) ? _download(blob, name, opts) : _click(a, a.target = '_blank');
                } else {
                    _click(a);
                }
            } else {
                a.href = URL.createObjectURL(blob);
                setTimeout(function () {
                    URL.revokeObjectURL(a.href);
                }, 40000);
                setTimeout(function () {
                    _click(a);
                }, 0);
            }
        } : 'msSaveOrOpenBlob' in navigator ? function saveAs(blob, name, opts) {
            name = name || blob.name || 'download';
            if (typeof blob === 'string') {
                if (_corsEnabled(blob)) {
                    _download(blob, name, opts);
                } else {
                    var a = document.createElement('a');
                    a.href = blob;
                    a.target = '_blank';
                    setTimeout(function () {
                        _click(a);
                    });
                }
            } else {
                navigator.msSaveOrOpenBlob(_bom(blob, opts), name);
            }
        } : function saveAs(blob, name, opts, popup) {
            popup = popup || open('', '_blank');
            if (popup) {
                popup.document.title = popup.document.body.innerText = 'downloading...';
            }
            if (typeof blob === 'string')
                return _download(blob, name, opts);
            var force = blob.type === 'application/octet-stream';
            var isSafari = /constructor/i.test(__global.HTMLElement) || __global.safari;
            var isChromeIOS = /CriOS\/[\d]+/.test(navigator.userAgent);
            if ((isChromeIOS || force && isSafari) && typeof FileReader === 'object') {
                var reader = new FileReader();
                reader.onloadend = function () {
                    var url = reader.result;
                    url = isChromeIOS ? url : url.replace(/^data:[^;]*;/, 'data:attachment/file;');
                    if (popup)
                        popup.location.href = url;
                    else
                        location = url;
                    popup = null;
                };
                reader.readAsDataURL(blob);
            } else {
                var URL = __global.URL || __global.webkitURL;
                var url = URL.createObjectURL(blob);
                if (popup)
                    popup.location = url;
                else
                    location.href = url;
                popup = null;
                setTimeout(function () {
                    URL.revokeObjectURL(url);
                }, 40000);
            }
        });
        __global.saveAs = _saveAs.saveAs = _saveAs;
        if (typeof module !== 'undefined') {
            module.exports = _saveAs;
        }
        ;
    },
    bom: function (blob, opts) {
        if (typeof opts === 'undefined')
            opts = { autoBom: false };
        else if (typeof opts !== 'object') {
            console.warn('Deprecated: Expected third argument to be a object');
            opts = { autoBom: !opts };
        }
        if (opts.autoBom && /^\s*(?:text\/\S*|application\/xml|\S*\/\S*\+xml)\s*;.*charset\s*=\s*utf-8/i.test(blob.type)) {
            return new Blob([
                String.fromCharCode(65279),
                blob
            ], { type: blob.type });
        }
        return blob;
    },
    download__deps: ['saveAs'],
    download: function (url, name, opts) {
        var xhr = new XMLHttpRequest();
        xhr.open('GET', url);
        xhr.responseType = 'blob';
        xhr.onload = function () {
            _saveAs(xhr.response, name, opts);
        };
        xhr.onerror = function () {
            console.error('could not download file');
        };
        xhr.send();
    },
    corsEnabled: function (url) {
        var xhr = new XMLHttpRequest();
        xhr.open('HEAD', url, false);
        try {
            xhr.send();
        } catch (e) {
        }
        return xhr.status >= 200 && xhr.status <= 299;
    },
    click: function (node) {
        try {
            node.dispatchEvent(new MouseEvent('click'));
        } catch (e) {
            var evt = document.createEvent('MouseEvents');
            evt.initMouseEvent('click', true, true, window, 0, 0, 0, 80, 20, false, false, false, false, 0, null);
            node.dispatchEvent(evt);
        }
    },	
	UNITY_SAVE: function (content, name, mimetype)
	{
        var blob = new Blob([Pointer_stringify(content)], { type: Pointer_stringify(mimetype) });
		saveAs(blob, Pointer_stringify(name));
	},
    UNITY_SAVE_BYTEARRAY: function (arr, size, name, mimetype)
    {
        var bytes = new Uint8Array(size);
        for (var i = 0; i < size; i++) bytes[i] = HEAPU8[arr + i];
        var blob = new Blob([bytes], { type: Pointer_stringify(mimetype) });
        saveAs(blob, Pointer_stringify(name));
    },
	UNITY_IS_SUPPORTED: function ()
	{
		try 
		{
			var isFileSaverSupported = !!new Blob;
			return isFileSaverSupported;
		} 
		catch (e) {return false;}
		return false;
	},
    saveAs: null
});
