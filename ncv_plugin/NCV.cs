using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin;
using UserType = Komejane.CommentData.UserType;

namespace ncv_plugin
{
  public class NCV : Plugin.IPlugin
  {
    Komejane.Komejane komejane = new Komejane.Komejane();
    Plugin.IPluginHost _host = null;

    public IPluginHost Host
    {
      get
      {
        return this._host;
      }

      set
      {
        this._host = value;
      }
    }

    public string Name
    {
      get
      {
        return Komejane.Komejane.Name;
      }
    }

    public string Description
    {
      get
      {
        return Komejane.Komejane.Description + "\nNCV Ver.";
      }
    }

    public string Version
    {
      get
      {
        return Komejane.Komejane.Version;
      }
    }

    public bool IsAutoRun
    {
      get
      {
        return false;
      }
    }

    public void AutoRun()
    {
      // TODO: 鯖インスタンスをサイレント起動
      Run();
    }

    public void Run()
    {
      komejane.Initialize();

      // TODO: 鯖インスタンスを起動
      komejane.WindowOwner = _host.MainForm;
      komejane.Run();

      _host.ReceivedComment += _host_ReceivedComment;
    }

    private void _host_ReceivedComment(object sender, ReceivedCommentEventArgs e)
    {
      try
      {
        foreach (var Chat in e.CommentDataList)
        {
          UserType type = UserType.Member;

          if (Chat.IsBSP) type = UserType.Bsp;

          int commentNo = -1;
          try { commentNo = int.Parse(Chat.No); } catch(Exception ex) { Komejane.Logger.Debug(ex.ToString()); commentNo = -1; }

          komejane.AddComment(new Komejane.CommentData(
            commentNo,
            Chat.Comment,
            type,
            Chat.UserId,
            Chat.Name,
            Chat.IsAnonymity,
            Chat.Premium
          ));
        }
      } catch (Exception ex)
      {
        Komejane.Logger.Debug(ex.ToString());
      }
    }
  }
}
