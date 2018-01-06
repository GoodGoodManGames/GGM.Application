namespace GGM.Application.Exception
{
    /// <summary>
    ///     RunApplicationException의 에러타입 입니다. 
    /// </summary>
    public enum RunApplicationError
    {
        /// <summary>
        ///     Service클래스가 아닌데 Service클래스로서 등록함
        /// </summary>
        IsNotServiceClass,
        /// <summary>
        ///     추상 타입을 Service클래스로서 등록함
        /// </summary>
        IsAbstractClass,
        
        /// <summary>
        ///     사용할 수 있는 생성자가 존재하지 않음
        /// </summary>
        NotExistMatchedServiceConstructor
    }

    /// <summary>
    ///     Application을 실행하는데 발생하는 예외입니다.
    /// </summary>
    public sealed class RunApplicationException : System.Exception
    {
        
        /// <param name="runApplicationError">에러타입</param>
        public RunApplicationException(RunApplicationError runApplicationError)
            : base($"Fail to run application. {nameof(RunApplicationError)} : {runApplicationError}")
        {
            RunApplicationError = runApplicationError;
        }

        /// <summary>
        ///     에러타입
        /// </summary>
        public RunApplicationError RunApplicationError { get; }

        internal static void Check(bool condition, RunApplicationError error)
        {
            if (!condition)
                throw new RunApplicationException(error);
        }
    }
}