using UnityEngine;

namespace VGameKit.Runtime.Utilities
{
    /// <summary>
    /// Utility class for getting, adding, or removing components on Unity GameObjects.
    /// </summary>
    public static class GetOrAddComponentUtility
    {
        /// <summary>
        /// Gets the component of type <typeparamref name="T"/> attached to the specified <see cref="GameObject"/>.
        /// If the component does not exist, it will be added and returned.
        /// </summary>
        /// <typeparam name="T">The type of component to get or add. Must inherit from <see cref="Component"/>.</typeparam>
        /// <param name="child">The <see cref="GameObject"/> to search for the component.</param>
        /// <returns>The existing or newly added component of type <typeparamref name="T"/>.</returns>
        /// <example>
        /// <code>
        /// BoxCollider boxCollider = gameObject.GetOrAddComponent&lt;BoxCollider&gt;();
        /// </code>
        /// </example>
        public static T GetOrAddComponent<T>(this GameObject child) where T : Component
        {
            T result = child.GetComponent<T>();
            if (result == null)
            {
                result = child.AddComponent<T>();
            }
            return result;
        }
    
        /// <summary>
        /// Removes the component of type <typeparamref name="T"/> from the specified <see cref="GameObject"/>, if it exists.
        /// </summary>
        /// <typeparam name="T">The type of component to remove. Must inherit from <see cref="Component"/>.</typeparam>
        /// <param name="gameObject">The <see cref="GameObject"/> from which to remove the component.</param>
        /// <example>
        /// <code>
        /// gameObject.RemoveComponent&lt;BoxCollider&gt;();
        /// </code>
        /// </example>
        public static void RemoveComponent<T>(this GameObject gameObject) where T : Component
        {
            T result = gameObject.GetComponent<T>();
            if(result != null)
            {
                Object.Destroy(result);
            }
        }
    }
}