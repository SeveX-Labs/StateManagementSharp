using System;

namespace StateManagementSharp.Exceptions;

public class CommitFailedException(string message, Exception? originalException = null) : Exception(message)
{
    public Exception? OriginalException { get; } = originalException;
}