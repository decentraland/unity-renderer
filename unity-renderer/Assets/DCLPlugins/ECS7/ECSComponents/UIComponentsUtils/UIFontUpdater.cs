using UnityEngine.UIElements;

namespace DCL.ECSComponents.Utils
{
    public struct UIFontUpdater
    {
        private Font? lastFont;
        private AssetPromise_Font lastPromise;
        private readonly VisualElement targetElement;
        private readonly AssetPromiseKeeper_Font fontPromiseKeeper;

        public UIFontUpdater(VisualElement targetElement, AssetPromiseKeeper_Font fontPromiseKeeper)
        {
            this.targetElement = targetElement;
            this.fontPromiseKeeper = fontPromiseKeeper;
            lastFont = null;
            lastPromise = null;
        }

        public void Update(Font newFont)
        {
            if (lastFont == newFont)
                return;

            lastFont = newFont;
            var prevPromise = lastPromise;

            lastPromise = new AssetPromise_Font(newFont.ToFontName());
            lastPromise.OnSuccessEvent += ChangeFont;
            fontPromiseKeeper.Keep(lastPromise);
            fontPromiseKeeper.Forget(prevPromise);
        }

        private void ChangeFont(Asset_Font font)
        {
            targetElement.style.unityFont = font.font.sourceFontFile;
        }

        public void Dispose()
        {
            fontPromiseKeeper.Forget(lastPromise);
        }
    }
}
