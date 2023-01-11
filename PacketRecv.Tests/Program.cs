using PacketRecv.Main;
using System;
using System.Threading.Tasks;


namespace PacketRecv.Tests
{
    internal class Program
    {
        
        async static Task Main(string[] args)
        {
            Console.WriteLine("start");
            WindowsPacketHandler _windows = new WindowsPacketHandler(false, "http://127.0.0.1:5002", 3);
            Task.Run(() => _windows.Start(20396));
        }
    }
}