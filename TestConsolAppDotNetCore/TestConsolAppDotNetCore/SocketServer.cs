using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class SocketServer
{
    private readonly string invtationTemplate = "Hi! Please enter number or command 'list'. I am waiting for you choise...";
    private readonly string readyTemplate = "\r\n> ";
    private readonly string badNumberMessage = "Number is not valid. Please enter valid number or command 'list'";

    private readonly object DbGetDataLock = new object();

    private TcpListener server = null;

    private Dictionary<string, int> clients = new Dictionary<string, int>();

    public SocketServer(string ip, int port)
    {
        IPAddress localAddr = IPAddress.Parse(ip);

        server = new TcpListener(localAddr, port);

        server.Start();
        StartListener();
    }

    public void StartListener()
    {
        try
        {
            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Client connected!");

                Thread t = new Thread(new ParameterizedThreadStart(HandleDeivce));
                t.Start(client);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            server.Stop();
        }
    }

    public void HandleDeivce(Object obj)
    {
        TcpClient client = (TcpClient)obj;
        var stream = client.GetStream();

        bool needToCloseConnection = false;
        string data = null;
        string sendMessage = string.Empty;
        Byte[] bytes = new Byte[32];
        int readBytes, totalBytes = 0;

        try
        {
            var value = 0;
            var remoteIp = client.Client.RemoteEndPoint.ToString();
            if (!clients.ContainsKey(remoteIp))
            {
                clients.Add(remoteIp, value);
                Byte[] reply = System.Text.Encoding.ASCII.GetBytes($"{invtationTemplate}{readyTemplate}");
                stream.Write(reply, 0, reply.Length);
            }
            else
                value = clients[remoteIp];

            while ((readBytes = stream.Read(bytes, totalBytes, bytes.Length - totalBytes)) != 0)
            {
                totalBytes += readBytes;
                if (bytes[totalBytes - 1] == 10 || bytes[totalBytes - 1] == 13)
                {
                    string hex = BitConverter.ToString(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, totalBytes);

                    totalBytes = 0;
                    if (data.Trim().ToLower().StartsWith("list"))
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (var d in clients)
                        {
                            sb.Append($"\r\n{d.Key} - {d.Value}");
                        }
                        sb.Append(readyTemplate);

                        sendMessage = sb.ToString();
                    }
                    else if (data.Trim().ToLower().StartsWith("exit"))
                    {
                        sendMessage = "Connection closed.";
                        needToCloseConnection = true;
                    }
                    else
                    {
                        int number;
                        if (int.TryParse(data, out number))
                        {
                            value += number;
                            clients[remoteIp] = value;

                            sendMessage = $"Sum: {value}{readyTemplate}";
                        }
                        else
                            sendMessage = $"{badNumberMessage}{readyTemplate}";
                    }
                    Byte[] reply = System.Text.Encoding.ASCII.GetBytes(sendMessage);
                    stream.Write(reply, 0, reply.Length);
                }
                if (needToCloseConnection)
                {
                    clients.Remove(remoteIp);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e.ToString());
        }
        finally
        {
            stream.Close();
            client.Close();
            client.Dispose();
        }
    }
}