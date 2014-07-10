using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Level14.BoardGameWeb.Models
{
    /// <summary>
    /// Holds the contents of a user session
    /// </summary>
    public class Session
    {
        public string NickName { get; set; }
        public string Key { get; set; }
        public int SocketPort { get; set; }

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
            {
                Key = Guid.NewGuid().ToString(),
                NickName = nick,
                SocketPort = 65001
            };
            return Sessions.TryAdd(session.Key, session);
        }

        private static object sessionLock = new object();
        private static ConcurrentDictionary<string, Session> sessions = new ConcurrentDictionary<string, Session>();
        private static ConcurrentDictionary<string, Session> Sessions {
            get
            {
                return sessions;
            }
        }
    }
}