using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.EventBus.Extension.AspNetCore.Events;

namespace AspNetCore.WebSample.Models
{
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
}
