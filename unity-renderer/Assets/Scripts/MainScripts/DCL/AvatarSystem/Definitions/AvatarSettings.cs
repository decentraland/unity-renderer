using UnityEngine;

namespace AvatarSystem
{
    public struct AvatarSettings
    {
        /// <summary>
        /// Bodyshape ID of the avatar
        /// </summary>
        public string bodyshapeId;

        /// <summary>
        /// Visibility of bodyshape's head
        /// </summary>
        public bool headVisible;

        /// <summary>
        /// Visibility of bodyshape's upperbody
        /// </summary>
        public bool upperbodyVisible;

        /// <summary>
        /// visibility of bodyshape's lowerbody
        /// </summary>
        public bool lowerbodyVisible;

        /// <summary>
        /// Visibility of bodyshape's feet
        /// </summary>
        public bool feetVisible;

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
    }
}