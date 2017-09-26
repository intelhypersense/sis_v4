
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
// State object for reading client data asynchronously     
public class StateObject
{
    // Client socket.     
    public Socket workSocket = null;
    // Size of receive buffer.     
    public const int BufferSize = 1024;
    // Receive buffer.     
    public byte[] buffer = new byte[BufferSize];
    // Received data string.     
    public StringBuilder sb = new StringBuilder();

    
}
public class AsynchronousSocketListener
{
    public static Queue<string> newConnectIpLst = new Queue<string>();
    public static List<Socket> handleList = new List<Socket>();
    // Thread signal.     
    public static ManualResetEvent allDone = new ManualResetEvent(false);
    public static byte[] sendMsg = null;
    public AsynchronousSocketListener()
    {
    }
    public static void UDPsent() {
        UdpClient client1 = new UdpClient(new IPEndPoint(IPAddress.Any, 0));

        IPEndPoint endpoint1 = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 28925);
        if (sendMsg != null)
        {
            client1.Send(sendMsg, sendMsg.Length, endpoint1);
        }
    }
    public static void UDPRecvThread()
    {
        UdpClient client = new UdpClient(new IPEndPoint(IPAddress.Any, 8925));
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            byte[] buf = client.Receive(ref endpoint);
            byte[] bufRef = { 0x55, 0xBB, 0x4E, 0x4F, 0x41, 0x4C, 0x41, 0x42, 0x53, 0x00 };
            if (BytesEqual(buf, bufRef))
            {
                string msg = Encoding.Default.GetString(buf);
                Console.WriteLine(msg);
                UdpClient client1 = new UdpClient(new IPEndPoint(IPAddress.Any, 0));

                IPEndPoint endpoint1 = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 28925);
                if (sendMsg != null)
                {
                    client1.Send(sendMsg, sendMsg.Length, endpoint1);
                }
            }
        }
    }
    private static bool BytesEqual(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;
        for(int i = 0;i < a.Length;i++)
        {
            if (a[i] != b[i])
                return false;
        }
        return true;
    }
    public static byte[] iPmsgLoad(string ip, string port)
    {
        byte[] msg =
        {
            0x55, 0xBB, 0x00,0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
        try
        {
            
            byte[] temp =  System.BitConverter.GetBytes(int.Parse(port));
            msg[6] = temp[0];
            msg[7] = temp[1];
            string[] arr = ip.Split(new char[] {'.'});//   
            for(int i = 2; i < 6;i++)
            {
                byte[] temp1 = System.BitConverter.GetBytes(int.Parse(arr[i - 2]));
                msg[6-1-i+2] = temp1[0];
            }
        } catch (Exception)
        {

        }
        return msg;
    }
    public static void StartListening(string ip,string port)
    {
        // Data buffer for incoming data.     
        byte[] bytes = new Byte[1024];
        // Establish the local endpoint for the socket.     
        // The DNS name of the computer     
        // running the listener is "host.contoso.com".     
        sendMsg = iPmsgLoad(ip,port);
        UDPsent();
        IPAddress ipAddress = IPAddress.Parse(ip);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Int32.Parse(port));
        // Create a TCP/IP socket.     
        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // Bind the socket to the local     
        //endpoint and listen for incoming connections.     
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(100);
            while (true)
            {
                // Set the event to nonsignaled state.     
                allDone.Reset();
                // Start an asynchronous socket to listen for connections.     
                Console.WriteLine("Waiting for a connection...");
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                //handleList.Add(listener);
                // Wait until a connection is made before continuing.     
                allDone.WaitOne();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();
    }
    public static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.     
        allDone.Set();
        // Get the socket that handles the client request.     
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);
        handleList.Add(handler);
        newConnectIpLst.Enqueue(handler.RemoteEndPoint.ToString());
        // Create the state object.     
        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
    }
    public static void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;
        // Retrieve the state object and the handler socket     
        // from the asynchronous state object.     
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;
        //handleList.Add(handler);
        // Read data from the client socket.     
        try {
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.     
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                // Check for end-of-file tag. If it is not there, read     
                // more data.     
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the     
                    // client. Display it on the console.     
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
                    // Echo the data back to the client.     
                    Send(handler, content);
                }
                else
                {
                    // Not all data received. Get more.     
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
        } catch(Exception ex)
        {
        }
    }
    public static void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.     
        byte[] byteData = Encoding.ASCII.GetBytes(data);
        // Begin sending the data to the remote device.     
        handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
    }
    private static void SendCallback(IAsyncResult ar)
    {
       try
        {
            // Retrieve the socket from the state object.     
            Socket handler = (Socket)ar.AsyncState;
            // Complete sending the data to the remote device.     
            int bytesSent = handler.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    public static string GetLocalIp()
    {
        string hostname = Dns.GetHostName();//Get the host name   
        IPHostEntry localhost = Dns.GetHostEntry(hostname);
        IPAddress localaddr = null;
        List<string> addressString = new List<string>();
        for (int i = 0;i < localhost.AddressList.Length;i++)
        {
            if(localhost.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
            {
                localaddr = localhost.AddressList[i];
                addressString.Add(localaddr.ToString());
            }
        }
        for(int i = 0;i < addressString.Count; i++)
        {
            if (addressString[i].IndexOf("192.168.42.") == 0)
            {
                return addressString[i];
            }
        }
        for (int i = 0; i < addressString.Count; i++)
        {
            if (addressString[i].IndexOf("192.168.") == 0)
            {
                return addressString[i];
            }
        }

        return addressString[0];
    }
}
