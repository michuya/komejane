using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Komejane.Server.Controller
{
  class WebSocketController : BasicController
  {
    static List<WebSocket> _client = new List<WebSocket>();

    static WebSocket[] WSClinets { get { return _client.ToArray(); } }

    public void info(HttpListenerContext context)
    {
      HttpListenerRequest req = context.Request;
      HttpListenerResponse res = context.Response;

      const string response = "{\"websocket\":{\"streamUri\":\"/stream\"}}";
      byte[] buffer = Encoding.ASCII.GetBytes(response);
      res.OutputStream.Write(buffer, 0, buffer.Length);

      base.index(context);
    }

    protected void sendMessageToClient(WebSocket client, string msg)
    {
      ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));

      client.SendAsync(buffer, WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
    }

    protected async void wsMessageProc(WebSocket client, string msg)
    {
      await Task.Run(() =>
      {
        switch (msg)
        {
          case "/ping":
            sendMessageToClient(client, "/pong");
            break;
        }
      });
    }

    public async void stream(HttpListenerContext context)
    {
      var ws = (await context.AcceptWebSocketAsync(null)).WebSocket;

      // 新規クライアントを追加
      _client.Add(ws);

      await Task.Delay(1000);

      // WebSocketの送受信ループ
      while (ws.State == WebSocketState.Open)
      {
        try
        {
          var buff = new ArraySegment<byte>(new byte[1024]);

          /// 受信待機
          var ret = await ws.ReceiveAsync(buff, System.Threading.CancellationToken.None);
          
          /// テキスト
          if (ret.MessageType == WebSocketMessageType.Text)
          {
            string msg = Encoding.UTF8.GetString(buff.Take(ret.Count).ToArray());
            Logger.Trace(string.Format("WSMessage: {0}", msg));

            wsMessageProc(ws, msg);
          }
          else if (ret.MessageType == WebSocketMessageType.Close) /// クローズ
          {
            Logger.Info(string.Format("{0}:Session Close:{1}", DateTime.Now.ToString(), context.Request.RemoteEndPoint.Address.ToString()));
            break;
          }
        }
        catch
        {
          // 例外 クライアントが異常終了しやがった
          Logger.Info(string.Format("{0}:Session Abort:{1}", DateTime.Now.ToString(), context.Request.RemoteEndPoint.Address.ToString()));
          break;
        }
      }

      // クライアントを切断
      _client.Remove(ws);
      ws.Dispose();
    }

    public override void index(HttpListenerContext context)
    {
      HttpListenerRequest req = context.Request;
      HttpListenerResponse res = context.Response;

      base.index(context);
    }
  }
}
