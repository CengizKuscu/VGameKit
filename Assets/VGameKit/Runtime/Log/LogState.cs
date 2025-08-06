using System;

namespace VGameKit.Runtime.Log
{
    [Flags]
    public enum LogState
    {
        None = 0,

        #region Info

        Core = 1 << 0,
        Development = 1 << 1,
        Info = 1 << 2,

        #endregion

        #region Plugins

        Analytics = 1 << 3,
        IAP = 1 << 4,
        Ads = 1 << 5,

        #endregion

        #region Warning

        Warning = 1 << 6,
        Game = 1 << 7,
        Pause = 1 << 8,
        ProcessFlow = 1 << 9,

        #endregion

        #region Error

        Error = 1 << 10,
        Fatal = 1 << 11,

        #endregion

        Booster = 1 << 12,
        Timer = 1 << 13,
    }
}