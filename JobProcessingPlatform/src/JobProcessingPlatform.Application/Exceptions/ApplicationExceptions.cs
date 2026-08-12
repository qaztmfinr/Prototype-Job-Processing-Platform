namespace JobProcessingPlatform.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

public class JobProcessingException : Exception
{
    public JobProcessingException(string message) : base(message) { }
    public JobProcessingException(string message, Exception innerException) : base(message, innerException) { }
}
