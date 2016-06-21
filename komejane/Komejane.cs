using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komejane
{
  public class Komejane
  {
    /* --------------------------------------------------------------------- */
    #region プラグイン定数とかステータスとか
    /* --------------------------------------------------------------------- */
    public enum UserType
    {
      Caster,
      Owner,
      Bsp,
      Member,
      Normal,
      System
    }

    static public String Version
    {
      get { return "0.0.1"; }
    }

    static public String Name
    {
      get { return "こめじゃね"; }
    }

    static public String Description
    {
      get { return "OBS CLR Plugin/Obs Studio Browser Plugin用HTML5コメントジェネレータ"; }
    }

    static public bool IsServerRun
    {
      get { return Http.isRun; }
    }

    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    public System.Windows.Forms.Form WindowOwner { get; set; }

    public Komejane()
    {

    }

    public bool IsAlive { get; protected set; }

    public void Run(bool isShowWindow = true)
    {
      Http server = Http.Instance;

      if (isShowWindow)
      {
        ControlForm form = new ControlForm();


        // オーナー取得できなくてもエラー起こさない
        if (WindowOwner != null)
        {
          try {
            form.Owner = WindowOwner;
            form.TopMost = WindowOwner.TopMost;
          } catch (Exception) { }
        }

        // フォームが閉じた時にIsAliveを殺す
        form.FormClosing += (sender, e) => { IsAlive = false; };

        // TODO: フォームの初期表示位置をコメビュのウィンドウの脇に設定(できれば他の子ウィンドウと被らない場所)
        if (form.Owner != null)
          form.Show();
        else
          form.ShowDialog();
      }

      IsAlive = true;
      Logger.Debug("プラグイン起動じゃい！");
    }

    public void Stop()
    {
      Http server = Http.Instance;
    }

    public async void AddComment(UserType type, int commentNo, string userId, string charaName, string comment, bool isAnonymous, int premium)
    {
      // コマンド系を無視
      if (comment.StartsWith("/")) return;

      string cstr = "[" + commentNo + ":" + charaName + "] " + comment + " // Type:" + type + ((isAnonymous) ? "(184)" : (premium > 0) ? "(" + premium + ")" : "");
      await Task.Run(() =>
      {
        Logger.Trace(cstr);
      });
    }
  }
}
