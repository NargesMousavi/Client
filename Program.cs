using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using myClient.Models;
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
                int timeOut = 3000;
                using (var cts = new CancellationTokenSource(timeOut))
                {
                    Task taskConnect = webSocket.ConnectAsync(serverUri, cts.Token);
                    await taskConnect;
                }
                var client = new ConcreteClient(webSocket);
                Console.WriteLine("Enter message:");
                var msg = Console.ReadLine();
                while (!string.Equals(msg, "exit"))
                {
                    // var res = await client.SendAsync(new Request { Body = msg });
                    var res = await client.SendAsync<IResponseMessage>(new Request { Body = msg });
                    Console.WriteLine($"Response: {res.Body}");
                    if (!res.Status)
                        break;
                    Console.WriteLine("Enter message");
                    msg = Console.ReadLine();
                }
                Console.WriteLine("End of messaging." + webSocket.CloseStatusDescription);
            }
        }
    }

}
