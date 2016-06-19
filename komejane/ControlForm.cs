using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Komejane
{
  public partial class ControlForm : Form
  {
    public ControlForm()
    {
      InitializeComponent();

      Http server = Http.Instance;

      server.ServerStarted += Server_ServerStarted;
      server.ServerStop += Server_ServerStop;
    }

    private void Server_ServerStop(object sender, EventArgs e)
    {
      updateServerStatus();
    }

    private void Server_ServerStarted(object sender, EventArgs e)
    {
      updateServerStatus();
    }

    public void updateServerStatus()
    {
      Config conf = Config.Instance;

      if (Komejane.isRun)
      {
        txtUrlSample.Text = "http://" + conf.ListenHost + ":" + conf.Port;
        txtUrlSample.Enabled = true;

        btnServRun.Text = "サーバ再起動";
        btnServStop.Enabled = true;
      }
      else
      {
        btnServStop.Enabled = false;

        txtUrlSample.Enabled = false;
        txtUrlSample.Text = "停止中…";

        btnServRun.Text = "サーバ起動";
      }
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void btnServRun_Click(object sender, EventArgs e)
    {
      Http server = Http.Instance;

      if (btnClose.Enabled)
        server.serverRestart();
      else
        server.serverStart();
    }

    private void btnServStop_Click(object sender, EventArgs e)
    {
      Http server = Http.Instance;

      server.serverStop();
    }
  }
}
