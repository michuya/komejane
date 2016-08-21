using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

using AsyncLock=Komejane.Util.AsyncLock;

namespace Komejane.Server.Controller
{
  class WebSocketController : BasicController
  {
    public class WSClientData: IDisposable
    {
      public WebSocket sock;
      public AsyncLock lockObj;

      public WSClientData(WebSocket ws)
      {
        this.sock = ws;
        this.lockObj = new AsyncLock();
      }
      public void Dispose()
      {
        sock.Dispose();
      }
    }

    static List<WSClientData> clients = new List<WSClientData>();

    public static WebSocket[] WSClinets { get { return clients.Select((cs) => cs.sock).ToArray(); } }

    public static async void sendMessageAllClient(string msg)
    {
      await Task.Run(() => {
        clients.ForEach((cs) => sendMessageToClient(cs, msg));
      });
    }

    protected static async void sendMessageToClient(WSClientData client, string msg)
    {
      ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));

      using (await client.lockObj.LockAsync())
      {
        await client.sock.SendAsync(buffer, WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
      }
    }

    public void info(HttpListenerContext context)
    {
      HttpListenerRequest req = context.Request;
      HttpListenerResponse res = context.Response;

      const string response = "{\"websocket\":{\"streamUri\":\"/stream\"}}";
      byte[] buffer = Encoding.ASCII.GetBytes(response);
      res.OutputStream.Write(buffer, 0, buffer.Length);

      base.index(context);
    }


    protected async void wsMessageProc(WSClientData client, string msg)
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
      var wsContext = await context.AcceptWebSocketAsync(null);
      var ws = wsContext.WebSocket;

      WSClientData cs = new WSClientData(ws);

      // 新規クライアントを追加
      clients.Add(cs);

      Logger.Info("{0}:Session Start:{1}", DateTime.Now.ToString(), context.Request.RemoteEndPoint.Address.ToString());

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
            Logger.Trace("{0}:WSMessage:{1}", context.Request.RemoteEndPoint.Address.ToString(), msg);

            wsMessageProc(cs, msg);
          }
          else if (ret.MessageType == WebSocketMessageType.Close) /// クローズ
          {
            Logger.Info("{0}:Session Close:{1}", DateTime.Now.ToString(), context.Request.RemoteEndPoint.Address.ToString());
            break;
          }
        }
        catch (WebSocketException)
        {
          // 例外 クライアントが異常終了しやがった
          Logger.Info("{0}:Session Abort:{1}", DateTime.Now.ToString(), context.Request.RemoteEndPoint.Address.ToString());
          break;
        }
      }

      // クライアントを切断
      clients.Remove(cs);
      cs.Dispose();
    }

    public override void index(HttpListenerContext context)
    {
      HttpListenerRequest req = context.Request;
      HttpListenerResponse res = context.Response;

      base.index(context);
    }
  }
}
