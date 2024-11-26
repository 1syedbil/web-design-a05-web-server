using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WDD_A05
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[] serverPropertites = ParseCmdLineArgs(args);

            if (serverPropertites[0] == "webroot incorrect")
            {
                Console.WriteLine("Incorrect webroot format.");
                return;
            }
            else if (serverPropertites[0] == "ip incorrect")
            {
                Console.WriteLine("Incorrect ip format.");
                return;
            }
            else if (serverPropertites[0] == "port incorrect")
            {
                Console.WriteLine("Incorrect port format.");
                return;
            }

            WebServer server = new WebServer(serverPropertites[0], serverPropertites[1], serverPropertites[2]);

            server.StartWebServer();
        }

        static string[] ParseCmdLineArgs(string[] args)
        {
            string[] serverProperties = new string[3];

            Regex webRootFormat = new Regex("[a-zA-Z]:[\\\\\\/]");

            Regex ipFormat = new Regex("^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");

            foreach (string s in args)
            {
                if (webRootFormat.IsMatch(s))
                {
                    serverProperties[0] = s;
                    break;
                }
            }
            if (serverProperties[0] == null)
            {
                serverProperties[0] = "webroot incorrect";
                serverProperties[1] = string.Empty;
                serverProperties[2] = string.Empty;

                return serverProperties;
            }

            foreach (string s in args)
            {
                if (ipFormat.IsMatch(s))
                {
                    serverProperties[1] = s;
                    break;
                }
            }
            if (serverProperties[1] == null)
            {
                serverProperties[0] = "ip incorrect";
                serverProperties[1] = string.Empty;
                serverProperties[2] = string.Empty;

                return serverProperties;
            }

            foreach (string s in args)
            {
                if (Int32.TryParse(s, out int i))
                {
                    serverProperties[2] = s;
                    break;
                }
            }
            if (serverProperties[2] == null)
            {
                serverProperties[0] = "port incorrect";
                serverProperties[1] = string.Empty;
                serverProperties[2] = string.Empty;

                return serverProperties;
            }

            return serverProperties;
        }
    }
}
