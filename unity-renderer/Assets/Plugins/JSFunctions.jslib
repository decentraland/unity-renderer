/**
 * Interface?
 */

mergeInto(LibraryManager.library, {
  StartDecentraland: function() {
    window.DCL.JSEvents = JSEvents
    window.DCL.EngineStarted();

    // We expose the GL object to be able to generate WebGL textures in kernel and use them in Unity
    window.DCL.GL = GL;
    
    // Cache query string to be consulted later
    window.DCL.queryString = new URLSearchParams(window.location.search);
  },
  MessageFromEngine: function(type, message) {
    window.DCL.MessageFromEngine(Pointer_stringify(type), Pointer_stringify(message));
  },
  BinaryMessageFromEngine: function(sceneId, dataPtr, dataSize) {
    var bytes = HEAPU8.subarray(dataPtr, dataPtr + dataSize)
    window.DCL.BinaryMessageFromEngine(Pointer_stringify(sceneId), bytes);
  },
  GetGraphicCard: function() {
    const glcontext = GL.currentContext.GLctx;
    const debugInfo = glcontext.getExtension('WEBGL_debug_renderer_info');
    const graphicCard = glcontext.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL);

    // How to return strings: https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html
    var bufferSize = lengthBytesUTF8(graphicCard) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(graphicCard, buffer, bufferSize);
    return buffer;
  },
  ToggleFPSCap: function(useFPSCap) {
    window.capFPS = useFPSCap;
  },
  CheckURLParam: function(targetParam) {    
    return window.DCL.queryString.has(Pointer_stringify(targetParam))
  },
  GetURLParam: function(targetParam) {
    const stringifiedTargetParam = Pointer_stringify(targetParam)
    
    if(window.DCL.queryString.has(stringifiedTargetParam)) {
        const urlParamValue = window.DCL.queryString.get(stringifiedTargetParam);
      
        // How to return strings: https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html
        var bufferSize = lengthBytesUTF8(urlParamValue) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(urlParamValue, buffer, bufferSize);
        return buffer;
    }
    
    return '';
  }
});

