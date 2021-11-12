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

        var textureObject = GLctx.createTexture();
        const texId = GL.getNewId(textureObject);
        textureObject.name = texId
        GL.textures[texId] = textureObject

        var videoData = {
            video: vid,
            state: videoState.NONE,
            error: "",
            textureId: texId
        };

        videos[Pointer_stringify(videoId)] = videoData;

        if (useHls) {
            var hlsConfig = {
                maxBufferLength: 60,
            };
            const hls = new Hls(hlsConfig);
            hls.autoLevelCapping = 3; // 720p hard cap for performance
            hls.on(Hls.Events.MEDIA_ATTACHED, function () {
                hls.loadSource(videoUrl);
            });
            hls.on(Hls.Events.MEDIA_DETACHED, function () {
                hls.stopLoad();
            });
            hls.on(Hls.Events.ERROR, function (event, data) {
                if (data.fatal) {
                    switch (data.type) {
                        case Hls.ErrorTypes.NETWORK_ERROR:
                            console.log("VIDEO PLAYER: fatal network error encountered, try to recover");
                            hls.startLoad();
                            break;
                        case Hls.ErrorTypes.MEDIA_ERROR:
                            console.log("VIDEO PLAYER: fatal media error encountered, try to recover");
                            hls.recoverMediaError();
                            break;
                        default:
                            videoData.state = videoState.ERROR;
                            videoData.error = "Hls error";
                            hls.destroy();
                            break;
                    }
                }
            });
            videoData.state = videoState.READY;

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
    },

    WebVideoPlayerRemove: function (videoId) {
        const id = Pointer_stringify(videoId);
        if (!videos.hasOwnProperty(id)) {
            console.warn("video: trying to remove undefined video of id " + id);
            return;
        }
        videos[id].video.src = "";
        videos[id].video.load();
        videos[id].video = null;

        if (videos[id].hlsInstance !== undefined) {
            videos[id].hlsInstance.destroy();
            delete videos[id].hlsInstance;
        }

        const textureId = videos[id].textureId;
        var texture = GL.textures[textureId];
        texture.name = 0;
        GLctx.deleteTexture(texture);
        GL.textures[textureId] = null;
        delete videos[id];
    },

    WebVideoPlayerTextureGet: function (videoId) {
        const id = Pointer_stringify(videoId);
        return videos[id].textureId;
    },

    WebVideoPlayerTextureUpdate: function (videoId) {
        const id = Pointer_stringify(videoId);

        if (videos[id].state !== 4) return; //PLAYING

        const textureId = videos[id].textureId;

        GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[textureId]);

        GLctx.texImage2D(
            GLctx.TEXTURE_2D,
            0,
            GLctx.SRGB8_ALPHA8,
            videos[id].video.videoWidth,
            videos[id].video.videoHeight,
            0,
            GLctx.RGBA,
            GLctx.UNSIGNED_BYTE,
            videos[id].video
        );
    },

    WebVideoPlayerPlay: function (videoId, startTime) {
        try {
            const videoData = videos[Pointer_stringify(videoId)];
            if (videoData.hlsInstance !== undefined) {
                videoData.hlsInstance.attachMedia(videoData.video);
            }

            const playPromise = videoData.video.play();
            if (playPromise !== undefined) {
                playPromise.then(function () {
                    // Playback starts with no problem
                    if (startTime !== -1) {
                        videoData.video.currentTime = startTime
                    }
                })
                    .catch(function (error) {
                        // Playback cancelled before the video finished loading (e.g. when teleporting)
                        // we mustn't report this error as it's harmless and affects our metrics
                    });
            }
        } catch (err) {
            // Exception!
        }
    },

    WebVideoPlayerPause: function (videoId) {
        const videoData = videos[Pointer_stringify(videoId)];
        if (videoData.hlsInstance !== undefined) {
            videoData.hlsInstance.detachMedia(videoData.video);
        }
        videoData.video.pause();
        videoData.state = 7; //PAUSED
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

        if (second === 0) {
            vid.currentTime = 0;
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
