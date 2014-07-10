using Level14.BoardGameWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Level14.BoardGameWeb.Controllers
{
    public class BaseController : ApiController
    {
        protected Session RequireApiKey()
        {
            IEnumerable<string> apiKeys;
            ErrorMessage error;
            if (this.Request.Headers.TryGetValues("BoardApiKey", out apiKeys))
            {
                var key = apiKeys.FirstOrDefault();
                if (key != null)
                {
                    Session session;
                    if (Models.Session.TryGetByKey(key, out session))
                    {
                        return session;
                    }
                    else
                    {
                        error = ErrorMessage.InvalidApiKey;
                    }
                }
                else
                {
                    error = ErrorMessage.NoApiKey;
                }
                
            }
            else
            {
                error = ErrorMessage.NoApiKey;
            }
            var response = Request.CreateResponse(HttpStatusCode.Unauthorized, error);

            var header = new System.Net.Http.Headers.AuthenticationHeaderValue("BoardApiKeyAuth", "realm=\"BoardGameWeb\"");
            response.Headers.WwwAuthenticate.Add(header);

            throw new HttpResponseException(response);
        }
    }
}
