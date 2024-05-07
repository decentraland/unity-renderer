using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public struct AvatarSettings
    {
        /// <summary>
        /// Name of the player controlling this avatar (if any)
        /// </summary>
        public string playerName;

        /// <summary>
        /// Bodyshape ID of the avatar
        /// </summary>
        public string bodyshapeId;

        /// <summary>
        /// Hair color of the avatar
        /// </summary>
        public Color hairColor;

        /// <summary>
        /// Skin color of the avatar
        /// </summary>
        public Color skinColor;

        /// <summary>
        /// Eyes color of the avatar
        /// </summary>
        public Color eyesColor;

        /// <summary>
        /// Hiding override
        /// </summary>
        public HashSet<string> forceRender;
    }

}
