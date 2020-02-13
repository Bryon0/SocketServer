using System;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace ConsoleApp1
{
    #region Defitions
    public enum ConsoleEvent
    {
        CTRL_C = 0,
        CTRL_BREAK = 1,
        CTRL_CLOSE = 2,
        CTRL_LOGOFF = 5,
        CTRL_SHUTDOWN = 6
    }
    internal class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    internal struct Request
    {
        public string strConnection;
        public string strProvider;
        public string strSqlStatement;
        public string strMsgType;
        public string strPassword;
        public string strUserName;

        public string strCommand;
    }

    internal struct Response
    {
        public string strErrorMsg;
        public string strQueryReply;
        public int nErrorCode;
        public List<string> ListQueruResults;
    }
    #endregion

    #region Server
    internal class AsynchronousSocketServer
    {
        private Socket m_Listener;
        private int m_port;
        private Boolean m_bListening = false;
        private IPAddress m_ipAddress;
        private IPAddress m_macAddress;
        private IPEndPoint m_localEndPoint;
        // Thread signal.  
        public ManualResetEvent allDone = new ManualResetEvent(false);
        public string strConent = "";

        public class OnMessagereceivedEventArgs : EventArgs
        {
            public Socket socket { get; set; }
            public string content { get; set; }
            public AsynchronousSocketServer server { get; set; }
        }

        protected virtual void OnMessageReceived(OnMessagereceivedEventArgs e)
        {
            EventHandler<OnMessagereceivedEventArgs> handler = MessageReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<OnMessagereceivedEventArgs> MessageReceived;

        public AsynchronousSocketServer(string hostName, int port)
        {
            this.m_port = port;
            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPHostEntry ipHostInfo = Dns.GetHostEntry(hostName);
            m_macAddress = ipHostInfo.AddressList[0];
            m_ipAddress = ipHostInfo.AddressList[ipHostInfo.AddressList.Length - 1];
            m_localEndPoint = new IPEndPoint(m_ipAddress, m_port);
        }

        public string ServerIPAddress
        {
            get { return m_ipAddress.ToString(); }
        }

        public string ServerMacAddress
        {
            get { return m_macAddress.ToString(); }
        }

        public string ServerPort
        {
            get { return Convert.ToString(m_port); }
        }

        public void Dispose()
        {
            m_Listener.Shutdown(SocketShutdown.Both);
            m_Listener.Close();
        }

        public void StartListening()
        {
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // 

            // Create a TCP/IP socket.  
            m_Listener = new Socket(m_ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine($"Server started at {m_ipAddress} on port {Convert.ToString(m_port)}.");

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                m_bListening = true;
                m_Listener.Bind(m_localEndPoint);
                m_Listener.Listen(100);

                while (m_bListening)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    m_Listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        m_Listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

                //m_Listener.Shutdown(SocketShutdown.Both);
                // m_Listener.Close();
            }
            catch (ObjectDisposedException)
            {
                StopListening();
                Console.WriteLine("Listener closed.");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.   
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read   
                // more data.  
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    //EventArgs args = new EventArgs();
                    OnMessagereceivedEventArgs args = new OnMessagereceivedEventArgs();
                    args.socket = handler;
                    args.content = content.Remove(content.IndexOf("<EOF>"));

                    OnMessageReceived(args);
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        public void SendMessage(Socket s, string str)
        {
            this.Send(s, str);
        }

        private void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
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

        public void StopListening()
        {
            //m_Listener.Shutdown(SocketShutdown.Both);
            //m_Listener.Close();
            m_bListening = false;
            allDone.Set();
        }
    }
    #endregion

    internal class Program
    {
        private static Mutex mutex = null;
        private static Thread thr;
        private static AsynchronousSocketServer server;
        // The console event handler...
        public delegate void ControlEventHandler(ConsoleEvent consoleEvent);

        // The imported API method...
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ControlEventHandler e, bool add);

        // Control Event Handler
        public static void OnControlEvent(ConsoleEvent consoleEvent)
        {
            Console.WriteLine("Event: {0}", consoleEvent);
            if(consoleEvent == ConsoleEvent.CTRL_CLOSE || consoleEvent == ConsoleEvent.CTRL_LOGOFF || consoleEvent == ConsoleEvent.CTRL_SHUTDOWN)
            {
                server.StopListening();
                thr.Abort();
            }
        }

        #region EventHandler
        private static void c_OnMessageReceived(Object sender, EventArgs e)
        {
            try
            {
                AsynchronousSocketServer.OnMessagereceivedEventArgs args = e as AsynchronousSocketServer.OnMessagereceivedEventArgs;
                int nErrorCode = 0;
                string strErrorMessage = "";
                string strRec = "";
                string serialized = "";
                string strReply = "";
                Response response = new Response();
                Dictionary<int, Dictionary<int, string>> d = new Dictionary<int, Dictionary<int, string>>();
                Request request = JsonConvert.DeserializeObject<Request>(args.content);

                //AppendTextBox("Message received: " + Environment.NewLine);
                Console.WriteLine($"Message received: {args.content} " + Environment.NewLine);

                //CRUD
                if (request.strMsgType == "R")
                {
                    response.ListQueruResults = new List<string>();
                    strReply = "Read Record(s)";
                    nErrorCode = Database.odbc_read(strErrorMessage, request.strSqlStatement, request.strProvider, request.strConnection, 60, ref d);
                    if (nErrorCode > 0)
                    {
                        foreach (KeyValuePair<int, Dictionary<int, string>> rec in d)
                        {
                            for (int i = 0; i < rec.Value.Count; i++)
                            {
                                strRec += rec.Value[i];
                                if (i < rec.Value.Count - 1)
                                {
                                    strRec += ",";
                                }
                            }
                            strRec += Environment.NewLine;
                            response.ListQueruResults.Add(strRec);
                        }
                    }
                    else
                    {

                    }
                }
                else if (request.strMsgType == "C")
                {
                    strReply = "Create Record(s)";
                    nErrorCode = Database.odbc_write(strErrorMessage, request.strSqlStatement, request.strProvider, request.strConnection, 60);
                    if (nErrorCode > 0)
                    {

                    }
                }
                else if (request.strMsgType == "U")
                {
                    strReply = "Update Record(s)";
                    nErrorCode = Database.odbc_write(strErrorMessage, request.strSqlStatement, request.strProvider, request.strConnection, 60);
                    if (nErrorCode > 0)
                    {

                    }
                }
                else if (request.strMsgType == "D")
                {
                    strReply = "Delete Record(s)";
                    nErrorCode = Database.odbc_write("", request.strSqlStatement, request.strProvider, request.strConnection, 60);
                    if (nErrorCode > 0)
                    {

                    }
                }
                else if (request.strMsgType == "CMD")
                {
                    if (request.strCommand == "SHUTDOWN")
                    {
                        server.StopListening();

                    }
                    else if (request.strCommand == "STARTUP")
                    {
                        server.StartListening(); 
                    }
                }
                else
                {
                    strReply = "Unknown Command";
                    return;
                }

                response.nErrorCode = nErrorCode;
                response.strErrorMsg = strErrorMessage;
                response.strQueryReply = strReply;
                serialized = JsonConvert.SerializeObject(response);
                server.SendMessage(args.socket, serialized);
            }
            catch (Exception ex)
            {
                // AppendTextBox("EXCEPTION: " + ex.Message + Environment.NewLine);
                Console.WriteLine("EXCEPTION: " + ex.Message + Environment.NewLine);
            }
        }
        #endregion

        private static void Main(string[] args)
        {
            try
            {
                const string appName = "Server";
                bool createdNew;
                const int PORT = 11000;
                int port = PORT;
                
                if (args.Length > 0)
                {
                    if(!int.TryParse(args[0], out port))
                    {
                        port = PORT;
                    }
                }

                mutex = new Mutex(true, appName, out createdNew);

                if (!createdNew)
                {
                    Console.WriteLine("Only one instance is allowed! Exiting the application.");
                    Console.ReadKey();
                    return;
                }


                SetConsoleCtrlHandler(new ControlEventHandler(OnControlEvent), true);

                server = new AsynchronousSocketServer(Dns.GetHostName(), port);
                server.MessageReceived += Program.c_OnMessageReceived;

                thr = new Thread(() => { server.StartListening(); });
                thr.Start();

            }
            catch
            {

            }
        }
    }
}
