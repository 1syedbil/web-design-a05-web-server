using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WDD_A05
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WebServer server = new WebServer("C:/localWebSite", "10.192.40.223", "8080");

            server.StartWebServer();
        }
    }
}
