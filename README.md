[![License: MIT](https://www.rabbitmq.com/img/RabbitMQ-logo.svg)](https://www.rabbitmq.com/)

[TOC]

# RabbitMQ.EventBus.Extension.AspNetCore

Lightweight EventBus Extension Library implementation of  `RabbitMQ.Client` in  ASP .NET Core Application.

## Install Package

[https://www.nuget.org/packages/RabbitMQ.EventBus.Extension.AspNetCore](https://www.nuget.org/packages/RabbitMQ.EventBus.Extension.AspNetCore)

## Configure

`Startup.cs`

### 1. Connection registration

~~~ csharp
public void ConfigureServices(IServiceCollection services)
{
    // 注册 RabbitMQ
    services.AddRabbitMQEventBus(() => Configuration.GetConnectionString("Rabbit"),
    eventBusOption =>
    {
        eventBusOption.ClientProvidedAssembly(typeof(Startup).Namespace);
        eventBusOption.EnableRetryOnFailure(true, 5000, TimeSpan.FromSeconds(30));
        eventBusOption.RetryOnFailure(TimeSpan.FromSeconds(1));
        eventBusOption.MessageTTL(2000);
        eventBusOption.SetBasicQos(10);
        eventBusOption.DeadLetterExchangeConfig(config =>
        {
            config.Enabled = true;
             config.ExchangeNameSuffix = "";
        });
    });
    
    services.AddControllers();
}
~~~
### 2. Message subscription

#### 2.1  Automatic  subscription

~~~ csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseRouting();

    app.UseRabbitMQEventBus(); // Automatic subscription

    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}
~~~
#### 2.2 Manual  subscription

~~~ csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IRabbitMQEventBus eventBus)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseRouting();

    eventBus.Subscribe<MessageBody>(); // Manual subscription 

    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}
~~~
### 3. Publish message

~~~ csharp
[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
    private readonly IRabbitMQEventBus _eventBus;

    public ValuesController(IRabbitMQEventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    // GET: api/<ValuesController>
    [HttpGet]
    public ActionResult<string> Get()
    {
        return "The path variable must be 0 or 1.";
    }

    // GET: api/<ValuesController>/5
    [HttpGet("{id}")]
    public ActionResult<string> Get(int id)
    {
        var routingKey = $"rabbitmq.eventbus.test{(id > 0 ? id + "" : "")}";

        _eventBus.Publish(new
        {
            Body = $"{routingKey} => 发送消息",
            Time = DateTimeOffset.Now
        }, exchange: "RabbitMQ.EventBus.Simple", routingKey: routingKey);

        return "Ok";
    }
}
~~~
### 4.  Subscription message

~~~ csharp
// routingKey: rabbitmq.eventbus.test
[EventBus(Exchange = "RabbitMQ.EventBus.Simple",RoutingKey = "rabbitmq.eventbus.test")]
public class MessageBody : IEvent
{
    public string Body { get; set; }
    public DateTimeOffset Time { get; set; }
}

public class MessageBodyHandle : IEventHandler<MessageBody>, IDisposable
{
    private readonly Guid _id;
    private readonly ILogger<MessageBodyHandle> _logger;

    public MessageBodyHandle(ILogger<MessageBodyHandle> logger)
    {
        _id = Guid.NewGuid();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(EventHandlerArgs<MessageBody> args)
    {
        Console.WriteLine("==================================================");
        Console.WriteLine(_id + "=>" + nameof(MessageBody));
        Console.WriteLine(args.Event.Body);
        Console.WriteLine(args.Original);
        Console.WriteLine(args.Redelivered);
        Console.WriteLine("==================================================");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Console.WriteLine("释放");
    }
}


// routingKey: rabbitmq.eventbus.test1
[EventBus(Exchange = "RabbitMQ.EventBus.Simple",RoutingKey = "rabbitmq.eventbus.test1")]
public class MessageBody1 : IEvent
{
    public string Body { get; set; }
    public DateTimeOffset Time { get; set; }
}

public class MessageBodyHandle11 : IEventHandler<MessageBody1>, IDisposable
{
    private readonly Guid _id;
    private readonly ILogger<MessageBodyHandle11> _logger;

    public MessageBodyHandle11(ILogger<MessageBodyHandle11> logger)
    {
        _id = Guid.NewGuid();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(EventHandlerArgs<MessageBody1> args)
    {
        Console.WriteLine("==================================================");
        Console.WriteLine(_id + "=>" + nameof(MessageBody1));
        Console.WriteLine(args.Event.Body);
        Console.WriteLine(args.Original);
        Console.WriteLine(args.Redelivered);
        Console.WriteLine("==================================================");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Console.WriteLine("释放");
    }
}

public class MessageBodyHandle12 : IEventHandler<MessageBody1>
{
    private readonly Guid _id;
    private readonly ILogger<MessageBodyHandle12> _logger;

    public MessageBodyHandle12(ILogger<MessageBodyHandle12> logger)
    {
        _id = Guid.NewGuid();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(EventHandlerArgs<MessageBody1> args)
    {
        Console.WriteLine("==================================================");
        Console.WriteLine(_id + "=>" + nameof(MessageBody1));
        Console.WriteLine(args.Event.Body);
        Console.WriteLine(args.Original);
        Console.WriteLine(args.Redelivered);
        Console.WriteLine("==================================================");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Console.WriteLine("释放");
    }
}
~~~