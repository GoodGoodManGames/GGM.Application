namespace GGM.Application.Exception
{
    public enum RunApplicationError
    {
        IsNotServiceClass,
        IsAbstractClass,
        NotExistMatchedServiceConstructor
    }

    public sealed class RunApplicationException : System.Exception
    {
        public RunApplicationException(RunApplicationError runApplicationError)
            : base($"Fail to run application. {nameof(RunApplicationError)} : {runApplicationError}")
        {
            RunApplicationError = runApplicationError;
        }

        public RunApplicationError RunApplicationError { get; }
    }
}
