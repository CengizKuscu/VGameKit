using System;
using UnityEngine;

namespace VGameKit.Runtime.Utilities
{
    /// <summary>
    /// Extension methods for converting between <see cref="SerializableVector3"/> and <see cref="Vector3"/>.
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Converts a <see cref="SerializableVector3"/> to a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="serializableVector3">The serializable vector to convert.</param>
        /// <returns>A <see cref="Vector3"/> with the same x, y, and z values.</returns>
        public static Vector3 ToVector3(this SerializableVector3 serializableVector3)
        {
            return new Vector3(serializableVector3.x, serializableVector3.y, serializableVector3.z);
        }
    
        /// <summary>
        /// Converts a <see cref="Vector3"/> to a <see cref="SerializableVector3"/>.
        /// </summary>
        /// <param name="vector3">The vector to convert.</param>
        /// <returns>A <see cref="SerializableVector3"/> with the same x, y, and z values.</returns>
        public static SerializableVector3 FromVector3(this Vector3 vector3)
        {
            return new SerializableVector3(vector3);
        }
    }
    
    /// <summary>
    /// Serializable representation of a <see cref="Vector3"/>.
    /// </summary>
    [Serializable]
    public class SerializableVector3
    {
        /// <summary>
        /// The x component of the vector.
        /// </summary>
        public float x;
        /// <summary>
        /// The y component of the vector.
        /// </summary>
        public float y;
        /// <summary>
        /// The z component of the vector.
        /// </summary>
        public float z;
    
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableVector3"/> class with the given components.
        /// </summary>
        /// <param name="x">The x component.</param>
        /// <param name="y">The y component.</param>
        /// <param name="z">The z component.</param>
        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableVector3"/> class from a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="vector3">The vector to copy values from.</param>
        public SerializableVector3(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }
    }
}