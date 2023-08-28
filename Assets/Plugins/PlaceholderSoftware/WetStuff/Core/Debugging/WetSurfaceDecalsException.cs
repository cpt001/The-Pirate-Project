using System;

namespace PlaceholderSoftware.WetStuff.Debugging
{
    public class WetSurfaceDecalsException
        : Exception
    {
        internal WetSurfaceDecalsException(string message)
            : base(message)
        {
        }
    }
}