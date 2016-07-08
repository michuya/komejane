using Microsoft.VisualStudio.TestTools.UnitTesting;
using Komejane.Server.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komejane.Server.Controller.Tests
{
  [TestClass()]
  public class ControllerWrapperTests
  {
    [TestMethod()]
    public void ControllerWrapperTest()
    {
      // 引数付きだけテスト
      string root = System.IO.Path.Combine(Config.Instance.DllDirectory, Config.Instance.WebRootDirectory);
      ControllerWrapper wrapper = new ControllerWrapper("DefaultController",
        constructorArgs: new string[] { root, Config.Instance.WebIndex },
        @namespace: "Komejane.Server.Controller");
    }
  }
}