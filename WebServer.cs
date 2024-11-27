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

                DateTime dateTime = DateTime.Now;
                string log = dateTime.ToString("ddd, dd MMM yyyy HH:mm:ss 'EST'", System.Globalization.CultureInfo.InvariantCulture) + " [SERVER STARTED] - " + webRoot + ", " + ip.ToString() + ", " + port.ToString() + "\n\n";

                File.WriteAllText("myOwnWebServer.txt", log);

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
            DateTime dateTime = DateTime.Now;

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
            if (!host.Contains(ip.ToString()))
            {
                BadHttpRequest(client, stream);

                return;
            }

            string method = firstLineBySpaces[0];
            string path = webRoot + firstLineBySpaces[1];
            string httpProtocol = firstLineBySpaces[2];

            string log = dateTime.ToString("ddd, dd MMM yyyy HH:mm:ss 'EST'", System.Globalization.CultureInfo.InvariantCulture) + " [REQUEST RECEIVED] - " + method + ", " + firstLineBySpaces[1] + "\n\n";

            File.AppendAllText("myOwnWebServer.txt", log);

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

            string dateTimeStamp = "Date: " + dateTime.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'", System.Globalization.CultureInfo.InvariantCulture);
            string headerMessage = string.Empty;
            string protocol = "HTTP/1.1 200 OK";
            string contentType = string.Empty;
            string acceptRanges = string.Empty;
            string contentLength = string.Empty;
            string server = "Server: myOwnWebServer/1.0";
            string connection = "Connection: close";

            if (fileType == 1 || fileType == 2)
            {
                contentLength = "Content-Length: " + Encoding.ASCII.GetByteCount(fileContents).ToString();

                if (fileType == 1)
                {
                    contentType = "Content-Type: text/html; charset=UTF-8";
                }
                else if (fileType == 2)
                {
                    contentType = "Content-Type: text/plain; charset=UTF-8";
                }

                headerMessage = protocol + "\r\n" + contentType + "\r\n" + server + "\r\n" + dateTimeStamp + "\r\n" + contentLength + "\r\n" + connection + "\r\n\r\n" + fileContents;

                header = Encoding.ASCII.GetBytes(headerMessage);

                stream.Write(header, 0, header.Length);

                DateTime logTime = DateTime.Now;

                string log = logTime.ToString("ddd, dd MMM yyyy HH:mm:ss 'EST'", System.Globalization.CultureInfo.InvariantCulture) + " [RESPONSE SENT] - " + contentType + ", " + contentLength + ", " + server + ", " + dateTimeStamp + "\n\n";

                File.AppendAllText("myOwnWebServer.txt", log);

                return;
            }

            else if (fileType == 3 || fileType == 4)
            {
                contentLength = "Content-Length: " + image.Length.ToString();

                acceptRanges = "Accept-Ranges: bytes";

                if (fileType == 3)
                {
                    contentType = "Content-Type: image/gif";
                }
                else if (fileType == 4)
                {
                    contentType = "Content-Type: image/jpeg";
                }

                headerMessage = protocol + "\r\n" + contentType + "\r\n" + acceptRanges + "\r\n" + server + "\r\n" + dateTimeStamp + "\r\n" + contentLength + "\r\n" + connection + "\r\n\r\n";

                header = Encoding.ASCII.GetBytes(headerMessage);

                stream.Write(header, 0, header.Length);

                stream.Write(image, 0, image.Length);

                DateTime logTime = DateTime.Now;

                string log = logTime.ToString("ddd, dd MMM yyyy HH:mm:ss 'EST'", System.Globalization.CultureInfo.InvariantCulture) + " [RESPONSE SENT] - " + contentType + ", " + contentLength + ", " + server + ", " + dateTimeStamp + "\n\n";

                File.AppendAllText("myOwnWebServer.txt", log);

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

            DateTime dateTime = DateTime.Now;

            string log = dateTime.ToString("ddd, dd MMM yyyy HH:mm:ss 'EST'", System.Globalization.CultureInfo.InvariantCulture) + " [RESPONSE ERROR] - 404\n\n";

            File.AppendAllText("myOwnWebServer.txt", log);

            client.Close();
        }

        private void MethodNotAllowed(TcpClient client, NetworkStream stream)
        {
            byte[] serverMessageData = new byte[500];
            string serverMessage = string.Empty;

            serverMessage = "HTTP/1.1 405 Method Not Allowed\r\n";
            serverMessageData = Encoding.ASCII.GetBytes(serverMessage);

            stream.Write(serverMessageData, 0, serverMessageData.Length);

            DateTime dateTime = DateTime.Now;

            string log = dateTime.ToString("ddd, dd MMM yyyy HH:mm:ss 'EST'", System.Globalization.CultureInfo.InvariantCulture) + " [RESPONSE ERROR] - 405\n\n";

            File.AppendAllText("myOwnWebServer.txt", log);

            client.Close();
        }

        private void BadHttpRequest(TcpClient client, NetworkStream stream)
        {
            byte[] serverMessageData = new byte[500];
            string serverMessage = string.Empty;

            serverMessage = "HTTP/1.1 400 Bad Request\r\n";
            serverMessageData = Encoding.ASCII.GetBytes(serverMessage);

            stream.Write(serverMessageData, 0, serverMessageData.Length);

            DateTime dateTime = DateTime.Now;

            string log = dateTime.ToString("ddd, dd MMM yyyy HH:mm:ss 'EST'", System.Globalization.CultureInfo.InvariantCulture) + " [RESPONSE ERROR] - 400\n\n";

            File.AppendAllText("myOwnWebServer.txt", log);

            client.Close();
        }
    }
}
