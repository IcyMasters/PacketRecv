using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PacketRecv.Main
{
    public enum State
    {
        CLOSE_WAIT,
        CLOSED,
        ESTABLISHED,
        FIN_WAIT_1,
        FIN_WAIT_2,
        LAST_ACK,
        LISTEN,
        SYN_RECEIVED,
        SYN_SEND,
        SYN_SENT,
        TIMED_WAIT,
        None, // UDP
    }

    public enum ConnectionType
    {
        TCP,
        UDP,
        Other,
    }
    public struct NetStat
    {
        public ConnectionType CType;
        public IPAddress LAddress;
        public IPAddress FAddress;
        public State State;
        public int Pid;

        public IPAddress iPAddress(string ip) => IPAddress.Parse(ip);

    }
}
