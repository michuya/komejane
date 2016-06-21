using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komejane
{
  public class LoggerEventArgs : EventArgs
  {
    public LoggerData Log { get; private set; }
    public LoggerEventArgs(LoggerData log)
    {
      Log = log;
    }
  }

  /// <summary>
  /// 簡易ロガー用ログデータ
  /// </summary>
  public class LoggerData
  {
    string[] logLevelString = { "info", "debug", "trace" };

    public Logger.LogLevel Level { get; protected set; }
    public string LevelString { get { return logLevelString[(int)Level]; } }
    public DateTime Date { get; protected set; }
    public string Message { get; protected set; }

    public LoggerData(string message)
    {
      Date = DateTime.Now;
      Level = Logger.LogLevel.DEBUG;
      Message = message;
    }
    public LoggerData(Logger.LogLevel level, string message)
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
  public sealed class Logger
  {
    /* --------------------------------------------------------------------- */
    #region シングルトン関係
    /* --------------------------------------------------------------------- */
    private static Logger instance = new Logger();

    public static Logger Instance
    {
      get { return instance; }
    }
    /* --------------------------------------------------------------------- */
    #endregion
    /* --------------------------------------------------------------------- */

    /* --------------------------------------------------------------------- */
    #region イベント関係
    /* --------------------------------------------------------------------- */
    public EventHandler<LoggerEventArgs> AddLog;

    private void OnAddLog(LoggerEventArgs e)
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

    string _logdir;
    string logDirectory
    {
      get { return _logdir; }
      set {
        string dllLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        _logdir = System.IO.Path.GetDirectoryName(dllLocation) + "\\" + value;
      }
    }
    string logFile { get; set; }
    string logPath { get { return System.IO.Path.GetFullPath(logDirectory + "\\" + logFile); } }

    object lockLogWriter = new object();
    DateTime lastWriteTime = DateTime.Now;

    System.IO.StreamWriter logWriter = null;

    List<LoggerData> logs = new List<LoggerData>(1000);

    private Logger()
    {

      logDirectory = "komejane\\log";
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

        // LogWriterを用意
        lock (lockLogWriter)
        {
          if (logWriter == null)
            logWriter = System.IO.File.AppendText(logPath);

          logWriter.Write(e.Log.ToString() + Environment.NewLine);
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

        LoggerData log = new LoggerData(level, message);

        // イベント発行前にオンメモリのログへ追加
        logs.Add(log);

        // 新規ログ追加イベント発行
        OnAddLog(new LoggerEventArgs(log));
      });
    }

    public static LoggerData[] GetAllLogs()
    {
      return Logger.Instance.logs.ToArray();
    }
    public static void Info(string message)
    {
      Logger logger = Logger.Instance;
      logger.writeLine(LogLevel.INFO, message);
    }
    public static void Debug(string message)
    {
      Logger logger = Logger.Instance;
      logger.writeLine(LogLevel.DEBUG, message);
    }
    public static void Trace(string message)
    {
      Logger logger = Logger.Instance;
      logger.writeLine(LogLevel.TRACE, message);
    }
    public static void WriteLine(LogLevel level, string message)
    {
      Logger logger = Logger.Instance;
      logger.writeLine(level, message);
    }
  }
}
