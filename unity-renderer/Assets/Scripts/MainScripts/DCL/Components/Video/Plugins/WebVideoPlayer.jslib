var WebVideoPlayer = {
  $videos: {},
  $hls: null,

  WebVideoPlayerCreate: function (videoId, url) {
    const videoState = {
      NONE: 0,
      ERROR: 1,
      LOADING: 2,
      READY: 3,
      PLAYING: 4,
      BUFFERING: 5,
    };

    var str = Pointer_stringify(url);
    const vid = document.createElement("video");
    vid.autoplay = false;

    var videoData = {
      video: vid,
      state: videoState.NONE,
      error: "",
    };

    hls = new Hls();
    hls.loadSource(str);
    hls.attachMedia(vid);

    vid.oncanplay = function () {
      videoData.state = videoState.READY;
      console.log("video: READY");
    };

    vid.loadstart = function () {
      videoData.state = videoState.LOADING;
      console.log("video: LOADING");
    };

    vid.onplaying = function () {
      videoData.state = videoState.PLAYING;
      console.log("video: PLAYING");
    };

    vid.onwaiting = function () {
      videoData.state = videoState.BUFFERING;
      console.log("video: BUFFERING");
    };

    vid.onerror = function () {
      videoData.state = videoState.ERROR;
      videoData.error = vid.error.message;
      console.log("video: ERROR " + videoData.error);
    };

    vid.crossOrigin = "anonymous";
    videos[Pointer_stringify(videoId)] = videoData;
  },

  WebVideoPlayerRemove: function (videoId) {
    const id = Pointer_stringify(videoId);
    videos[id].video.srt = "";
    videos[id].video.load();
    videos[id].video = null;
    delete videos[id];
    delete hls;
  },

  WebVideoPlayerTextureUpdate: function (videoId, texturePtr, isWebGL1) {
    const id = Pointer_stringify(videoId);

    if (videos[id].paused) return;

    GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[texturePtr]);
    if (isWebGL1) {
      GLctx.texImage2D(
        GLctx.TEXTURE_2D,
        0,
        GLctx.RGBA,
        GLctx.RGBA,
        GLctx.UNSIGNED_BYTE,
        videos[id].video
      );
    } else {
      GLctx.texSubImage2D(
        GLctx.TEXTURE_2D,
        0,
        0,
        0,
        GLctx.RGBA,
        GLctx.UNSIGNED_BYTE,
        videos[id].video
      );
    }
  },

  WebVideoPlayerPlay: function (videoId) {
    try {
      videos[Pointer_stringify(videoId)].video.play();
    } catch (err) {
      // Exception!
    }
  },

  WebVideoPlayerPause: function (videoId) {
    videos[Pointer_stringify(videoId)].video.pause();
  },

  WebVideoPlayerVolume: function (videoId, volume) {
    videos[Pointer_stringify(videoId)].video.volume = volume;
  },

  WebVideoPlayerGetHeight: function (videoId) {
    return videos[Pointer_stringify(videoId)].video.videoHeight;
  },

  WebVideoPlayerGetWidth: function (videoId) {
    return videos[Pointer_stringify(videoId)].video.videoWidth;
  },

  WebVideoPlayerGetTime: function (videoId) {
    return videos[Pointer_stringify(videoId)].video.currentTime;
  },

  WebVideoPlayerGetDuration: function (videoId) {
    return videos[Pointer_stringify(videoId)].video.duration;
  },

  WebVideoPlayerGetState: function (videoId) {
    const id = Pointer_stringify(videoId);
    if (!videos.hasOwnProperty(id)) {
      return 1; //videoState.ERROR;
    }
    return videos[id].state;
  },

  WebVideoPlayerGetError: function (videoId) {
    const id = Pointer_stringify(videoId);
    var errorStr = "";
    if (!videos.hasOwnProperty(id)) {
      errorStr = "Video id " + id + " does not exist";
    } else {
      errorStr = videos[id].error;
    }

    var bufferSize = lengthBytesUTF8(errorStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(errorStr, buffer, bufferSize);
    return buffer;
  },
};
autoAddDeps(WebVideoPlayer, "$hls");
autoAddDeps(WebVideoPlayer, "$videos");
mergeInto(LibraryManager.library, WebVideoPlayer);
