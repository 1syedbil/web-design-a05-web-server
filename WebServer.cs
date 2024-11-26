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
using System.Security.Cryptography;

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

            int fileType = CheckFileType(path);

            string fileContents = File.ReadAllText(path);

            byte[] image = File.ReadAllBytes(path);

            switch (fileType)
            {
                case 1:
                    SendOkResponse(client, stream, fileContents, serverMessageData, image, fileType);

                    break;

                case 2:
                    SendOkResponse(client, stream, fileContents, serverMessageData, image, fileType);

                    break;

                case 3:
                    SendOkResponse(client, stream, fileContents, serverMessageData, image, fileType);

                    break;

                case 4:
                    SendOkResponse(client, stream, fileContents, serverMessageData, image, fileType);

                    break;

                default:
                    break;
            }

            client.Close();
        }

        private void SendOkResponse(TcpClient client, NetworkStream stream, string fileContents, byte[] header, byte[] image, int fileType)
        {
            DateTime dateTime = DateTime.UtcNow;

            string dateTimeStamp = "Date: " + dateTime.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'", System.Globalization.CultureInfo.InvariantCulture) + "\r\n";
            string headerMessage = string.Empty;
            string protocol = "HTTP/1.1 200 OK\r\n";
            string contentType = string.Empty;
            string acceptRanges = string.Empty;
            string contentLength = string.Empty;
            string server = "Server: Bilal-Syed-WDD_A05/1.0\r\n";
            string connection = "Connection: close\r\n\r\n";

            if (fileType == 1 || fileType == 2)
            {
                contentLength = "Content-Length: " + Encoding.ASCII.GetByteCount(fileContents).ToString() + "\r\n";

                if (fileType == 1)
                {
                    contentType = "Content-Type: text/html; charset=UTF-8\r\n";
                }
                else if (fileType == 2)
                {
                    contentType = "Content-Type: text/plain; charset=UTF-8\r\n";
                }

                headerMessage = protocol + contentType + server + dateTimeStamp + contentLength + connection + fileContents;

                header = Encoding.ASCII.GetBytes(headerMessage);

                stream.Write(header, 0, header.Length);

                return;
            }

            else if (fileType == 3 || fileType == 4)
            {
                contentLength = "Content-Length: " + image.Length.ToString() + "\r\n";

                acceptRanges = "Accept-Ranges: bytes\r\n";

                if (fileType == 3)
                {
                    contentType = "Content-Type: image/gif\r\n";
                }
                else if (fileType == 4)
                {
                    contentType = "Content-Type: image/jpeg\r\n";
                }

                headerMessage = protocol + contentType + acceptRanges + server + dateTimeStamp + contentLength + connection;

                header = Encoding.ASCII.GetBytes(headerMessage);

                stream.Write(header, 0, header.Length);

                stream.Write(image, 0, image.Length);

                return;
            }

        }

        private int CheckFileType(string filePath)
        {
            string extension = Path.GetExtension(filePath);

            if (extension == ".html" || extension == ".htm" || extension == ".xhtml" || extension == ".asp" || extension == ".php")
            {
                return 1;
            }
            else if (extension == ".txt")
            {
                return 2;
            }
            else if (extension == ".gif")
            {
                return 3;
            }
            else if (extension == ".jpg" || extension == ".jpeg" || extension == ".jpe")
            {
                return 4;
            }

            return 0;
        }

        private void FileNotFound(TcpClient client, NetworkStream stream)
        {
            byte[] serverMessageData = new byte[500];
            string serverMessage = string.Empty;

            serverMessage = "HTTP/1.1 404 Not Found\r\n";
            serverMessageData = Encoding.ASCII.GetBytes(serverMessage);

            stream.Write(serverMessageData, 0, serverMessageData.Length);

            client.Close();
        }

        private void MethodNotAllowed(TcpClient client, NetworkStream stream)
        {
            byte[] serverMessageData = new byte[500];
            string serverMessage = string.Empty;

            serverMessage = "HTTP/1.1 405 Method Not Allowed\r\n";
            serverMessageData = Encoding.ASCII.GetBytes(serverMessage);

            stream.Write(serverMessageData, 0, serverMessageData.Length);

            client.Close();
        }

        private void BadHttpRequest(TcpClient client, NetworkStream stream)
        {
            byte[] serverMessageData = new byte[500];
            string serverMessage = string.Empty;

            serverMessage = "HTTP/1.1 400 Bad Request\r\n";
            serverMessageData = Encoding.ASCII.GetBytes(serverMessage);

            stream.Write(serverMessageData, 0, serverMessageData.Length);

            client.Close();
        }
    }
}
