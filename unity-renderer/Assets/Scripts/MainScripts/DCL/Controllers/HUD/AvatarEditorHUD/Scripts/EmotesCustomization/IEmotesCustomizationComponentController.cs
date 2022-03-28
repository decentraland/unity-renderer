using System;

namespace DCL.EmotesCustomization
{
    public interface IEmotesCustomizationComponentController : IDisposable
    {
        /// <summary>
        /// Initializes the emotes customization controller.
        /// </summary>
        /// <param name="userProfile">User profile.</param>
        /// <param name="catalog">Wearables catalog.</param>
        void Initialize(UserProfile userProfile, BaseDictionary<string, WearableItem> catalog);
    }
}