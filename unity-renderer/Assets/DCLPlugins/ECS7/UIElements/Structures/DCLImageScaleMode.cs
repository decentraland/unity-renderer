namespace DCL.UIElements.Structures
{
    public enum DCLImageScaleMode
    {
        // Traditional slicing
        NINE_SLICES = 0,

        // Does not scale, draws in a pixel-perfect model relative to the object center
        CENTER = 1,

        // Scales the texture, maintaining aspect ratio, so it completely fits withing the position rectangle passed to GUI.DrawTexture
        // Corresponds to Sprite's ScaleMode.ScaleToFit.
        // Applies custom UVs
        STRETCH = 2
    }
}
