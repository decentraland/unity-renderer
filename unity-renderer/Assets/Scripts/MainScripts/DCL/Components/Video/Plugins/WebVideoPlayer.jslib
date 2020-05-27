var WebVideoPlayer = {
  $videos: {},

  WebVideoPlayerCreate: function (videoId, url, useHls) {
    const videoState = {
      NONE: 0,
      ERROR: 1,
      LOADING: 2,
      READY: 3,
      PLAYING: 4,
      BUFFERING: 5,
      SEEKING: 6,
      PAUSED: 7,
    };

    const videoUrl = Pointer_stringify(url);
    const vid = document.createElement("video");
    vid.autoplay = false;

    var videoData = {
      video: vid,
      state: videoState.NONE,
      error: "",
    };

    if (useHls) {
      const hls = new Hls();
      hls.loadSource(videoUrl);
      hls.attachMedia(vid);
      hls.on(Hls.Events.ERROR, function () {
        videoData.state = videoState.ERROR;
        videoData.error = "Hls error";
      });
      videoData["hlsInstance"] = hls;
    } else {
      vid.src = videoUrl;
    }

    vid.oncanplay = function () {
      videoData.state = videoState.READY;
    };

    vid.loadstart = function () {
      videoData.state = videoState.LOADING;
    };

    vid.onplaying = function () {
      videoData.state = videoState.PLAYING;
    };

    vid.onpause = function () {
      videoData.state = videoState.PAUSED;
    };

    vid.onwaiting = function () {
      videoData.state = videoState.BUFFERING;
    };

    vid.onseeking = function () {
      videoData.state = videoState.SEEKING;
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
    videos[id].video.src = "";
    videos[id].video.load();
    videos[id].video = null;
    if (videos[id].hlsInstance !== undefined) {
      delete videos[id].hlsInstance;
    }
    delete videos[id];
  },

  WebVideoPlayerTextureUpdate: function (videoId, texturePtr, isWebGL1) {
    const id = Pointer_stringify(videoId);

    if (videos[id].state !== 4) return; //PLAYING

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
      const videoData = videos[Pointer_stringify(videoId)];
      videoData.video.play();
    } catch (err) {
      // Exception!
    }
  },

  WebVideoPlayerPause: function (videoId) {
    const videoData = videos[Pointer_stringify(videoId)];
    videoData.video.pause();
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

  WebVideoPlayerSetTime: function (videoId, second) {
    const videoData = videos[Pointer_stringify(videoId)];
    const vid = videoData.video;

    if (second == 0) {
      const playbackRate = vid.playbackRate;
      vid.pause();
      vid.load();
      vid.play();
    } else if (vid.seekable && vid.seekable.length > 0) {
      vid.currentTime = second;
    }
  },

  WebVideoPlayerSetLoop: function (videoId, value) {
    videos[Pointer_stringify(videoId)].video.loop = value;
  },

  WebVideoPlayerSetPlaybackRate: function (videoId, value) {
    videos[Pointer_stringify(videoId)].video.playbackRate = value;
  },
};
autoAddDeps(WebVideoPlayer, "$videos");
mergeInto(LibraryManager.library, WebVideoPlayer);
