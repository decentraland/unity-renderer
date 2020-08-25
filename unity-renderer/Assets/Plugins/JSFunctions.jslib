/**
 * Interface?
 */

mergeInto(LibraryManager.library, {
  StartDecentraland: function() {
    window.DCL.JSEvents = JSEvents
    window.DCL.EngineStarted();

    // We expose the GL object to be able to generate WebGL textures in kernel and use them in Unity
    window.DCL.GL = GL;
  },
  MessageFromEngine: function(type, message) {
    window.DCL.MessageFromEngine(Pointer_stringify(type), Pointer_stringify(message));
  }
});

