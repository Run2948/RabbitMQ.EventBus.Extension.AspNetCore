using System;
using System.Collections.Generic;
using System.Linq;

namespace RabbitMQ.EventBus.Extension.AspNetCore.Extensions
{
    internal static class TypeExtensions
    {
        public static IEnumerable<Type> GetAssemblies(this Type type)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(type)));
        }
        
        public static IEnumerable<Type> GetMakeGenericType(this Type interfaceType, Type makeType)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(interfaceType.MakeGenericType(makeType))));
        }
    }
}
