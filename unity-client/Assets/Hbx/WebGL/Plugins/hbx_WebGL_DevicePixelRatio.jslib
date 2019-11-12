//----------------------------------------------
//            Hbx: WebGL
// Copyright © 2017-2018 Hogbox Studios
//----------------------------------------------

mergeInto(LibraryManager.library, {

  hbx_WebGL_GetWindowDevicePixelRatio: function () {
    return window.devicePixelRatio || 1.0;
  },

  hbx_WebGL_GetDevicePixelRatio: function () {
    return window.hbxDpr || 1.0;
  },

  hbx_WebGL_SetDevicePixelRatio: function (dpr) {
    window.hbxDpr = dpr;
  },

  hbx_WebGL_ScaleDevicePixelRatio: function (scale) {
    window.hbxDpr = (window.devicePixelRatio || 1.0) * scale;
  }

});
