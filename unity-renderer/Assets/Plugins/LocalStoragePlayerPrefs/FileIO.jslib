var FileIO = {

  SaveStringToLocalStorage : function(key, data) {
    localStorage.setItem(UTF8ToString(key), UTF8ToString(data));
  },

  LoadStringFromLocalStorage : function(key) {
    var returnStr = localStorage.getItem(UTF8ToString(key));
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },

  RemoveFromLocalStorage : function(key) {
    localStorage.removeItem(UTF8ToString(key));
  },

  HasKeyInLocalStorage : function(key) {
    if (localStorage.getItem(UTF8ToString(key))) {
      return 1;
    }
    else {
      return 0;
    }
  }
};

mergeInto(LibraryManager.library, FileIO);;
