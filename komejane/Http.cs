using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.WebSockets;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Komejane
{
  public sealed class Http
  {
    /* --------------------------------------------------------------------- */
    #region シングルトン関係
    /* --------------------------------------------------------------------- */
    private static Http instance = new Http();

    public static Http Instance
    {
      get { return instance; }
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region イベント関係
    /* --------------------------------------------------------------------- */
    public event EventHandler ServerStarted;
    public event EventHandler ServerStop;

    private void OnServerStarted(EventArgs e)
    {
      if (ServerStarted != null)
        ServerStarted(this, e);
    }
    private void OnServerStop(EventArgs e)
    {
      if (ServerStop != null)
        ServerStop(this, e);
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    public static bool isRun
    {
      get { return (instance != null && instance.isServerRun); }
    }

    HttpListener server = null;

    private Http()
    {


    }

    private bool isServerSocketListening()
    {
      Config conf = Config.Instance;

      using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
      {
        try
        {
          socket.Connect(conf.ListenHost, conf.Port);
        }
        catch (SocketException ex)
        {
          if (ex.SocketErrorCode == SocketError.ConnectionRefused)
          {
            return false;
          }
        }

        return true;
      }
    }

    public void serverStart()
    {
      Config conf = Config.Instance;

      string prefix = "http://" + conf.ListenHost + ":" + conf.Port + "/";

      if (server == null)
        server = new HttpListener();

      // 接続元設定を適用
      server.Prefixes.Add(prefix);
      server.Start();

      OnServerStarted(new EventArgs());
    }

    public void serverStop()
    {
      if (server == null || !server.IsListening) return;

      server.Stop();
      OnServerStop(new EventArgs());
    }

    public async void serverRestart()
    {
      await Task.Run(() =>
      {
        if (server != null)
        {
          server.Abort();
          server.Close();
          server = null;

          while (true)
          {
            if (!isServerSocketListening())
              serverStart();
          }
        }
        else
          serverStart();
      });
    }

    private bool isServerRun
    {
      get { return (server != null && server.IsListening); }
    }
  }
}
