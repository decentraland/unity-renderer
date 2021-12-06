using System;

namespace DCL.Builder.Manifest
{
    public interface IManifest
    {
        /// <summary>
        /// Version of the manifest
        /// </summary>
        int version { get; set; }

        /// <summary>
        /// Data of the project
        /// </summary>
        ProjectData project { get; set; }

        /// <summary>
        /// ParcelScene converted to a scheme that builder server can understand
        /// </summary>
        WebBuilderScene scene { get; set; }
    }

    [Serializable]
    public class Manifest : IManifest
    {
        public int version { get; set; }
        public ProjectData project { get; set; }
        public WebBuilderScene scene { get; set; }
    }
}