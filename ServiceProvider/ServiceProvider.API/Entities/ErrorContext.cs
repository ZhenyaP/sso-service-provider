using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ServiceProvider.API.Entities
{
    public class ErrorContext
    {
        private static readonly AsyncLocal<ErrorContext> CurrentErrorContext = new AsyncLocal<ErrorContext>();
        private readonly Lazy<ConcurrentBag<string>> _attachedMessages = new Lazy<ConcurrentBag<string>>(() => new ConcurrentBag<string>());

        private ErrorContext()
        { }

        public static ErrorContext Current
        {
            get
            {
                var errorContext = CurrentErrorContext.Value;
                if (errorContext == null)
                {
                    CurrentErrorContext.Value = errorContext = new ErrorContext();
                }
                return errorContext;
            }
        }

        public static ErrorContext CreateNewErrorContext()
        {
            var errorContext = new ErrorContext();
            CurrentErrorContext.Value = errorContext;
            return errorContext;
        }

        public void AttachMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _attachedMessages.Value.Add(message);
            }
        }

        public IReadOnlyList<string> GetMessages()
        {
            return _attachedMessages.Value.ToArray();
        }
    }
}
