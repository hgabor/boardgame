using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.Http.SelfHost;

namespace Level14.BoardGameWeb
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new HttpSelfHostConfiguration("http://localhost:8080");

            config.Routes.MapHttpRoute(
                name: "GameStatus",
                routeTemplate: "games/{id}",
                defaults: new { controller = "Games", action = "GetStatus" }
                );

            config.Routes.Add(
                name: "DefaultApi",
                route: new HttpRoute(
                    routeTemplate: "{controller}/{id}",
                    defaults: new HttpRouteValueDictionary { { "id", RouteParameter.Optional } }
                )
            );

            try
            {
                using (var server = new HttpSelfHostServer(config))
                {
                    server.OpenAsync().Wait();
                    Console.WriteLine("Press any key to stop the server");

                    char key;
                    do
                    {
                        Console.WriteLine(
@"Usage:
    s - list sessions
    q - quit"
                            );
                        key = char.ToLower(Console.ReadKey().KeyChar);
                        Console.WriteLine();

                        switch (key)
                        {
                            case 's':
                                foreach (var s in Models.Session.Sessions)
                                {
                                    Console.WriteLine("{0}: {1}", s.Key, s.Value.NickName);
                                }
                                break;
                        }

                    } while (key != 'q');
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}
