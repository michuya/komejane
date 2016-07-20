using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace Komejane
{
  [DataContract]
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
    public bool IsCommand { get { return Comment.StartsWith("/"); } }

    [DataMember]
    public UserType Type { get; set; } = _type;
    [DataMember]
    public int CommentNo { get; set; } = _commentNo;
    [DataMember]
    public string Comment { get; set; } = _comment;
    [DataMember]
    public string UserId { get; set; } = _userId;
    [DataMember]
    public string CharaName { get; set; } = _charaName;
    [DataMember]
    bool IsAnonymous { get; set; } = _isAnonymous;
    [DataMember]
    int Premium { get; set; } = _premium;

    public bool IsDefaultName { get { return string.IsNullOrWhiteSpace(CharaName); } }
    [DataMember]
    public string Name
    {
      get { return (IsDefaultName) ? Config.Instance.DefaultName : CharaName; }
      private set { } // dummy setter
    }

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
    public string ToJson()
    {
      var serializer = new DataContractJsonSerializer(typeof(CommentData));

      var mstream = new MemoryStream();

      serializer.WriteObject(mstream, this);

      return Encoding.UTF8.GetString(mstream.ToArray());
    }
  }
}
