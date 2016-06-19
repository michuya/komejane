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

    static public bool isRun
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

    public void Run(bool isShowWindow = true)
    {
      Http server = Http.Instance;

      if (isShowWindow)
      {
        ControlForm form = new ControlForm();
        form.ShowDialog();
      }
    }

    public void Stop()
    {
      Http server = Http.Instance;
    }
  }
}
