using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Level14.BoardGameWeb.Models;
using System.Web.Http;

namespace Level14.BoardGameWeb.Controllers
{
    public class GamesController: BaseController
    {
        public IEnumerable<GameInfo.BasicInfo> Get([FromUri] bool? started = null)
        {
            RequireApiKey();
            IEnumerable<GameInfo> gameList = Games.List;
            if (started.HasValue)
            {
                gameList = gameList.Where(game => game.Started == started);
            }
            return gameList.Select(g => g.GetBasicInfo());
        }

        public GameInfo.BasicInfo Post([FromBody]dynamic body)
        {
            Session s = RequireApiKey();
            string type = body.type.Value;
            if (string.IsNullOrEmpty(type)) throw new Exception("Type cannot be empty");
            var game = Games.StartNew(s, type);
            return game.GetBasicInfo();
        }

        public GameInfo.DetailedInfo GetStatus([FromUri]string id)
        {
            RequireApiKey();
            Guid guid = Guid.Parse(id);
            var game = Games.List.First(g => g.Id == guid);
            return game.GetDetailedInfo(false);
        }
    }
}
