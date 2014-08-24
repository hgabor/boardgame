using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Web;

namespace Level14.BoardGameWeb.Models
{
    /// <summary>
    /// Holds the contents of a user session
    /// </summary>
    public class Session : IDisposable
    {
        private readonly string nickName;
        public string NickName { get { return nickName; } }
        private readonly string key;
        public string Key { get { return key; } }

        private TcpListener listener; // Only ListeningTask can use this object!
        public int SocketPort { get; private set; }

        private Session(string nickName, string key)
        {
            this.nickName = nickName;
            this.key = key;
        }

        private Thread socketThread;
        private void StartListening()
        {
            listener = new System.Net.Sockets.TcpListener(IPAddress.Any, 0);
            listener.Start();
            SocketPort = ((IPEndPoint)listener.LocalEndpoint).Port;

            socketThread = new Thread(ListeningTask);
            socketThread.Start();
        }

        private enum Signal : byte
        {
            Invalid = 0,
            Ping = 1,
            Pong = 2,
            StartGame = 10,
            ErrorInvalidKey = 100,
        }

        private ConcurrentQueue<Signal> signalQueue = new ConcurrentQueue<Signal>();
        private void ListeningTask()
        {
            try
            {
                var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
                var buffer = new byte[keyBytes.Length];

                Console.WriteLine("Waiting for incoming connections");
                while (true)
                {
                    while (!listener.Pending())
                    {
                        Thread.Sleep(500);
                    }
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Client connected!");

                    // Wait for introduction...
                    var stream = client.GetStream();
                    var readBytes = stream.Read(buffer, 0, buffer.Length);
                    if (readBytes != buffer.Length)
                    {
                        // Didn't send complete API key
                        stream.WriteByte((byte)Signal.ErrorInvalidKey);
                        client.Close();
                        continue;
                    }

                    for (int i = 0; i < buffer.Length; i++)
                    {
                        if (buffer[i] != keyBytes[i])
                        {
                            // Invalid API key
                            stream.WriteByte((byte)Signal.ErrorInvalidKey);
                            client.Close();
                            continue;
                        }
                    }

                    // API key is correct. Wait for notifications...

                    while (true)
                    {
                        // Send a ping, the only way to detect the true state of the connection
                        // Check the status every 10 seconds, implemented below
                        stream.WriteByte((byte)Signal.Ping);
                        int pong = stream.ReadByte();
                        if (pong != (byte)Signal.Pong)
                        {
                            // Disconnected or invalid data
                            client.Close();
                            break; // from the inner loop, we should continue listening in case the user reconnects
                        }

                        for (int i = 0; i < 10; i++)
                        {
                            // Read a signal every second
                            Signal s;
                            if (signalQueue.TryDequeue(out s))
                            {
                                switch (s)
                                {
                                    case Signal.StartGame:
                                        break;
                                    default:
                                        Console.WriteLine("Missing handler for signal '{0}' in Session.ListeningTash", s);
                                        break;
                                }
                            }
                            // Wait for 1 sec, loop 10 times = 10 seconds of waiting
                            Thread.Sleep(1000);
                        }
                    }
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        public void SignalGameStart(GameInfo gameInfo)
        {
            // FIXME: Noop for now
            // System.Diagnostics.Debugger.Break();
        }

        /// <summary>
        /// Gets the session data specified by the API key
        /// </summary>
        /// <param name="key">The API key</param>
        /// <param name="value">The session value</param>
        /// <returns>The API key was found</returns>
        public static bool TryGetByKey(string key, out Session value)
        {
            return Sessions.TryGetValue(key, out value);
        }

        /// <summary>
        /// Creates a new session
        /// </summary>
        /// <param name="nick">The nickname of the player</param>
        /// <param name="session">The session data</param>
        /// <returns>Everything went OK</returns>
        public static bool TryAddNew(string nick, out Session session)
        {
            session = new Session
            (
                key: Guid.NewGuid().ToString(),
                nickName: nick
            );
            bool success = Sessions.TryAdd(session.Key, session);
            if (success)
            {
                session.StartListening();
                return true;
            }
            else
            {
                return false;
            }
        }

        private static object sessionLock = new object();
        private static ConcurrentDictionary<string, Session> sessions = new ConcurrentDictionary<string, Session>();
        internal static ConcurrentDictionary<string, Session> Sessions
        {
            get
            {
                return sessions;
            }
        }

        public void Dispose()
        {
            socketThread.Abort();
        }

        public static void DisposeAll()
        {
            foreach (var s in sessions)
            {
                s.Value.Dispose();
            }
        }
    }
}
