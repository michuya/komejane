using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Komejane.Server.Controller
{
  public class DefaultController : BasicController
  {
    string WebRootDirectory { get; set; }
    string WebIndex { get; set; }

    /// <summary>
    /// 拡張子ごとのMIME辞書。辞書登録する拡張子には「.」を含めない事。
    /// </summary>
    public Dictionary<string, string> MimeDictionary { get; set; }

    public DefaultController(string webroot, string webindex) : base()
    {
      WebRootDirectory = webroot;
      WebIndex = webindex;
    }

    public override void index(HttpListenerContext context)
    {
      HttpListenerRequest req = context.Request;
      HttpListenerResponse res = context.Response;

      // リクエストのローカルパスを組み立て
      string requestURI = Path.Combine(WebRootDirectory, req.Url.AbsolutePath.Substring(1).Replace('/', '\\'));
      Logger.Debug("RequestURI: " + requestURI);

      // ディレクトリだった場合はインデックスのファイルを追加
      if (Directory.Exists(requestURI))
      {
        requestURI = Path.Combine(requestURI, WebIndex);

        Logger.Debug("RequestURI(Append Index): " + requestURI);
      }

      // ファイルが存在する場合はファイルを返す
      try
      {
        if (!FileResponse(res, requestURI))
        {
          // FileResponseが失敗した場合は404
          res.StatusCode = 404;
        }
      }
      catch (Exception)
      {
        // FileResponseが例外だした場合も404
        res.StatusCode = 404;
      }

      // ストリームの最後の処理はBasicControllerに任せる
      base.index(context);
    }

    protected bool FileResponse(HttpListenerResponse res, string filepath)
    {
      // 指定したファイルがファイルじゃなかったら失敗
      if (!File.Exists(filepath)) return false;

      try
      {
        FileInfo info = new FileInfo(filepath);

        // MimeTypeを取得
        res.ContentType = GetMimeType(filepath);

        // ファイルサイズを設定
        res.ContentLength64 = info.Length;

        // ファイル読み込み用ストリームから直接レスポンス用のストリームへコピー
        FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        fs.CopyTo(res.OutputStream);

        return true;
      }
      catch (Exception ex)
      {
        Logger.Info(ex.ToString());

        // 例外発生しても失敗。ここで出るのだいたいIOExceptionだし。
        return false;
      }
    }

    protected string GetMimeType(string filepath)
    {
      FileInfo info = new FileInfo(filepath);

      string ext = info.Extension.ToLower().TrimStart(".".ToCharArray());

      // デフォルト値はoctet-streamにしておく
      string mimeType = "application/octet-stream";

      try
      {
        if (MimeDictionary.ContainsKey(ext))
        {
          // 拡張子MIME辞書から取得
          mimeType = MimeDictionary[ext];
          Logger.Trace("MimeType(fromExtention): " + mimeType);
        }
        else
        {
          // ファイル内容からMIMEを推定
          mimeType = FileUtility.FindMimeFromFile(filepath);
          Logger.Trace("MimeType(fromFile): " + mimeType);
        }
      }
      catch (Exception ex)
      {
        Logger.Info(ex.ToString());

        // 例外が発生した場合は判定失敗とみなしてoctet-streamを返す
        return "application/octet-stream";
      }

      return mimeType;
    }
  }
}
