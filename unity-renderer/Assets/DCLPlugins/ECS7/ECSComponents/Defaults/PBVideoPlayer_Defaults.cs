using System.Collections.Generic;

namespace DCL.ECSComponents
{
    public static class PBVideoPlayer_Defaults
    {
        public static float GetVolume(this PBVideoPlayer self)
        {
            return self.HasVolume ? self.Volume : 1.0f;
        }

        public static bool IsPlaying(this PBVideoPlayer self)
        {
            return self.HasPlaying && self.Playing; // default: playing=false
        }

        public static float GetPlaybackRate(this PBVideoPlayer self)
        {
            return self.HasPlaybackRate ? self.PlaybackRate : 1.0f;
        }

        public static float GetPosition(this PBVideoPlayer self)
        {
            return self.HasPosition ? self.Position : 0.0f;
        }

        public static bool GetLoop(this PBVideoPlayer self)
        {
            return self.HasLoop && self.Loop; // default: loop=false
        }

        public static string GetVideoUrl(this PBVideoPlayer self, ContentProvider sceneContentProvider,
            IReadOnlyList<string> sceneRequiredPermissions, IReadOnlyList<string> sceneAllowedMediaHostnames)
        {
            UtilsScene.TryGetMediaUrl(self.Src, sceneContentProvider,
                sceneRequiredPermissions, sceneAllowedMediaHostnames, out string url);

            return url;
        }
    }
}
