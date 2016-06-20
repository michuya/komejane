using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komejane
{
  public class HttpLoggerEventArgs : EventArgs
  {
    public HttpLoggerData Log { get; private set; }
    public HttpLoggerEventArgs(HttpLoggerData log)
    {
      Log = log;
    }
  }

  /// <summary>
  /// 簡易ロガー用ログデータ
  /// </summary>
  public class HttpLoggerData
  {
    string[] logLevelString = { "info", "debug", "trace" };

    public HttpLogger.LogLevel Level { get; protected set; }
    public string LevelString { get { return logLevelString[(int)Level]; } }
    public DateTime Date { get; protected set; }
    public string Message { get; protected set; }

    public HttpLoggerData(string message)
    {
      Date = DateTime.Now;
      Level = HttpLogger.LogLevel.DEBUG;
      Message = message;
    }
    public HttpLoggerData(HttpLogger.LogLevel level, string message)
    {
      Date = DateTime.Now;
      Level = level;
      Message = message;
    }

    public override string ToString()
    {
      return "[" + (new DateTimeOffset(Date)).ToString("yyyy/MM/dd HH:mm:ss K") + "] [" + LevelString + "] " + Message;
    }
  }

  /// <summary>
  /// 簡易ロガー
  /// </summary>
  public sealed class HttpLogger
  {
    /* --------------------------------------------------------------------- */
    #region シングルトン関係
    /* --------------------------------------------------------------------- */
    private static HttpLogger instance = new HttpLogger();

    public static HttpLogger Instance
    {
      get { return instance; }
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region イベント関係
    /* --------------------------------------------------------------------- */
    public EventHandler<HttpLoggerEventArgs> AddLog;

    private void OnAddLog(HttpLoggerEventArgs e)
    {
      if (AddLog != null)
        AddLog(this, e);
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    public enum LogLevel
    {
      INFO,
      DEBUG,
      TRACE
    }

    string logDirectory { get; set; }
    string logFile { get; set; }

    object lockLogWriter = new object();
    DateTime lastWriteTime = DateTime.Now;

    System.IO.StreamWriter logWriter = null;

    List<HttpLoggerData> logs = new List<HttpLoggerData>(1000);

    private HttpLogger()
    {
      logDirectory = ".\\komejane\\log";
      logFile = "access.log";

      // ログ追記イベント
      AddLog += (sender, e) =>
      {
        // ログディレクトリの作成
        if (!System.IO.Directory.Exists(logDirectory))
        {
          System.IO.Directory.CreateDirectory(logDirectory);
        }
        // TODO: ログファイルの容量 or 日付をトリガーにファイルの切り替えを行う

        string logPath = System.IO.Path.GetFullPath(logDirectory + "\\" + logFile);

        // LogWriterを用意
        lock (lockLogWriter)
        {
          if (logWriter == null)
            logWriter = System.IO.File.AppendText(logPath);

          logWriter.WriteAsync(e.Log.ToString() + Environment.NewLine);
          lastWriteTime = DateTime.Now;
        }

        // 5秒使ってなかったらストリームを閉じる
        System.Timers.Timer timer = new System.Timers.Timer(5000);
        timer.AutoReset = false;
        timer.Elapsed += (_sender, _e) =>
        {
          if ((DateTime.Now - lastWriteTime).TotalMilliseconds >= 4900)
          {
            lock (lockLogWriter)
            {
              System.Diagnostics.Debug.WriteLine("log writer dispose");
              if (logWriter != null) logWriter.Dispose();
              logWriter = null;
            }
          }
        };
        timer.Start();
      };
    }

    public void writeLine(string message)
    {
      writeLine(LogLevel.DEBUG, message);
    }
    public async void writeLine(LogLevel level, string message)
    {
      await Task.Run(() => {

        HttpLoggerData log = new HttpLoggerData(level, message);

        // イベント発行前にオンメモリのログへ追加
        logs.Add(log);

        // 新規ログ追加イベント発行
        OnAddLog(new HttpLoggerEventArgs(log));
      });
    }

    public static void Info(string message)
    {
      HttpLogger logger = HttpLogger.Instance;
      logger.writeLine(LogLevel.INFO, message);
    }
    public static void Debug(string message)
    {
      HttpLogger logger = HttpLogger.Instance;
      logger.writeLine(LogLevel.DEBUG, message);
    }
    public static void Trace(string message)
    {
      HttpLogger logger = HttpLogger.Instance;
      logger.writeLine(LogLevel.TRACE, message);
    }
    public static void WriteLine(LogLevel level, string message)
    {
      HttpLogger logger = HttpLogger.Instance;
      logger.writeLine(level, message);
    }
  }
}
