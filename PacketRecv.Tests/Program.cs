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
            WindowsPacketHandler _windows = new WindowsPacketHandler(true, "http://127.0.0.1:5002", 5);
            int ask = Int32.Parse(Console.ReadLine());
            var a = await _windows.Start(ask);
            Console.Write(a);
            Console.Write("\n EOF");
            Console.Read();
        }
    }
}