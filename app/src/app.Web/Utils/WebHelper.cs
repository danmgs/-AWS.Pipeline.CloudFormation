using System;
using System.Net;
using System.Net.Sockets;

namespace app.Web
{
    public static class WebHelper
    {   
        public static string DisplayLocalHostName()
        {
            string hostName = null;

            try
            {
                // Get the local computer host name.
                hostName = Dns.GetHostName();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }

            return hostName;
        }
    }
}
