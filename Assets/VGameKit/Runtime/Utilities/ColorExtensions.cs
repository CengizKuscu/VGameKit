using UnityEngine;
using UnityEngine.UI;

namespace VGameKit.Runtime.Utilities
{
    /// <summary>
    /// Provides extension methods for setting the alpha value of UnityEngine.UI.Graphic and UnityEngine.Material objects.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Sets the alpha value of a UI Graphic component.
        /// </summary>
        /// <param name="graphic">The Graphic component whose alpha will be set.</param>
        /// <param name="alpha">The new alpha value (0.0 to 1.0).</param>
        public static void SetAlpha(this Graphic graphic, float alpha)
        {
            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
    
        /// <summary>
        /// Sets the alpha value of a Material.
        /// </summary>
        /// <param name="material">The Material whose alpha will be set.</param>
        /// <param name="alpha">The new alpha value (0.0 to 1.0).</param>
        public static void SetAlpha(this Material material, float alpha)
        {
            Color color = material.color;
            color.a = alpha;
            material.color = color;
        }
    }
}