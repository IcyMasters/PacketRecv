using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PacketRecv.Main
{
    public class PacketRecv
    {
        private const int AF_INET = 2;
        // s
        #region TCP
        /*
         IPHLPAPI_DLL_LINKAGE DWORD GetExtendedTcpTable(
         [out]     PVOID           pTcpTable, =>  void pointer => void * in c++/c => pointer to 
                                                  a address memory without knowing type of that
         [in, out] PDWORD          pdwSize, =>  int-32 pointer 
         [in]      BOOL            bOrder, => bool
         [in]      ULONG           ulAf, => unsigned long int/decimal
         [in]      TCP_TABLE_CLASS TableClass, => struct
         [in]      ULONG           Reserved => unsigned long int/decimal
          );
          this function return pdwSize , this field will contain the correct size of the structure.
         */
        
        
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize,
            bool bOrder, int ulAf, TcpTableClass tableClass, uint reserved = 0);

        private List<TcpProcessRecord> GetAllTcpConnections()
        {
            int buffer = 0;

            List<TcpProcessRecord> tcpProcesses= new List<TcpProcessRecord>();
            
            
            //Get Size Of Tcp Table (Check Comments in Dllimport) / output = buffer
            uint res = GetExtendedTcpTable(
                IntPtr.Zero,
                ref buffer,
                true,
                AF_INET,
                TcpTableClass.TCP_TABLE_OWNER_PID_ALL
                );

            //alocate memory from TCP Table Size
            IntPtr tcpProcessesPtr = Marshal.AllocHGlobal(buffer);

            try
            {
                // tcpProcessesPtr have the table now
                res = GetExtendedTcpTable(
                    tcpProcessesPtr,
                    ref buffer,
                    true,
                    AF_INET,
                    TcpTableClass.TCP_TABLE_OWNER_PID_ALL
                );
                
                // if res == 0 means GetExtendedTcpTable Failed
                if (res != 0)
                {
                    return new List<TcpProcessRecord>();
                }
                
                
                MIB_TCPTABLE_OWNER_PID tcpRecordsTable = (MIB_TCPTABLE_OWNER_PID)
                    Marshal.PtrToStructure(tcpProcessesPtr, 
                        typeof(MIB_TCPTABLE_OWNER_PID));
                
                
                IntPtr tableRowPtr = (IntPtr)((long)tcpProcessesPtr + 
                                              Marshal.SizeOf(tcpRecordsTable.dwNumEntries));

                for (int row = 0; row < tcpRecordsTable.dwNumEntries; row++)
                {
                    MIB_TCPROW_OWNER_PID tcpRow = (MIB_TCPROW_OWNER_PID)Marshal.
                        PtrToStructure(tableRowPtr, typeof(MIB_TCPROW_OWNER_PID));

                    tcpProcesses.Add(new TcpProcessRecord(
                                          new IPAddress(tcpRow.localAddr),
                                          new IPAddress(tcpRow.remoteAddr),
                                          BitConverter.ToUInt16(new byte[2] {
                                              tcpRow.localPort[1],
                                              tcpRow.localPort[0] }, 0),
                                          BitConverter.ToUInt16(new byte[2] {
                                              tcpRow.remotePort[1],
                                              tcpRow.remotePort[0] }, 0),
                                            tcpRow.owningPid, tcpRow.state));


                    tableRowPtr = (IntPtr)((long)tableRowPtr + Marshal.SizeOf(tcpRow));


                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                // Add Log
            }
            
            catch (Exception exception)
            {
                //Add Log
            } 
            finally
            {
                Marshal.FreeHGlobal(tcpProcessesPtr);
            }
            return tcpProcesses != null ? tcpProcesses.Distinct()
                .ToList<TcpProcessRecord>() : new List<TcpProcessRecord>();



        }
        #endregion
        
        #region UDP
        // IPHLPAPI_DLL_LINKAGE DWORD GetExtendedUdpTable(
        //     [out]     PVOID           pUdpTable, => pointer void => void*
            // [in, out] PDWORD          pdwSize, => int-32 pointer => DWORD *
            // [in]      BOOL            bOrder, 
            // [in]      ULONG           ulAf, => unsigned long int/decimal
            // [in]      UDP_TABLE_CLASS TableClass,
            // [in]      ULONG           Reserved => unsigned long int/decimal
        // );
        
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int pdwSize,
            bool bOrder, int ulAf, UdpTableClass tableClass, uint reserved = 0);

        

        private List<UdpProcessRecord> GetAllUdpConnections()
        {
            int buffer = 0;
            List<UdpProcessRecord> udpProcess = new List<UdpProcessRecord>();
            
            uint res = GetExtendedUdpTable(
                IntPtr.Zero,
            ref buffer, 
                true,
                AF_INET,
                UdpTableClass.UDP_TABLE_OWNER_PID);

            IntPtr udpprocessptr = Marshal.AllocHGlobal(buffer);
            try
            {
                res = GetExtendedUdpTable(
                    udpprocessptr,
                    ref buffer,
                    true,
                    AF_INET,
                    UdpTableClass.UDP_TABLE_OWNER_PID
                );

                if (res != 0)
                {
                    return new List<UdpProcessRecord>();
                }

                MIB_UDPTABLE_OWNER_PID UdpRecordsTable =
                    (MIB_UDPTABLE_OWNER_PID)Marshal.PtrToStructure(udpprocessptr, typeof(MIB_UDPTABLE_OWNER_PID));

                IntPtr UdpTableRowPtr = (IntPtr)((long)udpprocessptr + Marshal.SizeOf(UdpRecordsTable.dwNumEntries));


                for (int i = 0; i < UdpRecordsTable.dwNumEntries; i++)
                {
                    MIB_UDPROW_OWNER_PID UdpRow =
                        (MIB_UDPROW_OWNER_PID)Marshal.PtrToStructure(UdpTableRowPtr, typeof(MIB_UDPROW_OWNER_PID));

                    udpProcess.Add(new UdpProcessRecord(
                        new IPAddress(UdpRow.localAddr),
                        BitConverter.ToUInt16(new byte[2]
                        {
                            UdpRow.localPort[0], UdpRow.localPort[1]
                        }, 0),
                        UdpRow.owningPid));

                    UdpTableRowPtr = (IntPtr)((long)UdpTableRowPtr + Marshal.SizeOf(UdpRow));
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                // Add Logger
            }
            catch (Exception exception)
            {
                // Add Logger
            }

            finally
            {
                Marshal.FreeHGlobal(udpprocessptr);
            }

            return udpProcess != null ? udpProcess.Distinct()
                    .ToList<UdpProcessRecord>() : new List<UdpProcessRecord>();
            }
            
        
        #endregion
        
        // Not Ready For Production
    }
}