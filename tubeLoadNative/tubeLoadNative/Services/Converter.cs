using System;

namespace tubeLoadNative.Services
{
    public static class Converter
    {
        public static string MillisecondsToString(int milliseconds)
        {
            return TimeSpan.FromMilliseconds(milliseconds).ToString(@"mm\:ss");
        }
    }
}
