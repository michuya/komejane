using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komejane
{
  public class CommentData
  {
    public enum UserType
    {
      Caster,
      Owner,
      Bsp,
      Member,
      Normal,
      System
    }

    /* --------------------------------------------------------------------- */
    #region 初期値
    /* --------------------------------------------------------------------- */
    const UserType _type = UserType.Normal;
    const int _commentNo = -1;
    const string _userId = "";
    const string _charaName = "";
    const string _comment = "/nop";
    const bool _isAnonymous = true;
    const int _premium = 0;
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region プロパティ
    /* --------------------------------------------------------------------- */
    /// <summary>
    /// コマンド文か否か
    /// </summary>
    public bool IsCommand { get { return _comment.StartsWith("/"); } }

    public UserType Type { get; set; } = _type;
    public int CommentNo { get; set; } = _commentNo;
    public string Comment { get; set; } = _comment;
    public string UserId { get; set; } = _userId;
    public string CharaName { get; set; } = _charaName;
    bool IsAnonymous { get; set; } = _isAnonymous;
    int Premium { get; set; } = _premium;

    public bool IsDefaultName { get { return string.IsNullOrWhiteSpace(CharaName); } }
    public string Name { get { return (IsDefaultName) ? Config.Instance.DefaultName : CharaName; } }

    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    public CommentData(int commentNo, string comment, UserType type = _type, string userId = _userId, string charaName = _charaName, bool isAnonymous = _isAnonymous, int premium = _premium)
    {
      Type = type;
      CommentNo = commentNo;
      Comment = comment;
      UserId = userId;
      CharaName = charaName;
      IsAnonymous = isAnonymous;
      Premium = premium;
    }

    public override string ToString()
    {
      return "[" + CommentNo + ":" + CharaName + "] " + Comment + " // Type:" + Type + ((IsAnonymous) ? "(184)" : (Premium > 0) ? "(" + Premium + ")" : "");
    }
  }
}
