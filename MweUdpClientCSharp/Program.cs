using System.Net;
using System.Threading;

namespace MweUdpClientCSharp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = new UdpMulticastClient(IPAddress.Parse("235.1.1.1"), 8480, 0);
            var cts = new CancellationTokenSource();
            client.ReceiveDataForever(cts.Token);
        }
    }
}