using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

using System.Text.RegularExpressions;

using Komejane.Server.Controller;

namespace Komejane.Server
{
  /// <summary>
  /// コントローラ情報
  /// </summary>
  public struct RouteControllerInfo
  {
    public string Name { get; private set; }
    public string Method { get; private set; }
    string[] _options;
    public string[] Options { get {
        if (_options == null) return new string[0];
        else return _options;
      }
      private set {
        _options = value;
      }
    }

    public RouteControllerInfo(string name, string[] opt, string method = null)
    {
      Name = name;
      _options = opt;
      Method = method;
    }

    public static bool IsEmpty(RouteControllerInfo target)
    {
      return string.IsNullOrWhiteSpace(target.Name);
    }

    public override string ToString()
    {
      if (IsEmpty(this)) return "";

      string ret = string.Format("{0}({1})", Name, (Options != null) ? string.Join(", ", Options) : "");
      if (!string.IsNullOrWhiteSpace(Method))
        ret += "." + Method + "()";
      return ret;
    }
  }

  /// <summary>
  /// ルーティング処理する奴
  /// </summary>
  public class Router
  {
    Regex methodParser = new Regex(@"^\s*(get|post|put|delete|head|patch)\s+([a-z0-9_\-:/]*)\s+([a-z0-9_\.\(\)]*)\s*$", RegexOptions.IgnoreCase);

    RouteNode root = new RouteNode();
    public RouteControllerInfo DefaultController { get; set; }

    Dictionary<string, ControllerWrapper> Controllers = new Dictionary<string, ControllerWrapper>();

    public RouteNode RootNode
    {
      get { return root; }
    }

    /* --------------------------------------------------------------------- */
    #region コンストラクタ/デストラクタ
    /* --------------------------------------------------------------------- */
    public Router()
    {
      // デフォルトコントローラを作成
      DefaultController = new RouteControllerInfo("DefaultController", new string[] {
        System.IO.Path.Combine(Config.Instance.DllDirectory, Config.Instance.WebRootDirectory),
        Config.Instance.WebIndex
      });

      // デフォルトコントローラにMIME辞書を登録
      DefaultController dc = (DefaultController)RegistController(DefaultController).ControllerInstance;
      dc.MimeDictionary = Config.Instance.MimeFromExtentionDictionary;
    }

    public Router(string[] routes):this()
    {
      AddRoutes(routes);
    }

    ~Router()
    {
      root.Dispose();
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region ユーティリティメソッド
    /* --------------------------------------------------------------------- */
    public static void ResponseNotFound(HttpListenerResponse res)
    {
      res.StatusCode = 404;
      res.Close();
    }

    public static void ResponseServerError(HttpListenerResponse res)
    {
      res.StatusCode = 500;
      res.Close();
    }

    public static RouteControllerInfo ControllerParser(string ctrl)
    {
      string[] splitCtrl = ctrl.Split('.');

      if (splitCtrl.Length > 2) throw new ArgumentException("メソッドチェーンには対応していません。");

      // コントローラをパース
      var ctrlMatch = Regex.Match(splitCtrl[0], @"([_a-zA-Z][_a-zA-Z0-9]+)(\(.*?\))?");

      if (ctrlMatch.Groups.Count < 2) throw new ArgumentException("コントローラ名が不正です。");

      // コントローラ情報をRouteControllerInfo用に変換
      string controller = ctrlMatch.Groups[1].Value;
      string[] controllerArgs = null;

      // パラメータがある場合はそれも変換
      if (ctrlMatch.Groups.Count > 2)
      {
        // バギーな引数パーサー。引数内にカンマが含まれた場合に狂う。
        // TODO: バギーな処理を修正
        controllerArgs = Regex.Split(ctrlMatch.Groups[2].Value.Trim("() ".ToCharArray()), @"\s*,\s*");

        // 引数フォーマットのチェックと修正
        if (controllerArgs.Length == 1 && string.IsNullOrWhiteSpace(controllerArgs[0]))
          controllerArgs = null;
        else if (controllerArgs.Contains("")) throw new ArgumentException("コントローラのパラメータが不正です。");
        else controllerArgs = controllerArgs.Select((s) => s.Trim("\"".ToCharArray())).ToArray();
      }

      // メソッドがある場合はsplitの結果が2個になる
      string method = null;
      if (splitCtrl.Length > 1)
      {
        // メソッド名をパース
        var m = Regex.Match(splitCtrl[1], @"([_a-zA-Z][_a-zA-Z0-9]+)(\(\))?");

        // メソッドのフォーマットに異常がなければ通常文字列に変換
        if (m.Groups.Count == 3)
        {
          if (!string.IsNullOrWhiteSpace(m.Groups[2].Value.Trim("() ".ToCharArray())))
            throw new ArgumentException("メソッドの引数は利用できません。");
          method = m.Groups[1].Value;
        } else throw new ArgumentException("メソッド名に不正な文字が含まれています。");
      }

      // パースした物を入れたRouteControllerInfoを作って返す
      return new RouteControllerInfo(controller, controllerArgs, method);
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region ルート設定
    /* --------------------------------------------------------------------- */
    public void AddRoutes(string[] routes)
    {
      foreach (var route in routes)
      {
        AddRoute(route);
      }
    }

    public void AddRoute(string route)
    {
      Match match = methodParser.Match(route);

      if (match.Groups.Count < 4) throw new ArgumentException();

      // メソッド、URLパス、コントローラを分割
      string method = match.Groups[1].Value.ToUpper();
      Uri uri = new Uri("http://example.com" + match.Groups[2].Value); // ドメイン名はダミー
      string controller = match.Groups[3].Value;

      // HTTPメソッドをrootに追加
      if (!root.isContainer(method))
        root.AddNode(method, new RouteNode());

      // HTTPメソッド下のツリーにノードを追加
      RouteNode current = root[method];

      foreach (string s in uri.Segments)
        current = current.CreateNode(s);

      // コントローラ情報を登録
      current.SetController(Router.ControllerParser(controller));
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region ルーティング処理
    /* --------------------------------------------------------------------- */
    protected RouteControllerInfo GetRouteController(string httpMethod, Uri url)
    {
      // TODO: コントローラ探索の仕様をもっと柔軟にする
      RouteNode current = root[httpMethod.ToUpper()];
      RouteControllerInfo controller = DefaultController;
      foreach (string dir in url.Segments)
      {
        if (current.isContainer(dir))
        {
          current = current[dir];
          if (!RouteControllerInfo.IsEmpty(current.Controller))
            controller = current.Controller;
        }
        else
        {
          // ファイルリクエスト用のコントローラへ制御を委譲
          controller = DefaultController;
          break;
        }
      }

      return controller;
    }

    /// <summary>
    /// コントローラを呼び出す
    /// </summary>
    /// <param name="controller">コントローラ情報</param>
    /// <param name="context">接続してきたクライアントのコンテキスト</param>
    protected void RunController(RouteControllerInfo controller, HttpListenerContext context)
    {
      ControllerWrapper wrapper = GetControllerWrapper(controller);

      if (wrapper == null)
      {
        ResponseServerError(context.Response);
        return;
      }
      else
      {
        string method = "index";
        if (!string.IsNullOrWhiteSpace(controller.Method))
          method = controller.Method;
        wrapper.CallMethod(method, context);
      }
    }

    public void Routing(HttpListenerContext context)
    {
      HttpListenerRequest req = context.Request;
      HttpListenerResponse res = context.Response;

      // ルーティングに存在しないメソッド叩いても404しか返さないよ！
      if (!root.isContainer(req.HttpMethod.ToUpper()))
      {
        ResponseNotFound(res);
        return;
      }

      // ルーティング情報からコントローラを取得
      RouteControllerInfo controller = GetRouteController(req.HttpMethod, req.Url);

      // コントローラがうまく取得できなかった場合は強制404
      if (RouteControllerInfo.IsEmpty(controller))
      {
        ResponseNotFound(res);
        return;
      }

      // コントローラを呼び出す
      RunController(controller, context);
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region コントローラー関連
    /* --------------------------------------------------------------------- */
    protected ControllerWrapper RegistController(RouteControllerInfo controller)
    {
      try
      {
        ControllerWrapper instance = new ControllerWrapper(controller.Name,
          constructorArgs: (controller.Options.Length > 0) ? controller.Options : null,
          @namespace: "Komejane.Server.Controller");

        Controllers.Add(controller.Name, instance);

        return instance;
      }
      catch (ArgumentException) { return null; }
    }

    protected ControllerWrapper GetControllerWrapper(RouteControllerInfo controller)
    {
      if (RouteControllerInfo.IsEmpty(controller))
        return null;

      if (!Controllers.ContainsKey(controller.Name))
      {
        if (RegistController(controller) == null) return null;
      }

      return Controllers[controller.Name];
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */
  }
}
