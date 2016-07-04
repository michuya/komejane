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

      router = new Router();

      router.AddRoute(routes[1]);
      Assert.AreEqual<string>(router.RootNode.ToString(), @"[GET] => [/] => [api] => <DefaultController, api>");

      router = new Router();

      router.AddRoute(routes[2]);
      Assert.AreEqual<string>(router.RootNode.ToString(), @"[GET] => [/] => [api/] => [wsInfo] => <WebSocketController, info>");
    }

    [TestMethod()]
    public void AddRoutesTest()
    {
      Router router = new Router();

      router.AddRoutes(routes);

      Assert.AreEqual<string>(router.RootNode.ToString(), string.Join("\n", new string[] {
        "[GET] => [/] => [api] ====> <DefaultController, api>",
        "                [api/] ===> [wsInfo] => <WebSocketController, info>",
        "                            [config] => <ConfigController>",
        "                [stream] => <WebSocketController, stream>",
        "                <DefaultController>",
      }));
    }

    [TestMethod()]
    public void RoutingTest()
    {
      Assert.Fail();
    }

    [TestMethod()]
    public void ControllerParserTest()
    {
      Dictionary<string, RouteControllerInfo> testDatas = new Dictionary<string, RouteControllerInfo>()
      {
        { "DefaultController", new RouteControllerInfo("DefaultController", null) },
        { "DefaultController.index", new RouteControllerInfo("DefaultController", null, "index") },
        { "DefaultController.index()", new RouteControllerInfo("DefaultController", null, "index") },
        { "DefaultController()", new RouteControllerInfo("DefaultController", null) },
        { "DefaultController().index", new RouteControllerInfo("DefaultController", null, "index") },
        { "DefaultController().index()", new RouteControllerInfo("DefaultController", null, "index") },
        { "DefaultController(\"a\", \"b\")", new RouteControllerInfo("DefaultController", new string[] { "a", "b" }) },
        { "DefaultController(\"a\", \"b\").index", new RouteControllerInfo("DefaultController", new string[] { "a", "b" }, "index") },
        { "DefaultController(\"a\", \"b\").index()", new RouteControllerInfo("DefaultController", new string[] { "a", "b" }, "index") },
        { "DefaultController(\"c\")", new RouteControllerInfo("DefaultController", new string[] { "c" }) },
        { "DefaultController(\"c\").index", new RouteControllerInfo("DefaultController", new string[] { "c" }, "index") },
        { "DefaultController(\"c\").index()", new RouteControllerInfo("DefaultController", new string[] { "c" }, "index") }
      };

      foreach (var pair in testDatas)
      {
        var ctrl = Router.ControllerParser(pair.Key);
        Assert.AreEqual(ctrl.Name, pair.Value.Name);
        Assert.AreEqual(ctrl.Method, pair.Value.Method);
        CollectionAssert.AreEqual(ctrl.Options, pair.Value.Options);
      }
    }
  }
}