using System;
using System.Globalization;
using System.Threading;

namespace DiscordBot
{/// <summary>Extension class which provides a more stable (and random) random number generator.</summary>
    internal static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random Local;

        /// <summary>
        /// Returns a Random based on this thread.
        /// </summary>
        internal static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }

    /// <summary>Extension class to more easily parse Integers.</summary>
    internal static class Int32Helper
    {
        /// <summary>
        /// Utilizes int.TryParse to easily Parse an Integer.
        /// </summary>
        /// <param name="text">Text to be parsed</param>
        /// <returns></returns>
        internal static int Parse(string text)
        {
            int temp = 0;
            int.TryParse(text, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out temp);
            return temp;
        }
    }
}