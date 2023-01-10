using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketRecv.Main
{
    /// <summary>
    /// Class <c>WindowsPacketHandler</c> models netstat functions to return all packets from Process after <c>_time</c> seconds
    /// </summary>
    public class WindowsPacketHandler : IDisposable
    {
        private Process _processcreated;
        private int _targetpid = 0;
        private bool _returnlist = false;
        private int _time = 0;
        private List<int> _packetslist = new List<int>();

        
        
        public Task<List<int>> Start(int pid)
        {
            this._targetpid = pid;
            this.CreateProcess(pid);

            return Task.FromResult(this._packetslist);
        }

        public Task<List<int>> Start(string pName)
        {
            int PidRes = this.FindPidFromName(pName);
            if (PidRes == 0)
            {

            }
            this._targetpid = PidRes;
            this.CreateProcess(PidRes);

            return Task.FromResult(this._packetslist);
        }
        
        
        
        #region Process Handler

        public bool CloseProcess()
        {
            try
            {
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
            this._processcreated.StartInfo.FileName = "cmd.exe";
            this._processcreated.StartInfo.Arguments = "netstat" + pid + "-o -n";
            this._processcreated.StartInfo.UseShellExecute = false;
            this._processcreated.StartInfo.RedirectStandardOutput = true;
            this._processcreated.OutputDataReceived += ProcessOnOutputDataReceived;
            this._processcreated.Start();
            
        }
        
        
        #endregion
        
        #region Process Output Handler
        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }



        
        #endregion
        
        public WindowsPacketHandler(bool returnlist, int time=30)
        {
            this._returnlist = returnlist;
            this._processcreated = new Process();
            this._time = time;
        }
        
        #region IDisposable
        ~WindowsPacketHandler()
        {

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
