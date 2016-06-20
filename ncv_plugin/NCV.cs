using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin;

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
    }

    public void Run()
    {
      // TODO: 鯖インスタンスを起動
      komejane.Run();
    }
  }
}
