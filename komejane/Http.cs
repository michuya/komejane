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
    public event EventHandler<HttpRequestEventArgs> ClientRequest;
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
    private void OnClientRequest(HttpRequestEventArgs e)
    {
      if (ClientRequest != null)
        ClientRequest(this, e);
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

    private Http()
    {
      ClientConnection += Http_ClientConnection;
    }

    private async void Http_ClientConnection(object sender, HttpClientEventArgs e)
    {
#if DEBUG
      System.Diagnostics.Debug.WriteLine("Request => " + e.Context.Request.RawUrl);
#endif
      await Task.Run(() => {
        HttpListenerRequest req = e.Context.Request;
        HttpListenerResponse res = e.Context.Response;

        // "/stream"でWebSocketを待ち受ける
        if (req.IsWebSocketRequest && req.RawUrl == "/stream")
        {
          // アクセスログ
          Logger.Info(req.RemoteEndPoint.Address + " \"" + req.HttpMethod + " " + req.RawUrl + " WebSocket/Connect\" " + req.UrlReferrer + "\" \"" + req.UserAgent + "\"");

          WebSocketProc(e.Context);
        }
        else
        {
          OnClientRequest(new HttpRequestEventArgs(e.Context.Request, e.Context.Response));

          // アクセスログ
          Logger.Info(req.RemoteEndPoint.Address + " \"" + req.HttpMethod + " " + req.RawUrl + " HTTP/" + req.ProtocolVersion +"\" " + res.StatusCode + " " + res.ContentLength64 + " \"" + req.UrlReferrer + "\" \"" + req.UserAgent + "\"");
          e.Context.Response.Close();
        }
      });
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
        } catch (HttpListenerException) { break; } // 鯖停止時に例外くるからループ終了
        System.Diagnostics.Debug.WriteLine("Client connected");
        OnClientConnection(new HttpClientEventArgs(context));
      }
    }
    /* --------------------------------------------------------------------- */
#endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
#region WebSocketAPIコネクション
    /* --------------------------------------------------------------------- */
    private async void WebSocketProc(HttpListenerContext context)
    {
      var wscon = await context.AcceptWebSocketAsync(null);
      var ws = wscon.WebSocket;

      clients.Add(ws);

      while (ws.State == WebSocketState.Open)
      {
        try
        {
          var buff = new ArraySegment<byte>(new byte[1024]);

          // 受信待機
          var ret = await ws.ReceiveAsync(buff, System.Threading.CancellationToken.None);

          if (ret.MessageType == WebSocketMessageType.Text)
          {
            // なんか受信する物あるっけ？エラーとか？
          }
          else if (ret.MessageType == WebSocketMessageType.Close) /// クローズ
          {
            break;
          }
        } catch
        {
          break;
        }
      }

      clients.Remove(ws);
      ws.Dispose();
    }
    /* --------------------------------------------------------------------- */
#endregion
    /* --------------------------------------------------------------------- */

  }
}
