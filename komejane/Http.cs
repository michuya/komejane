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

    public static bool isRun
    {
      get { return (instance != null && instance.isServerRun); }
    }

    HttpListener server = null;

    private Http()
    {
      Config conf = Config.Instance;

      string prefix = "http://" + conf.ListenHost + ":" + conf.Port + "/";

    }

    private bool isServerSocketListening()
    {
      Config conf = Config.Instance;

      using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
      {
        try
        {
          socket.Connect("127.0.0.1", conf.Port);
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

    private async void serverReload()
    {
      Action serverStart = () =>
      {
        server = new HttpListener();
        server.Start();
      };

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
