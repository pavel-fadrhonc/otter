namespace LostPolygon.SpriteSharp.Processing {
    /// <summary>
    /// Sprite processing method.
    /// </summary>
    public enum SpriteProcessingMethod {
#if !SS_ADVANCED_METHODS_DISABLED
        Normal = 0,
        Precise = 1,
        AlphaSeparation = 2,
#endif
        RectGrid = 3,
    }
}