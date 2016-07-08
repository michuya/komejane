using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komejane.Server
{
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
      set { AddNode(key, value); }
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
}
