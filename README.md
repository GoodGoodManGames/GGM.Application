# GGMApplication
GGMApplication은 GGMContext과 설정을 이용하여 서비스를 부팅하여 어플리케이션을 실행하는 부분입니다.

## Config
GGMApplication은 ConfigAttribute을 지원하며 설정 값을 프로퍼티에 매핑해주는 기능을 가지고 있습니다.
ApplicationContext로 생성된 객체들은 자동으로 설정이 주입됩니다.

```csharp
// appilcation.cfg
WebService.ResourcePath = "./Resources"

// MyService.cs
[Config("WebService.ResourcePath")]
public string ResourcePath { get; set; } // "./Resources"가 주입된다.
```

> 주의하여야 할 점은, 설정 주입은 생성자가 끝난 뒤에 주입된다는 것입니다.

## Service
사용자는 GGMApplication을 실행할때 Service를 지정해줄 수 있으며, 지정된 서비스는 GGMApplication에 의해 생성된 후 의존성과 설정을 주입받습니다.

Service는 GGMContext에 의해 관리되는 것이 아니라 GGMApplication에 의해 관리됩니다. 때문에  Managed와 다르게 다른 서비스 혹은 Managed에 주입되지 않습니다.

```csharp
// Main.cs
public static void Main(stringp[] args)
{
    GGMApplication.Run(
        typeof(Program)
        , "./application.cfg"
        , args
        , typeof(MyWebService) // service 1
        , typeof(MyWebService) // service 2
        );
}

// MyWebService.cs
public class MyWebService : WebService
{
    [AutoWired]
    public MyWebService(MyController myController) // MyController 의존성 주입
        : base(null, new string[] { "http://localhost:8002/" }, myController)
    { }

    [Config("WebService.ResourcePath")]
    public string ResourcePath { get; set; } // 설정값 주입
    
    public override Task Boot(string[] arguments)
    {
        /** */
    }
}
```