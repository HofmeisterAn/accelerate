namespace Accelerate.Exceptions;

public sealed class AccelerateException : Exception
{
    public AccelerateException(AccelerateErrorCode errorCode) : base("Accelerate command failed: " + errorCode)
    {
        ErrorCode = errorCode;
    }

    public AccelerateErrorCode ErrorCode { get; }
}