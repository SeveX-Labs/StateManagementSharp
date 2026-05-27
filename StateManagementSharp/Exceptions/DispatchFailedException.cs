using System;

namespace StateManagementSharp.Exceptions;

public class DispatchFailedException(string message, Exception? originalException = null) : Exception(message)
{
    public Exception? OriginalException { get; } = originalException;
}