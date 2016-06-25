using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace Komejane
{
  public class FileUtility
  {
    /* --------------------------------------------------------------------- */
    #region FindMimeFromData
    /* --------------------------------------------------------------------- */
    private enum FMFD : uint
    {
      FMFD_DEFAULT = 0,
      FMFD_URLASFILENAME = 1 << 0,
      FMFD_ENABLEMIMESNIFFING = 1 << 1,
      FMFD_IGNOREMIMETEXTPLAIN = 1 << 2,
      FMFD_SERVERMIME = 1 << 3, // ?
    }

    [DllImport("urlmon")]
    private static extern
    uint FindMimeFromData(IntPtr pBC,
                         IntPtr pwzUrl,
                         byte[] buffer,
                         int cbSize,
                         [In, MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
                         FMFD dwMimeFlags,
                         [MarshalAs(UnmanagedType.LPWStr)] out string ppwzMimeOut,
                         uint dwReserved);

    public static string FindMimeFromData(byte[] data, string mimeProposed = null)
    {
      const uint E_INVALIDARG = 0x80070057;
      string mime;

      var ret = FindMimeFromData(IntPtr.Zero, IntPtr.Zero, data, data.Length, mimeProposed, FMFD.FMFD_DEFAULT, out mime, 0);

      if (ret == 0)
        return mime;
      else if (ret == E_INVALIDARG)
        throw new ArgumentException();
      else
        return null;
    }

    public static string FindMimeFromStream(StreamReader stream, string mimeProposed = null)
    {
      char[] readBuff = new char[256];
      int readBytes = stream.ReadBlock(readBuff, 0, 256);

      List<byte> buff = new List<byte>(readBytes);
      foreach (char c in readBuff)
      {
        buff.AddRange(BitConverter.GetBytes(c));
      }

      return FindMimeFromData(buff.ToArray(), mimeProposed);
    }

    public static string FindMimeFromFile(string path, string mimeProposed = null)
    {
      System.IO.StreamReader stream = new System.IO.StreamReader(path);

      string result = FindMimeFromStream(stream, mimeProposed);

      stream.Close();
      stream.Dispose();

      return result;
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region リソース制御
    /* --------------------------------------------------------------------- */
    /// <summary>
    /// 埋め込みリソースを指定したパスへファイルとして出力する
    /// </summary>
    /// <param name="namespace">リソースの名前空間</param>
    /// <param name="resourceName">リソース名</param>
    /// <param name="outputDir">出力先ディレクトリ</param>
    /// <param name="outputFilename">出力時のファイル名(省略時はリソース名を使用)</param>
    /// <param name="overwrite">上書きをするか否か。デフォルトはtrue</param>
    public static async Task ResourceWriter(string @namespace, string resourceName, string outputDir, string outputFilename = null, bool overwrite = true)
    {
      if (string.IsNullOrWhiteSpace(@namespace) || string.IsNullOrWhiteSpace(resourceName) || string.IsNullOrWhiteSpace(outputDir)) {
        return;
      }

      string outputPath = Path.Combine(outputDir, (string.IsNullOrWhiteSpace(outputFilename) ? resourceName : outputFilename));

      // ファイル上書きフラグが無い場合は出力ファイルが被った場合に例外で失敗させる
      if (!overwrite && File.Exists(outputPath))
        throw new IOException("File already exists");

      // 現在実行中のアセンブリを取得
      var assm = Assembly.GetExecutingAssembly();

      // リソースとして埋め込んだ画像ファイルのストリームを取得
      using (var resource = assm.GetManifestResourceStream(string.Format("{0}.{1}", @namespace, resourceName)))
      {
        // リソースの取得に失敗している場合は例外
        if (resource == null) throw new FileNotFoundException();

        // ディレクトリがなかったら作成
        if (!Directory.Exists(outputDir))
          Directory.CreateDirectory(outputDir);

        using (FileStream writer = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
          // ストリームから書き込む
          await resource.CopyToAsync(writer);
        }
      }
    }
    public static string[] GetResources()
    {
      // 現在実行中のアセンブリを取得
      var assm = Assembly.GetExecutingAssembly();

      return assm.GetManifestResourceNames();
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

  }
}
