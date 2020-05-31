using System;

namespace MorkoBotRavenEdition.Utilities.Exceptions
{
    internal class ActionException : Exception
    {
        internal ActionException(string message) : base(message) {}
    }
}
