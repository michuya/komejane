using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Komejane
{
  class FileUtility
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

    public static string FindMimeFromStream(System.IO.StreamReader stream, string mimeProposed = null)
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
  }
}
