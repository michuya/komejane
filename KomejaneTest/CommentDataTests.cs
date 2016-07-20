using Microsoft.VisualStudio.TestTools.UnitTesting;
using Komejane;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komejane.Tests
{
  [TestClass()]
  public class CommentDataTests
  {
    [TestMethod()]
    public void ToJsonTest()
    {
      CommentData cd = new CommentData(
        7,
        "メッセージテスト",
        CommentData.UserType.Normal,
        "userid***",
        null,
        true,
        0
      );

      Assert.AreEqual(cd.ToJson(), "{\"CharaName\":null,\"Comment\":\"メッセージテスト\",\"CommentNo\":7,\"IsAnonymous\":true,\"Name\":\"名無し\",\"Premium\":0,\"Type\":4,\"UserId\":\"userid***\"}");
    }
  }
}