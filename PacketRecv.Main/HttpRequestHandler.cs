using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PacketRecv.Main
{
    public class HttpRequestHandler
    {
        private static readonly HttpClient Client = new HttpClient();
        public string SiteAddress { get; set; }
        


        private async Task SendRequest(string ip)
        {
            Console.WriteLine($"Sending {ip}");
            var body = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("IpAddress", ip)
            };
            try
            {
                var result = await Client.PostAsync(
                    this.SiteAddress,
                    new FormUrlEncodedContent(body)
                );

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    //TODO Add Logger
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                //TODO Add Warning for Internet Connection
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
                //TODO Add Logger
            }
            
        }

        public async Task DataHandler(List<NetStat> nets)
        {
            foreach(var packet in nets)
            {
                try
                {
                    await this.SendRequest(packet.FAddress.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
               

            }

            
        }
    }
}
