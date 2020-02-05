using System;

namespace ServiceProvider.API.Exceptions
{
    public class JsonWebKeyNotFoundException : Exception
    {
        public JsonWebKeyNotFoundException() { }

        public JsonWebKeyNotFoundException(string message) : base(message) { }

        public JsonWebKeyNotFoundException(string message, Exception inner)
            : base(message, inner) { }
    }
}
