using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Komejane
{
  public sealed class Config
  {
    private static Config instance = new Config();

    public static Config Instance
    {
      get { return instance; }
    }

    private Config()
    {

    }

    // 初期値
    UInt16 port = 4815;
    string listenHost = "127.0.0.1";

    // プロパティ
    public UInt16 Port
    {
      get { return port; }
    }

    public string ListenHost
    {
      get { return listenHost; }
    }
  }
}
