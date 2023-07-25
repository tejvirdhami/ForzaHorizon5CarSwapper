using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Memory;

namespace ForzaCarSwapper
{
    public partial class MainForm : Form
    {
        public Mem m = new Mem();
        private IEnumerable<long> Addresses = null;
        public MainForm()
        {
            InitializeComponent();
            btnSwap.Enabled = false;
            txtSwapId.Enabled = false;
        }

        bool ProcOpen = false;

        private void BGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProcOpen = m.OpenProcess("ForzaHorizon5.exe");
   
                if(!ProcOpen)
                    {
                    Thread.Sleep(1000);
                    return;
                }

                Thread.Sleep(1000);
                BGWorker.ReportProgress(0);
   
        }

        private void updateScanLabel(string message)
        {
            if (lblScanStatus.InvokeRequired)
            {
                lblScanStatus.BeginInvoke((MethodInvoker)(() => updateScanLabel(message)));
            }
            else
            {
                lblScanStatus.Text = message;
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            BGWorker.RunWorkerAsync();
        }

        private void BGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(ProcOpen)
            {
                ProcOpenLabel.Text = "Game Found";
                ProcOpenLabel.ForeColor = Color.Green;
            }
        }

        private void BGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BGWorker.RunWorkerAsync();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            updateScanLabel("");
            Addresses = null;

            Thread ScanThread = new Thread(async () =>
            {
                try
                {
                    updateScanLabel("Scanning...");
                    Addresses = await m.AoBScan("BB 0B 00 00 00 00 00 00 02 00 00 00 00 00 00 00 00", true, true, false);
                }
                catch { }

                if (Addresses != null)
                {
                    if (btnSwap.InvokeRequired)
                    {
                        btnSwap.BeginInvoke((MethodInvoker)(() => {
                            updateScanLabel("Viper found!");
                            lblScanStatus.ForeColor = Color.Green;
                            btnSwap.Enabled = true;
                            txtSwapId.Enabled = true;
                        }));
                    }
                    else
                    {
                        updateScanLabel("Viper found!");
                        lblScanStatus.ForeColor = Color.Green;
                        btnSwap.Enabled = true;
                        txtSwapId.Enabled = true;
                    }
                }
                else
                    updateScanLabel("Failed to find Viper!");
                lblScanStatus.ForeColor = Color.Red;
            });
            ScanThread.Start();
        }

        private void btnSwap_Click(object sender, EventArgs e)
        {
            if (Regex.IsMatch(txtSwapId.Text, @"^[0-9]+$") && Addresses != null)
            {
                foreach (long addr in Addresses)
                    m.WriteMemory(addr.ToString("X"), "2bytes", txtSwapId.Text);

                lblScanStatus.ForeColor = Color.Green;
                updateScanLabel("Swap Successful!");
            }
            else if (Addresses == null)
            {
                lblScanStatus.ForeColor = Color.Red;
                updateScanLabel("Swap Failed!");
                MessageBox.Show("Scan Needed", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
                MessageBox.Show("Numbers Only", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void txtSwapId_TextChanged(object sender, EventArgs e)
        {
            updateScanLabel("N/A");
            lblScanStatus.ForeColor= Color.Black;
        }

        private void label3_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "https://github.com/tejvirdhami");
        }
    }
}
