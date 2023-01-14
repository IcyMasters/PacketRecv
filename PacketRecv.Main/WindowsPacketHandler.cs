using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace PacketRecv.Main
{
    /// <summary>
    /// Class <c>WindowsPacketHandler</c> models netstat.exe functions to return all packets from Process after <c>_time</c> seconds
    /// </summary>
    public class WindowsPacketHandler
    {
        private int _targetpid = 0;
        private bool _returnlist;
        private int _time = 0;
        private List<NetStat> _packetslist = new List<NetStat>();
        private HttpRequestHandler _httprequest = new HttpRequestHandler();
        
        
        public async Task<List<NetStat>> Start(int pid)
        {
            this._targetpid = pid;
            var process = this.CreateProcess();
            this.CreateTimer(this._time);
            
            if (_returnlist == false)
            {
                await _httprequest.DataHandler(this._packetslist);
            }
            return this._packetslist;

        }

        public async Task<List<NetStat>> Start(string pName)
        {
            int pidRes = this.FindPidFromName(pName);
            if (pidRes == 0)
            {
                return this._packetslist;
            }
            this._targetpid = pidRes;
            var process = this.CreateProcess();
            this.CreateTimer(this._time);
            if (_returnlist == false)
            {
                await _httprequest.DataHandler(this._packetslist);
            }
            return this._packetslist;
        }
         
        
        #region Process Handler
        

        public int FindPidFromName(string pName)
        {

            Process processId = Process.GetProcessesByName(pName).FirstOrDefault();
            if (processId is null)
            {
                Console.WriteLine("Not Found");
                return 0;
            }

            return processId.Id;


        }
        

        /// <summary>
        /// Create Netstat cmd with Event Handler
        /// -b => return name of process
        /// -o => return pid of process that create packet
        /// -n => return ip & port in numeric format
        /// </summary>
        private Process CreateProcess()
        {
            Process process = new Process();
            process.StartInfo.FileName = "netstat.exe";
            process.StartInfo.Arguments =  " -o -n";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.OutputDataReceived += ProcessOnOutputDataReceived;
            process.BeginOutputReadLine();
            return process;


        }
        
        
        #endregion
        
        #region Process Output Handler
        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var data = this.DataToStruct(e.Data);
            if (data.Pid != 0)
            {
                this._packetslist.Add(data);
            }

            return;
        }

        private NetStat DataToStruct(string c)
        {
            var data = Regex.Split(c, "\\s+");
            
            if (!(data.Length >= 6))
            {
                return new NetStat();
            }
            State state;
                

                if (data[5] != this._targetpid.ToString())
                {
                    return new NetStat();
                }
                if (data[3].Split(':')[0].Equals("127.0.0.1") || data[3].Split(':')[0].Equals("198.0.0.1"))
                {
                    return new NetStat();
                }
                
                if (!Enum.TryParse<State>(data[4], out state))
                {
                    throw new Exception("State Cant Defined In List");
                }
                
                NetStat netStat = new NetStat(
                        data[1] == "TCP" ? ConnectionType.TCP : data[1] == "UDP" ? ConnectionType.UDP : ConnectionType.Other,
                        IPAddress.Parse(data[2].Split(':')[0]),
                        IPAddress.Parse(data[3].Split(':')[0]),
                        state,
                        this._targetpid
                );
                return netStat;
            }



        #endregion

        #region Timer Handler
        private void CreateTimer(int time)
        {

            var timer =  Task.Delay(this.SecondsToMiliSeconds(time));
            timer.Wait();
        }

        private int SecondsToMiliSeconds(int time) => time * 1000;
        #endregion

        public WindowsPacketHandler(bool returnlist, string ip, int time = 30)
        {
            this._returnlist = returnlist;
            this._time = time;
            this._httprequest.SiteAddress = ip;
        }
        
    }
}
