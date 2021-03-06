﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.WebSockets;
using System.Net.Sockets;
using System.Threading.Tasks;

using System.IO;

namespace Komejane.Server
{
  public class HttpExceptionEventArgs : EventArgs
  {
    public HttpListenerException Exception { get; private set; }
    public HttpExceptionEventArgs(HttpListenerException ex)
    {
      Exception = ex;
    }
  }
  public class HttpClientEventArgs : EventArgs
  {
    public HttpListenerContext Context { get; private set; }
    public HttpClientEventArgs(HttpListenerContext context)
    {
      Context = context;
    }
  }
  public class HttpRequestEventArgs : EventArgs
  {
    public HttpListenerRequest Request { get; private set; }
    public HttpListenerResponse Response { get; private set; }
    public HttpRequestEventArgs(HttpListenerRequest req, HttpListenerResponse res)
    {
      Request = req;
      Response = res;
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
    public event EventHandler<HttpClientEventArgs> ClientConnection;
    public event EventHandler<HttpClientEventArgs> ClientDisconnected;

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
    private void OnClientConnection(HttpClientEventArgs e)
    {
      if (ClientConnection != null)
        ClientConnection(this, e);
    }
    private void OnClientDisconnection(HttpClientEventArgs e)
    {
      if (ClientDisconnected != null)
        ClientDisconnected(this, e);
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    public static bool isRun
    {
      get { return (instance != null && instance.isServerRun); }
    }

    HttpListener server = null;
    List<WebSocket> clients = new List<WebSocket>(32);
    Router router = new Router();

    private Http()
    {
      router.AddRoutes(Config.Instance.WebRoutes);
    }

    private bool isServerRun
    {
      get { return (server != null && server.IsListening); }
    }

    /* --------------------------------------------------------------------- */
    #region サーバ制御関係
    /* --------------------------------------------------------------------- */

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

      // TODO: ホストが+や*や0.0.0.0の場合に自身のIPアドレスを表示させる
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

      try
      {
        server.Start();
      }
      catch (HttpListenerException)
      {
        server = null;
        // TODO: アクセス制限が発生した場合に解消するヒントを表示
        return;
      }

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

      HttpListen();
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
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region 標準HTTPコネクション
    /* --------------------------------------------------------------------- */
    private async void HttpListen()
    {
      while (true)
      {
        HttpListenerContext context;
        try
        {
          context = await server.GetContextAsync();
        }
        catch (HttpListenerException) { break; } // 鯖停止時に例外くるからループ終了
        System.Diagnostics.Debug.WriteLine("Client connected");

        OnClientConnection(new HttpClientEventArgs(context));
        router.Routing(context);
      }
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region WebSocket関連
    /* --------------------------------------------------------------------- */
    public void SendComment(CommentData comment)
    {
      Controller.WebSocketController.sendMessageAllClient(comment.ToJson());
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

  }
}
