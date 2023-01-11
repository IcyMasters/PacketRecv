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
        private Process _processcreated;
        private int _targetpid = 0;
        private bool _returnlist;
        private int _time = 0;
        private List<NetStat> _packetslist = new List<NetStat>();
        private HttpRequestHandler _httprequest = new HttpRequestHandler();
        
        
        public async Task<List<NetStat>> Start(int pid)
        {
            this._targetpid = pid;
            this.CreateProcess(pid);
            this.CreateTimer(this._time);
            this.CloseProcess();
            if (_returnlist == false)
            {
                await _httprequest.DataHandler(this._packetslist);
            }
            return await Task.FromResult(this._packetslist);

        }

        public async Task<List<NetStat>> Start(string pName)
        {
            int PidRes = this.FindPidFromName(pName);
            if (PidRes == 0)
            {
                return await Task.FromResult(this._packetslist);
            }
            this._targetpid = PidRes;
            this.CreateProcess(PidRes);
            this.CreateTimer(this._time);
            this.CloseProcess();
            if (_returnlist == false)
            {
                await _httprequest.DataHandler(this._packetslist);
            }
            return await Task.FromResult(this._packetslist);
        }
         
        
        #region Process Handler

        public bool CloseProcess()
        {
            try
            {
                this._processcreated.CancelOutputRead();
                this._processcreated.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        

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
        /// <param name="pid"> Target's Process ID</param>
        private void CreateProcess(int pid)
        {
            this._processcreated.StartInfo.FileName = "netstat.exe";
            this._processcreated.StartInfo.Arguments =  pid + " -o -n";
            this._processcreated.StartInfo.UseShellExecute = false;
            this._processcreated.StartInfo.RedirectStandardOutput = true;
            this._processcreated.Start();
            this._processcreated.OutputDataReceived += ProcessOnOutputDataReceived;
            this._processcreated.BeginOutputReadLine();

        }
        
        
        #endregion
        
        #region Process Output Handler
        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = e.Data;
            this.DataToStruct(data);
        }

        private void DataToStruct(string c)
        {
            var data = Regex.Split(c, "\\s+");
            
            if (!(data.Length >= 6))
            {
                return;
            }
            try
            {
                State state;
                NetStat netStat = new NetStat();

                if (data[5] != this._targetpid.ToString())
                {
                    return;
                }
                if (data[3].Split(':')[0].Equals("127.0.0.1") || data[3].Split(':')[0].Equals("198.0.0.1"))
                {
                    return;
                }

                netStat.CType = data[1] == "TCP" ? ConnectionType.TCP : data[1] == "UDP" ? ConnectionType.UDP : ConnectionType.Other;
                netStat.Pid = this._targetpid;
                netStat.LAddress = netStat.iPAddress(data[2].Split(':')[0]);
                netStat.FAddress = netStat.iPAddress(data[3].Split(':')[0]);
                if (!Enum.TryParse<State>(data[4], out state))
                {
                    throw new Exception("State Cant Defined In List");
                }
                netStat.State = state;
                this._packetslist.Add(netStat);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
                //TODO Add Logger
            }



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
            this._processcreated = new Process();
            this._time = time;
            this._httprequest.SiteAddress = ip;
        }
        
    }
}
