using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MweUdpClientCSharp
{
    public sealed class UdpMulticastClient : IDisposable
    {
        private readonly UdpClient _udpClient;
        private IPEndPoint _localEndpoint;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Task _receiveTask;

        public UdpMulticastClient(IPAddress multicastAddress, int port, int timeoutInMs, IPAddress localAddress = null)
        {
            _udpClient = new UdpClient();

            _udpClient.ExclusiveAddressUse = false;
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.ExclusiveAddressUse = false;

            _localEndpoint = new IPEndPoint(localAddress ?? IPAddress.Any, port);
            _udpClient.Client.ReceiveTimeout = timeoutInMs;

            _udpClient.Client.Bind(_localEndpoint);
            _udpClient.JoinMulticastGroup(multicastAddress, _localEndpoint.Address);
        }

        public void ReceiveDataForever(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                var response = Receive();
                if (response == null)
                    continue;
                Console.WriteLine(ASCIIEncoding.ASCII.GetString(response));
            }
        }

        private byte[] Receive()
        {
            try
            {
                return _udpClient.Receive(ref _localEndpoint);
            }
            catch (SocketException e)
            {
                // Receive timed out after X seconds.  Try again.  A timeout is set on receive so that the program can periodically check for cancellation if no events are flowing.
                return null;
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _receiveTask.Wait(TimeSpan.FromSeconds(2));
            _udpClient.Close();
        }
    }
}