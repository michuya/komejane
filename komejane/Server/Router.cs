using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

using System.Text.RegularExpressions;

namespace Komejane.Server
{
  // simple tree node
  public class RouteNode
  {
    Dictionary<string, RouteNode> children = new Dictionary<string, RouteNode>();
    List<string> values = new List<string>();

    /* --------------------------------------------------------------------- */
    #region プロパティ
    /* --------------------------------------------------------------------- */
    public Dictionary<string, RouteNode> Children
    { get { return children; } }

    public List<string> Value
    { get { return values; } }
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

    public RouteNode AddValue(string value)
    {
      values.Add(value);
      return this;
    }

    public RouteNode AddValues(string[] values)
    {
      this.values.AddRange(values);
      return this;
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
        return string.Format("<{0}>", string.Join(", ", values));
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

      if (values.Count > 0)
        cache = string.Format("{0}\n<{1}>", cache, string.Join(", ", values));

      return cache;
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */
  }

  public class Router
  {
    Regex methodParser = new Regex(@"^\s*(get|post|put|delete|head|patch)\s+([a-z0-9_\-:/]*)\s+([a-z0-9_\.\(\)]*)\s*$", RegexOptions.IgnoreCase);

    public delegate void ControllerMethodDelegate(HttpListenerRequest req, HttpListenerResponse res);

    RouteNode root = new RouteNode();
    public ControllerMethodDelegate DefaultController { get; set; }

    public RouteNode RootNode
    {
      get { return root; }
    }

    /* --------------------------------------------------------------------- */
    #region コンストラクタ/デストラクタ
    /* --------------------------------------------------------------------- */
    public Router()
    {
    }

    public Router(string[] routes)
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

      // TODO: メソッド周りのパース処理を実装
      current.AddValues(controller.Replace("()", "").Split('.'));
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
      RouteNode current = root[req.HttpMethod.ToUpper()];
      foreach(string dir in req.Url.Segments)
      {
        if (current.isContainer(dir))
          current = current[dir];
        else
        {
          // TODO: ファイルへのリクエストを確認した上でファイルがなければ404を返すようにする
          //       もしくはファイルリクエスト用のコントローラへ制御を委譲
          ResponseNotFound(res);
          return;
        }
      }

      // コントローラ未登録のパスだった場合は404
      if (current.Value.Count < 1)
      {
        ResponseNotFound(res);
        return;
      }

      // TODO: 登録されたコントローラを呼び出す
      res.Close();
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */
  }
}
