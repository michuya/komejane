using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using Komejane.Server;

namespace Komejane
{
  public class Komejane
  {
    /* --------------------------------------------------------------------- */
    #region プラグイン定数とかステータスとか
    /* --------------------------------------------------------------------- */


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

    static public bool Initialized { get; protected set; } = false;

    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    public System.Windows.Forms.Form WindowOwner { get; set; }

    public Komejane()
    {

    }

    private async void InitializeWebDirectory()
    {
      string webDir = Path.Combine(Config.Instance.DllDirectory, Config.Instance.WebRootDirectory);

      if (Directory.Exists(webDir))
      {
        if (File.Exists(Path.Combine(webDir, Config.Instance.WebIndex)))
          return;

        else
        {
          // index.htmlだけチェック
          var dlgResult = System.Windows.Forms.MessageBox.Show("Webディレクトリに" + Config.Instance.WebIndex + "が見つかりません。\n初期ファイルを設置しますか？", "こめじゃね！", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);

          if (dlgResult == System.Windows.Forms.DialogResult.Yes)
            await FileUtility.ResourceWriter("Komejane.Resource.web", "index.html", Path.Combine(webDir, Config.Instance.WebIndex));
        }
      }
      else
      {
        string[] webResources = {
          "index.html",
          "komejane.js",
          "style.css"
        };

        // 初期ディレクトリを生成
        Directory.CreateDirectory(webDir);

        // リソース一式を書き出す
        // TODO: webディレクトリ以下のディレクトリ構造を自動で再現できるようにする
        foreach (string rcname in webResources)
        {
          await FileUtility.ResourceWriter("Komejane.Resource.web", rcname, webDir);
        }
      }
    }

    public void Initialize()
    {
      // 初期化完了フラグが立ってた場合は無視
      if (Initialized) return;

      InitializeWebDirectory();

      // 初期化完了フラグを立てる
      Initialized = true;
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

    public async void AddComment(CommentData comment)
    {
      if (comment.IsCommand) return;

      
      await Task.Run(() =>
      {
        Logger.Trace(comment.ToString());

        if (Http.isRun) Http.Instance.SendComment(comment);
      });
    }
  }
}
