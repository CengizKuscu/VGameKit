using UnityEngine;

namespace VGameKit.Runtime.Utilities
{
    /// <summary>
    /// Provides extension methods for the <see cref="Animator"/> class.
    /// </summary>
    public static class AnimatorExtensions
    {
        /// <summary>
        /// Resets all trigger parameters on the specified <see cref="Animator"/>.
        /// </summary>
        /// <param name="animator">The animator whose triggers will be reset.</param>
        public static void ResetAllTriggers(this Animator animator)
        {
            foreach (var trigger in animator.parameters)
            {
                if (trigger.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.ResetTrigger(trigger.name);
                }
            }
        }
    }
}