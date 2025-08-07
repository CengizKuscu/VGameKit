using System;
using UnityEngine;

namespace VGameKit.Runtime.Utilities.Camera
{
    /// <summary>
    /// Represents padding values for each side (top, bottom, left, right).
    /// </summary>
    [Serializable]
    public struct Padding
    {
        /// <summary>
        /// Padding at the top.
        /// </summary>
        [field: SerializeField] public float Top { get; set; }
    
        /// <summary>
        /// Padding at the bottom.
        /// </summary>
        [field: SerializeField] public float Bottom { get; set; }
    
        /// <summary>
        /// Padding on the left.
        /// </summary>
        [field: SerializeField] public float Left { get; set; }
    
        /// <summary>
        /// Padding on the right.
        /// </summary>
        [field: SerializeField] public float Right { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Padding"/> struct with the specified values.
        /// </summary>
        /// <param name="top">Padding at the top.</param>
        /// <param name="bottom">Padding at the bottom.</param>
        /// <param name="left">Padding on the left.</param>
        /// <param name="right">Padding on the right.</param>
        public Padding(float top, float bottom, float left, float right)
        {
            Top = top;
            Bottom = bottom;
            Left = left;
            Right = right;
        }
    }
}