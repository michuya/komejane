using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ankoPlugin2;

namespace ankome
{
    public class Ankochan : ankoPlugin2.IPlugin
    {
        Komejane.Komejane komejane = new Komejane.Komejane();
        ankoPlugin2.IPluginHost _host = null;

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
                return false;
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
        }
    }
}
