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
            int ask = Int32.Parse(Console.ReadLine());
            await _windows.Start(14552);
            Console.Write("\n EOF");
        }
    }
}