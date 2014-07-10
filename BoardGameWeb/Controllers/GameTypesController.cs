using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Level14.BoardGameWeb.Controllers
{
    public class GameTypesController : BaseController
    {
        public IEnumerable<string> Get()
        {
            return System.IO.Directory.GetDirectories("./Games").Select(
                s => System.IO.Path.GetFileName(s));
        }
    }
}