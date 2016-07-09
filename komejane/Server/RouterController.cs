using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Reflection;

namespace Komejane.Server.Controller
{
  public class ControllerWrapper
  {
    volatile object lockObj = new object();
    public IRouterController ControllerInstance { get; protected set; }

    public ControllerWrapper(string className, object[] constructorArgs = null, string @namespace = null)
    {
      CreateInstance(className, args:constructorArgs, @namespace:@namespace);
    }

    public void CreateInstance(string className, object[] args = null, string @namespace = null, string asmName = null)
    {
      string @class = string.Format("{0}{2}{1}",
        @namespace,
        className,
        (string.IsNullOrWhiteSpace(@namespace)) ? "" : "."
      );
      
      lock (lockObj)
      {
        // アセンブリが指定されてた場合はアセンブリをロードしてインスタンスを生成する
        if (!string.IsNullOrWhiteSpace(asmName))
        {
          Assembly asm = Assembly.LoadFrom(asmName);

          ControllerInstance = (IRouterController)asm.CreateInstance(@class);
        }
        else
        {
          // Typeを取得する
          Type t = Type.GetType(@class);
          string s = typeof(DefaultController).ToString();
          if (args == null)
            ControllerInstance = (IRouterController)Activator.CreateInstance(t);
          else
            ControllerInstance = (IRouterController)Activator.CreateInstance(t, args);
        }
      }
    }

    public void CallMethod(string method, HttpListenerContext context)
    {
      // コントローラが未設定の場合は処理しない
      if (ControllerInstance == null) return;

      lock (lockObj)
      {
        // インスタンスのタイプを取得
        Type t = ControllerInstance.GetType();

        // メソッド情報を取得
        System.Reflection.MethodInfo mi = t.GetMethod(method);

        // メソッドを呼び出す
        mi.Invoke(ControllerInstance, new object[] { context });
      }
    }
  }

  public interface IRouterController
  {
    void index(HttpListenerContext context);
  }

  public abstract class BasicController : IRouterController
  {
    public BasicController()
    {

    }

    public virtual void index(HttpListenerContext context)
    {
      HttpListenerRequest req = context.Request;
      HttpListenerResponse res = context.Response;

      // アクセスログ
      Logger.Info(req.RemoteEndPoint.Address + " \"" + req.HttpMethod + " " + req.RawUrl + " HTTP/" + req.ProtocolVersion + "\" " + res.StatusCode + " " + res.ContentLength64 + " \"" + req.UrlReferrer + "\" \"" + req.UserAgent + "\"");

      // ストリームを閉じる
      res.Close();
    }
  }
}
