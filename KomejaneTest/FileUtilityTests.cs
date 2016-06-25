using Microsoft.VisualStudio.TestTools.UnitTesting;
using Komejane;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Komejane.Tests
{
  [TestClass()]
  public class FileUtilityTests
  {
    [TestMethod()]
    public async Task ResourceWriterTest()
    {
      // 存在チェックの対象リソース
      string rcNamespace = "Komejane.Resource.web";
      string[] webResources = {
        "index.html",
        "komejane.js",
        "style.css"
      };

      // リソース一覧の取得
      string[] resources = FileUtility.GetResources();

      // リソースの存在チェック
      foreach (string rcname in webResources)
        Assert.IsTrue(resources.Contains(string.Format("{0}.{1}", rcNamespace, rcname)));

      // リソース書き出し先を取得
      string outputDirectory = Path.GetFullPath(".temp");
      var fullAsmName = this.GetType().Assembly.Location;
      var asmName = Path.GetFileNameWithoutExtension(fullAsmName);
      System.Diagnostics.Debug.WriteLine("asm = [" + asmName + "]:" + fullAsmName);
      System.Diagnostics.Debug.WriteLine("test output path = " + outputDirectory);
      if (!outputDirectory.Contains(asmName)) Assert.Fail(); // フルパスにアセンブリ名入ってなかったらテストを止める

      // リソースの書き出し前に出力先を削除
      if (Directory.Exists(outputDirectory))
        Directory.Delete(outputDirectory, true);

      // リソースの書き出しと比較を行う
      foreach (string rcname in webResources)
      {
        await FileUtility.ResourceWriter(rcNamespace, rcname, outputDirectory);

        Assert.IsTrue(
          fileCheck(
            Path.GetFullPath(Path.Combine(@"Resource\web", rcname)),
            Path.Combine(outputDirectory, rcname)
          ),
          rcname + "のファイル内容が不一致"
        );
      }

      // 対象リソースが見つからない場合に例外がかえるかのテスト
      try
      {
        await FileUtility.ResourceWriter(rcNamespace + ".", webResources[0], outputDirectory);
        Assert.Fail("namespaceにゴミつけたのに読みとれてる");
      }
      catch (FileLoadException) { }
      catch (FileNotFoundException) { }

      try
      {
        await FileUtility.ResourceWriter(rcNamespace, webResources[0] + ".", outputDirectory);
        Assert.Fail("ファイル名の後ろにゴミつけたのに読み取れてる");
      }
      catch (FileLoadException) { }
      catch (FileNotFoundException) { }

      // 上書き禁止時に正しくIOExceptionがかえるかテスト
      try
      {
        await FileUtility.ResourceWriter(rcNamespace, webResources[0], outputDirectory, overwrite: false);
        Assert.Fail("上書き禁止時にIOException出てない");
      }
      catch (IOException) { }

      string renameFilename = "rename-test.bin";
      await FileUtility.ResourceWriter(rcNamespace, webResources[0], outputDirectory, renameFilename);
      Assert.IsTrue(
        fileCheck(
          Path.GetFullPath(Path.Combine(@"Resource\web", webResources[0])),
          Path.Combine(outputDirectory, renameFilename)
        ),
        "別名での書き出しが正常に行えていない(ファイル内容不一致)"
      );
    }

    private bool fileCheck(string orig, string target)
    {
      FileStream fs1 = new FileStream(orig, FileMode.Open, FileAccess.Read, FileShare.Read);
      FileStream fs2 = new FileStream(target, FileMode.Open, FileAccess.Read, FileShare.Read);

      if (fs1.Length != fs2.Length) return false;

      int file1byte, file2byte;
      do
      {

        file1byte = fs1.ReadByte();
        file2byte = fs2.ReadByte();
      } while ((file1byte == file2byte) && file1byte != -1);

      fs1.Close();
      fs2.Close();

      return ((file1byte - file2byte) == 0);
    }
  }
}