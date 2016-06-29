using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Reflection;

namespace Komejane.Server
{
  public class ControllerWrapper
  {
    IRouterController controller = null;


    public ControllerWrapper(string className, string @namespace = null)
    {
      CreateInstance(className, @namespace);
    }

    public void CreateInstance(string className, string @namespace = null, string asmName = null)
    {
      string @class = string.Format("{0}{2}{1}",
        @namespace,
        className,
        (string.IsNullOrWhiteSpace(@namespace)) ? "" : "."
      );
      
      lock (controller)
      {
        // アセンブリが指定されてた場合はアセンブリをロードしてインスタンスを生成する
        if (string.IsNullOrWhiteSpace(asmName))
        {
          Assembly asm = Assembly.LoadFrom(asmName);

          controller = (IRouterController)asm.CreateInstance(@class);
        }
        else
        {
          // Typeを取得する
          Type t = Type.GetType(@class);

          controller = (IRouterController)Activator.CreateInstance(t);
        }
      }
    }

    public void CallMethod(string method, HttpListenerRequest req, HttpListenerResponse res)
    {
      // コントローラが未設定の場合は処理しない
      if (controller == null) return;

      lock (controller)
      {
        // インスタンスのタイプを取得
        Type t = controller.GetType();

        // メソッド情報を取得
        System.Reflection.MethodInfo mi = t.GetMethod(method);

        // メソッドを呼び出す
        mi.Invoke(controller, new object[] { req, res });
      }
    }
  }

  public interface IRouterController
  {
    void index(HttpListenerRequest req, HttpListenerResponse res);
  }

  public abstract class BasicController : IRouterController
  {
    public BasicController()
    {

    }

    public virtual void index(HttpListenerRequest req, HttpListenerResponse res)
    {
      // アクセスログ
      Logger.Info(req.RemoteEndPoint.Address + " \"" + req.HttpMethod + " " + req.RawUrl + " HTTP/" + req.ProtocolVersion + "\" " + res.StatusCode + " " + res.ContentLength64 + " \"" + req.UrlReferrer + "\" \"" + req.UserAgent + "\"");

      // ストリームを閉じる
      res.Close();
    }
  }
}
