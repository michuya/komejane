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
  public class RouteNodeTests
  {
    [TestMethod()]
    public void ToStringTest()
    {
      RouteNode node = new RouteNode();

      // 子無しValue無し
      Assert.AreEqual<string>(node.ToString(), "<>");

      // 子無しValue有り
      node.AddValue("value1");
      Assert.AreEqual<string>(node.ToString(), "<value1>");
      node.AddValue("value2");
      Assert.AreEqual<string>(node.ToString(), "<value1, value2>");

      node = new RouteNode();

      node.CreateNode("GET");
      Assert.AreEqual<string>(node.ToString(), "[GET] => <>");
      node.Children["GET"].CreateNode("/");
      Assert.AreEqual<string>(node.ToString(), "[GET] => [/] => <>");
      node.Children["GET"].CreateNode("/users");
      Assert.AreEqual<string>(node.Children["GET"].ToString(), "[/] ======> <>\n[/users] => <>");
      Assert.AreEqual<string>(node.ToString(), "[GET] => [/] ======> <>\n         [/users] => <>");
    }
  }
}