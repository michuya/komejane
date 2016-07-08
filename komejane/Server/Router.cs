﻿using System;
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
  /// ルーティング探索木用
  /// </summary>
  public class RouteNode
  {
    Dictionary<string, RouteNode> children = new Dictionary<string, RouteNode>();
    List<string> values = new List<string>();

    /* --------------------------------------------------------------------- */
    #region プロパティ
    /* --------------------------------------------------------------------- */
    public Dictionary<string, RouteNode> Children
    { get { return children; } }

    public RouteControllerInfo Controller { get; protected set; }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region コンストラクタ/デストラクタ
    /* --------------------------------------------------------------------- */
    public RouteNode() { }

    ~RouteNode()
    {
      Dispose();
    }

    /// <summary>
    /// 再帰的に子ノード全部開放
    /// </summary>
    public void Dispose()
    {
      values.Clear();
      foreach (RouteNode node in children.Values)
      {
        node.Dispose();
      }
      children.Clear();
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region 演算子
    /* --------------------------------------------------------------------- */
    public RouteNode this[string key]
    {
      set { AddNode(key, value);  }
      get { return children[key]; }
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region データ追加、削除
    /* --------------------------------------------------------------------- */
    public RouteNode CreateNode(string key)
    {
      if (!children.ContainsKey(key)) children.Add(key, new RouteNode());
      return this[key];
    }
    public RouteNode AddNode(string key, RouteNode node)
    {
      if (children.ContainsKey(key))
        children[key] = node;
      else
        children.Add(key, node);
      return node;
    }

    public RouteControllerInfo SetController(RouteControllerInfo info)
    {
      Controller = info;
      return info;
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region チェック関係
    /* --------------------------------------------------------------------- */
    public bool isContainer(string key)
    {
      return children.ContainsKey(key);
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region 拡張機能
    /* --------------------------------------------------------------------- */
    public override string ToString()
    {
      string cache = "";

      if (children.Count == 0)
      {
        return string.Format("<{0}>", Controller.ToString());
      }

      bool first = true;
      int keyMaxLen = 0;
      foreach (var key in children.Keys)
      {
        if (key.Length > keyMaxLen) { keyMaxLen = key.Length; }
      }
      foreach (var pair in children)
      {
        string childObject = string.Format("[{0}] {1}=> ", pair.Key, new string('=', keyMaxLen - pair.Key.Length));
        cache += string.Format("{2}{0}{1}", childObject, pair.Value.ToString().Replace("\n", "\n" + new string(' ', keyMaxLen + 6)), (first) ? "" : "\n");
        first = false;
      }

      if (!RouteControllerInfo.IsEmpty(Controller))
        cache = string.Format("{0}\n<{1}>", cache, Controller.ToString());

      return cache;
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */
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
      DefaultController = new RouteControllerInfo("DefaultController", new string[] {
        System.IO.Path.Combine(Config.Instance.DllDirectory, Config.Instance.WebRootDirectory),
        Config.Instance.WebIndex
      });

      DefaultController dc = (DefaultController)AddController(DefaultController).ControllerInstance;
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

      string method = match.Groups[1].Value.ToUpper();
      Uri uri = new Uri("http://example.com" + match.Groups[2].Value);
      string controller = match.Groups[3].Value;

      // add method
      if (!root.isContainer(method))
        root.AddNode(method, new RouteNode());

      RouteNode current = root[method];

      foreach (string s in uri.Segments)
      {
        current = current.CreateNode(s);
      }

      // コントローラ情報を登録
      current.SetController(Router.ControllerParser(controller));
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region ルーティング処理
    /* --------------------------------------------------------------------- */
    public void Routing(HttpListenerRequest req, HttpListenerResponse res)
    {
      // ルーティングに存在しないメソッド叩いても404しか返さないよ！
      if (!root.isContainer(req.HttpMethod.ToUpper()))
      {
        ResponseNotFound(res);
        return;
      }

      // ルーティングを検索
      // TODO: コントローラ探索の仕様をもっと柔軟にする
      RouteNode current = root[req.HttpMethod.ToUpper()];
      RouteControllerInfo controller = DefaultController;
      foreach(string dir in req.Url.Segments)
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

      // コントローラがうまく取得できなかった場合は強制404
      if (RouteControllerInfo.IsEmpty(controller))
      {
        ResponseNotFound(res);
        return;
      }

      // コントローラを呼び出す
      ControllerWrapper wrapper = GetController(controller);

      if (wrapper == null)
      {
        ResponseServerError(res);
        return;
      }
      else if (string.IsNullOrWhiteSpace(controller.Method))
        wrapper.CallMethod("index", req, res);
      else
        wrapper.CallMethod(controller.Method, req, res);

      // 念のためストリームが閉じてなかったらクローズしとく
      try { if (res.OutputStream.CanWrite) res.Close(); }
      catch (ObjectDisposedException) { }
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region コントローラー関連
    /* --------------------------------------------------------------------- */
    protected ControllerWrapper AddController(RouteControllerInfo controller)
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

    protected ControllerWrapper GetController(RouteControllerInfo controller)
    {
      if (RouteControllerInfo.IsEmpty(controller))
        return null;

      if (!Controllers.ContainsKey(controller.Name))
      {
        if (AddController(controller) == null) return null;
      }

      return Controllers[controller.Name];
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */
  }
}
