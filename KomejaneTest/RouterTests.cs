using Microsoft.VisualStudio.TestTools.UnitTesting;
using Komejane.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Komejane.Server.Tests
{
  [TestClass()]
  public class RouterTests
  {
    string[] routes =
    {
      "get / DefaultController",
      "get /api DefaultController.api()",
      "get /api/wsInfo WebSocketController.info()",
      "get /api/config ConfigController",
      "get /stream WebSocketController.stream()"
    };

    [TestMethod()]
    public void AddRouteTest()
    {
      Router router = new Router();

      router.AddRoute(routes[0]);

      Assert.AreEqual<string>(router.RootNode.ToString(), @"[GET] => [/] => <DefaultController>");
    }

    [TestMethod()]
    public void AddRoutesTest()
    {
      Router router = new Router();

      router.AddRoutes(routes);

      Assert.AreEqual<string>(router.RootNode.ToString(),
        @"
[GET] => [/] =======> <DefaultController>
         [/api] ====> [/wsInfo] => <WebSocketController, info>
                      [/config] => <ConfigController>
                      <DefaultController, api>
         [/stream] => <DefaultController, api>");
    }
}
}