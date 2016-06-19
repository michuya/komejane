using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komejane
{
  public class Komejane
  {
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
  }
}
