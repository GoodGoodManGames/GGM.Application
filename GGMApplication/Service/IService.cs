using System;

namespace GGM.Application.Service
{
    /// <summary>
    ///     Application 부팅 시에 Application에서 실행해줄 수 있는 서비스 클래스의 인터페이스입니다.
    ///     사용자들은 해당 인터페이스를 구현하여 Application에 추가해 줄 수 있으며, 서비스의 Boot들은 Application의 Run이 완료되기 전에 호출됩니다.
    /// </summary>
    public interface IService
    {
        /// <summary>
        ///     서비스의 구분자입니다. 이는 Application이 Service를 생성하면서 부여해줍니다.
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        ///     서비스를 실행시킵니다.
        /// </summary>
        void Boot(string[] arguments);
    }
}
