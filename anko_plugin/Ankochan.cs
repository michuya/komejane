using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ankoPlugin2;
using UserType = Komejane.CommentData.UserType;

namespace anko_plugin
{
  public class Ankochan : ankoPlugin2.IPlugin
  {
    Komejane.Komejane komejane = new Komejane.Komejane();
    ankoPlugin2.IPluginHost _host = null;

    bool isFirstReciveChat = false;
    object lockFirstRecive = new object();

    public IPluginHost host
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

    public string Description
    {
      get
      {
        return Komejane.Komejane.Description + "\nアンコちゃん Ver.";
      }
    }

    public bool IsAlive
    {
      get
      {
        return komejane.IsAlive;
      }
    }

    public string Name
    {
      get
      {
        return Komejane.Komejane.Name;
      }
    }

    public void Run()
    {
      // TODO: 鯖インスタンスを起動
      komejane.WindowOwner = (System.Windows.Forms.Form)_host.Win32WindowOwner;
      komejane.Run();

      _host.ReceiveChat += _host_ReceiveChat;
      _host.ConnectedServer += (sender, e) => { isFirstReciveChat = true; };
      _host.DisconnectedServer += (sender, e) => { isFirstReciveChat = false; };
    }

    private void addComment(LibAnko.chat chat)
    {
      var user = chat.userinfo;

      UserType type = UserType.Member;

      if (chat.IsCaster) type = UserType.Caster;
      else if (chat.IsBSP) type = UserType.Bsp;
      else if (user != null && user.isMember) type = UserType.Member;
      else type = UserType.Normal;

      komejane.AddComment(new Komejane.CommentData(
        chat.No,
        chat.Message,
        type,
        chat.UserId,
        chat.NickName,
        chat.Anonymity,
        chat.Premium
      ));
    }

    private void _host_ReceiveChat(object sender, ReceiveChatEventArgs e)
    {
      if (!IsAlive) return;

      lock (lockFirstRecive)
      {
        if (isFirstReciveChat && e.Chat.No > 1)
        {
          foreach (var chat in _host.Chats) addComment(chat);

          isFirstReciveChat = false;
        }
      }

      addComment(e.Chat);
    }
  }
}
