using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;
using System.Xml.Schema;

namespace WDD_A05
{
    internal class WebServer
    {

        private Int32 port;
        private IPAddress ip;
        private string webRoot;
        private TcpListener webServer;

        public WebServer(string webRoot, string ip, string port)
        {
            this.webRoot = webRoot;

            this.ip = IPAddress.Parse(ip);

            Int32.TryParse(port, out int i);

            this.port = i;

            webServer = null;
        }

        public void StartWebServer()
        {
            try
            {
                webServer = new TcpListener(ip, port);

                webServer.Start();

                while (true)
                {
                    TcpClient webClient = webServer.AcceptTcpClient();

                    Task.Run(() => HandleHttpRequest(webClient));
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                webServer?.Stop();
            }
        }

        private void HandleHttpRequest(TcpClient client)
        {
            byte[] clientRequestData = new byte[500];
            string clientRequest = string.Empty;

            byte[] serverMessageData = new byte[50000];
            string serverMessage = string.Empty;    

            NetworkStream stream = client.GetStream();

            stream.Read(clientRequestData, 0, clientRequestData.Length);
            clientRequest = Encoding.ASCII.GetString(clientRequestData).Trim('\0');

            string[] requestByLines = clientRequest.Split(new char[] { '\r', '\n' });
            string[] copy = requestByLines;
            if (requestByLines.Length < 2)
            {
                BadHttpRequest(client, stream);

                return;
            }
            else
            {
                int j = 0;

                for (int i = 0; i < copy.Length; i++)
                {
                    if (copy[i] != string.Empty)
                    {
                        requestByLines[j] = copy[i];
                        j++;
                    }
                }
            }

            string[] firstLineBySpaces = requestByLines[0].Split(' ');
            if (firstLineBySpaces.Length != 3)
            {
                BadHttpRequest(client, stream);

                return;
            }

            string host = requestByLines[1];
            if (host != "HOST: " + ip + ":" + port && host != "Host: " + ip + ":" + port)
            {
                BadHttpRequest(client, stream);

                return;
            }

            string method = firstLineBySpaces[0];
            string path = webRoot + firstLineBySpaces[1];
            string httpProtocol = firstLineBySpaces[2];

            if (method != "GET")
            {
                MethodNotAllowed(client, stream);

                return;
            }

            if (!File.Exists(path))
            {
                FileNotFound(client, stream);

                return;
            }

            string fileContents = File.ReadAllText(path);

            serverMessage = "HTTP/1.1 200 OK\r\n" + "Content-Type: text/html\r\n" + "Content-Length: " + Encoding.ASCII.GetByteCount(fileContents).ToString() + "\r\n" + "Connection: close\r\n\r\n" + fileContents;

            serverMessageData = Encoding.ASCII.GetBytes(serverMessage);

            stream.Write(serverMessageData, 0, serverMessageData.Length);

            client.Close();
        }

        private void FileNotFound(TcpClient client, NetworkStream stream)
        {
            byte[] serverMessageData = new byte[500];
            string serverMessage = string.Empty;

            serverMessage = "404 Error: File not found";
            serverMessageData = Encoding.ASCII.GetBytes(serverMessage);

            stream.Write(serverMessageData, 0, serverMessageData.Length);

            client.Close();
        }

        private void MethodNotAllowed(TcpClient client, NetworkStream stream)
        {
            byte[] serverMessageData = new byte[500];
            string serverMessage = string.Empty;

            serverMessage = "405 Error: Method not allowed";
            serverMessageData = Encoding.ASCII.GetBytes(serverMessage);

            stream.Write(serverMessageData, 0, serverMessageData.Length);

            client.Close();
        }

        private void BadHttpRequest(TcpClient client, NetworkStream stream)
        {
            byte[] serverMessageData = new byte[500];
            string serverMessage = string.Empty;

            serverMessage = "400 Error: Bad Request";
            serverMessageData = Encoding.ASCII.GetBytes(serverMessage);

            stream.Write(serverMessageData, 0, serverMessageData.Length);

            client.Close();
        }
    }
}
