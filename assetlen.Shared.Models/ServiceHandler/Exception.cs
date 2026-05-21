namespace assetlen.ServiceHandler
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
    public class UnAuthorizedException : Exception
    {
        public UnAuthorizedException(string message) : base(message)
        {
        }
    }
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message)
        {
        }
    }
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
    public class ServerErrorException : Exception
    {
        public ServerErrorException(string message) : base(message)
        {
        }
    }
    public class SyncDisabledException : Exception
    {
        public SyncDisabledException() : base("Syncing is disabled") { }
    }

    public class NoInternetException : Exception
    {
        public NoInternetException() : base("No internet connection") { }
    }
}
