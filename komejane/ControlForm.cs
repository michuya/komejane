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
    LogForm logForm;

    public ControlForm()
    {
      InitializeComponent();

      Http server = Http.Instance;

      server.ServerStarted += Server_ServerStarted;
      server.ServerStop += Server_ServerStop;

      Logger.Instance.AddLog += (sender, e) =>
      {
        if (logForm == null) return;

        logForm.addLog(e.Log);
      };
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
      if (txtUrlSample.InvokeRequired)
      {
        Invoke(new Action(() => {
          Config conf = Config.Instance;

          if (Komejane.IsServerRun)
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
        }));
      }
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void btnServRun_Click(object sender, EventArgs e)
    {
      Http server = Http.Instance;

      if (btnServStop.Enabled)
        server.serverRestart();
      else
        server.serverStart();
    }

    private void btnServStop_Click(object sender, EventArgs e)
    {
      Http server = Http.Instance;

      server.serverStop();
    }

    private void ControlForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      Http server = Http.Instance;
      server.serverStop();
    }

    private void btnShowLog_Click(object sender, EventArgs e)
    {
      logForm = new LogForm();
      logForm.FormClosed += (_sender, _e) => { logForm = null; };
      logForm.Show();
    }
  }
}
