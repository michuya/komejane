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
    public void SplitURITest()
    {
      Dictionary<string, string[]> urls = new Dictionary<string, string[]>() {
        { "/", new string[] { "/" } },
        { "/api", new string[] { "/api" } },
        { "/api/", new string[] { "/api" } },
        { "/_/./.html", new string[] { "/_", "/.", "/.html" } },
        { "/index.html", new string[] { "/", "index.html" } },
        { "/a/b/c/d/e/f/g/", new string[] { "/a", "/b", "/c", "/d", "/e", "/f", "/g" } },
      };

      foreach (var pair in urls)
      {
        var splited = Router.SplitURIPath(pair.Key);
        try
        {
          CollectionAssert.AreEqual(splited, pair.Value);
        }
        catch (AssertFailedException ex)
        {
          System.Diagnostics.Debug.WriteLine("splited = \"" + string.Join("\", \"", splited) + "\"");
          System.Diagnostics.Debug.WriteLine("sample = \"" + string.Join("\", \"", pair.Value) + "\"");
          throw ex;
        }
      }
    }

    [TestMethod()]
    public void RoutingTest()
    {
      Assert.Fail();
    }
  }
}