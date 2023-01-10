namespace PacketRecv.Main
{
    public class PacketRecv
    {
        public void Main()
        {
            WindowsPacketHandler w = new WindowsPacketHandler(true);
            var k = w.Start(12).Result;
        }
    }
}