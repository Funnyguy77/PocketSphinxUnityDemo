using System;

namespace TarCs
{
    public class TarException : Exception
    {
        public TarException(string message) : base(message)
        {
        }
    }
}