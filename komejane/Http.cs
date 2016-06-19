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
  public class HttpExceptionEventArgs : EventArgs
  {
    public HttpListenerException Exception { get; private set; }
    public HttpExceptionEventArgs(HttpListenerException ex)
    {
      Exception = ex;
    }
  }

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
    public event EventHandler ServerError;

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
    private void OnServerError(HttpExceptionEventArgs e)
    {
      if (ServerError != null)
        ServerError(this, e);
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
            System.Diagnostics.Debug.WriteLine("isServerSocketListening = false");
            return false;
          }
        }

        System.Diagnostics.Debug.WriteLine("isServerSocketListening = true");
        return true;
      }
    }

    public async void serverStart()
    {
      Config conf = Config.Instance;

      string prefix = "http://" + conf.ListenHost + ":" + conf.Port + "/";

      if (server == null)
        server = new HttpListener();

      // 接続元設定を適用
      try
      {
        server.Prefixes.Add(prefix);
      }
      catch (HttpListenerException ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.ToString());
        OnServerError(new HttpExceptionEventArgs(ex));
      }
      server.Start();

      await Task.Run(() =>
      {
        while (true)
        {
          if (isServerSocketListening())
          {
            OnServerStarted(new EventArgs());
            break;
          }

          System.Threading.Thread.Sleep(5);
        }
      });

    }

    public async void serverStop()
    {
      if (server == null || !server.IsListening) return;

      server.Stop();

      await Task.Run(() =>
      {
        while (true)
        {
          if (!isServerSocketListening())
          {
            OnServerStop(new EventArgs());
            break;
          }

          System.Threading.Thread.Sleep(5);
        }
      });
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
            {
              serverStart();
              break;
            }

            System.Threading.Thread.Sleep(5);
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
