var ClipboardWebGL = {
    initialize: function(csReadCallback, csPasteCallback, csCopyCallback){
        window.clipboardReadText = function (text, error){
            const bufferSize = lengthBytesUTF8(text) + 1
            const ptrText = _malloc(bufferSize)
            stringToUTF8(text, ptrText, bufferSize)
            const intError = error? 0 : 1
            Runtime.dynCall('vii', csReadCallback, [ptrText, intError])
        }

        window.addEventListener('paste', function(e) {
            const text = e.clipboardData.getData('text')
            const bufferSize = lengthBytesUTF8(text) + 1
            const ptrText = _malloc(bufferSize)
            stringToUTF8(text, ptrText, bufferSize)
            Runtime.dynCall('vi', csPasteCallback, [ptrText])
        })

        window.addEventListener('copy', function(e) {
            Runtime.dynCall('v', csCopyCallback)
        })
    },

    writeText: function (text){
        navigator.clipboard.writeText(Pointer_stringify(text))
    },

    readText: function (){
        // NOTE: firefox does not support clipboard.read
        if (navigator.clipboard.readText === undefined){
            window.clipboardReadText("not supported", true)
            return
        }

        // NOTE: workaround cause jslib don't support async functions
        eval("navigator.clipboard.readText()" +
            ".then(text => window.clipboardReadText(text, false))"+
            ".catch(e => window.clipboardReadText(e.message, true))")
    }
};
mergeInto(LibraryManager.library, ClipboardWebGL);
