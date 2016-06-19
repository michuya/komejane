using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ankoPlugin2;

namespace ankome
{
    public class Komejane : ankoPlugin2.IPlugin
    {
        String pluginName = "こめじゃね";
        String pluginDescription = "OBS CLR Plugin/Obs Studio Browser Plugin用HTML5コメントジェネレータ";

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
                return this.pluginDescription;
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
                return this.pluginName;
            }
        }

        public void Run()
        {
            // 鯖インスタンスを起動
        }
    }
}
