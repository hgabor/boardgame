using Level14.BoardGameWeb.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Level14.BoardGameWeb.Controllers
{
    public class SessionController : BaseController
    {
        public Session Get()
        {
            Session s = RequireApiKey();
            return s;
        }

        public Session Post([FromBody]dynamic body)
        {
            string nick = body.nick.Value;
            Session session;
            if (!Session.TryAddNew(nick, out session))
            {
                return null;
            }
            return session;
        }
    }
}