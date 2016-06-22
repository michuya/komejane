using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace Komejane.Server
{
  // simple tree node
  public class RouteNode
  {
    Dictionary<string, RouteNode> children = new Dictionary<string, RouteNode>();
    List<string> values = new List<string>();

    public Dictionary<string, RouteNode> Children
    { get { return children; } }

    public List<string> Value
    { get { return values; } }

    public RouteNode()
    {

    }

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

      return cache;
    }
    public string ToString(int offset)
    {
      var offsetSpace = new string(' ', offset);
      return Regex.Replace(this.ToString(), "^", offsetSpace, RegexOptions.Multiline);
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

  }

  public class Router
  {
    Regex methodParser = new Regex(@"^\s*(get|post|put|delete|head|patch)\s+([a-z0-9_\-:/]*)\s+([a-z0-9_]*)\s*$", RegexOptions.IgnoreCase);

    RouteNode root = new RouteNode();

    public RouteNode RootNode
    {
      get { return root; }
    }

    public Router()
    {
    }

    public Router(string[] routes)
    {
      AddRoutes(routes);
    }

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
      string uri = match.Groups[2].Value;
      string controller = match.Groups[3].Value;

      string[] parsedUri = uri.Split('/').Skip(1).ToArray(); // 先頭の要素はrootの前に当たる位置だから削除

      // add method
      if (!root.isContainer(method))
        root.AddNode(method, new RouteNode());

      RouteNode current = root[method];

      foreach (string s in parsedUri)
      {
        current = current.CreateNode("/" + s);
      }
      current.AddValue(controller);
    }
  }
}
