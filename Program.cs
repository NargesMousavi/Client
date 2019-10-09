using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace myClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                StartCommunication().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async static Task StartCommunication()
        {
            using (var webSocket = new ClientWebSocket())
            {
                var serverUri = new Uri("ws://localhost:5000/ws");
                int timeOut = 50000;
                using (var cts = new CancellationTokenSource(timeOut))
                {
                    Task taskConnect = webSocket.ConnectAsync(serverUri, cts.Token);
                    await taskConnect;
                }
                var client = new ConcreteClient(webSocket);
                Console.WriteLine("Enter message:");
                var msg = Console.ReadLine();
                while (!string.Equals(msg, "exit") && webSocket.State!=WebSocketState.CloseReceived)
                {
                    var res = await client.SendAsync(new Request { Body = msg });
                    Console.WriteLine($"Response: {res.Body}");
                    Console.WriteLine("Enter message");
                    msg = Console.ReadLine();
                }
                Console.WriteLine("End of messaging."+ webSocket.CloseStatusDescription);
            }
        }
    }
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
            return new Respone { Body = res, Status = true };
        }
        public async Task<TResponseMessage> SendAsync<TResponseMessage>(IRequestMessage requestMessage) where TResponseMessage : IResponseMessage
        {
            throw new NotImplementedException();
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
