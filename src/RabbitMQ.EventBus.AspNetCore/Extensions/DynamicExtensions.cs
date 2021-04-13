using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RabbitMQ.EventBus.Extension.AspNetCore.Extensions
{
    internal static class DynamicExtensions
    {
        /// <summary>
        /// 消息序列化方法
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string Serialize<TMessage>(this TMessage message)
        {
            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// 消息反序列化方法
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public static TMessage Deserialize<TMessage>(this string message)
        {
            return JsonConvert.DeserializeObject<TMessage>(message);
        }

        /// <summary>
        /// 文本转字节数组
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this string body)
        {
            return Encoding.UTF8.GetBytes(body);
        }

        /// <summary>
        /// 字节数组转文本
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static string GetString(this byte[] body)
        {
            return Encoding.UTF8.GetString(body);
        }
    }
}
