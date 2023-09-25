namespace Accelerate.Exceptions;

public sealed class AccelerateException : Exception
{
    public AccelerateException(AccelerateErrorCode errorCode)
    {
        _ = errorCode;
    }
}