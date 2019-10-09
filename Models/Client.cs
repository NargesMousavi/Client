using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace myClient.Models
{
    public class ConcreteClient : IClient
    {
        public ClientWebSocket WebSocket { get; private set; }
        public ConcreteClient(ClientWebSocket webSocket)
        {
            WebSocket = webSocket;
        }
        public async Task<IResponseMessage> SendAsync(IRequestMessage requestMessage)
        {
            var message = new ArraySegment<byte>(Encoding.Default.GetBytes(requestMessage.Body));
            await WebSocket.SendAsync(message, WebSocketMessageType.Binary, true, CancellationToken.None);
            var buffer = new ArraySegment<byte>(new byte[1024]);
            var result = await WebSocket.ReceiveAsync(buffer, CancellationToken.None);
            var res = Encoding.UTF8.GetString(buffer.Array).TrimEnd((char)0);
            return new Respone { Body = res, Status = result.CloseStatus == WebSocketCloseStatus.NormalClosure ? false : true };
        }
        public async Task<T> SendAsync<T>(IRequestMessage requestMessage) where T : IResponseMessage
        {
            var ts = await SendAsync(requestMessage);
            return (T)ts;
        }
    }
    interface IClient
    {
        Task<IResponseMessage> SendAsync(IRequestMessage requestMessage);
        Task<TResponseMessage> SendAsync<TResponseMessage>(IRequestMessage requestMessage)
        where TResponseMessage : IResponseMessage;
    }
    public interface IMessage
    {
        string Body { get; set; }
    }
    public interface IRequestMessage : IMessage
    {
    }
    public interface IResponseMessage : IMessage
    {
        bool Status { get; set; }
    }
    public class Request : IRequestMessage
    {
        public string Body { get; set; }
    }
    public class Respone : IResponseMessage
    {
        public string Body { get; set; }
        public bool Status { get; set; }
    }
}