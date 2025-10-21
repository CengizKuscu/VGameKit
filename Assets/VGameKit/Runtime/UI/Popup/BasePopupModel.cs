using System;

namespace VGameKit.Runtime.UI.Popup
{
    /// <summary>
    /// Base model for popups, parameterized by popup name enum.
    /// </summary>
    /// <typeparam name="TPopupName">Enum type representing popup names.</typeparam>
    public class BasePopupModel<TPopupName> where TPopupName : Enum
    {
        /// <summary>
        /// Event triggered when the popup is closed.
        /// </summary>
        public Action<TPopupName> OnClose;
    }
}