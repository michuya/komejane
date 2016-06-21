using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using Assembly = System.Reflection.Assembly;

namespace Komejane
{
  public sealed class Config
  {
    /* --------------------------------------------------------------------- */
    #region シングルトン関係
    /* --------------------------------------------------------------------- */
    private static Config instance = new Config();

    public static Config Instance
    {
      get { return instance; }
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    private Config()
    {

    }

    // 初期値
    UInt16 port = 4815;
    string listenHost = "127.0.0.1";
    string dllDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    string webRootDirectory = "komejane\\web";
    string webIndex = "index.html";
    Dictionary<string, string> mimeFromExtension = new Dictionary<string, string>()
    {
      { "htm", "text/html" },
      { "html", "text/html" },
      { "txt", "text/plain" },
      { "js", "text/javascript" },
      { "css", "text/css" },
      { "gif", "image/gif" },
      { "png", "image/png" },
      { "jpg", "image/jpeg" },
      { "jpeg", "image/jpeg" }
    };

    // プロパティ
    public UInt16 Port
    {
      get { return port; }
    }

    public string ListenHost
    {
      get { return listenHost; }
    }

    public string DllDirectory
    {
      get { return dllDirectory; }
    }

    public string WebRootDirectory
    {
      get { return webRootDirectory; }
    }

    public string WebIndex
    {
      get { return webIndex; }
    }

    public Dictionary<string, string> MimeFromExtentionDictionary
    {
      get
      {
        return new Dictionary<string, string>(mimeFromExtension);
      }
    }
  }
}
