mergeInto(LibraryManager.library, {

  ConsoleLog: function (message) {
    console.log(UTF8ToString(message));
    console.log(message);
  },

});