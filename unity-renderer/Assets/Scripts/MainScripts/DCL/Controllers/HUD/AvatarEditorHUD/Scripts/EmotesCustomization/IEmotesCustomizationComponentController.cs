using System;

namespace DCL.EmotesCustomization
{
    public interface IEmotesCustomizationComponentController : IDisposable
    {
        /// <summary>
        /// It will be triggered when an emote should be previewed.
        /// </summary>
        public event Action<string> onEmotePreviewed;

        /// <summary>
        /// It will be triggered when an emote is equipped.
        /// </summary>
        public event Action<string> onEmoteEquipped;

        /// <summary>
        /// It will be triggered when an emote is unequipped.
        /// </summary>
        public event Action<string> onEmoteUnequipped;

        /// <summary>
        /// It will be triggered when the sell button is clicked for an emote.
        /// </summary>
        public event Action<string> onEmoteSell;

        /// <summary>
        /// Initializes the emotes customization controller.
        /// </summary>
        /// <param name="userProfile">User profile.</param>
        /// <param name="catalog">Wearables catalog.</param>
        IEmotesCustomizationComponentView Initialize(UserProfile userProfile, BaseDictionary<string, WearableItem> catalog);

        /// <summary>
        /// Restore the emote slots with the stored data.
        /// </summary>
        void RestoreEmoteSlots();
    }
}