using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Restore_Point
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            RestorePoint_Text.KeyDown += new KeyEventHandler(RestorePoint_Text_KeyDown);
        }

        private void Save_Button_Click(object sender, EventArgs e)
        {
            string restorePointName = RestorePoint_Text.Text;
            string command = $"/c wmic.exe /Namespace:\\\\root\\default Path SystemRestore Call CreateRestorePoint \"{restorePointName}\", 100, 7";

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = command;
            psi.WindowStyle = ProcessWindowStyle.Minimized;
            psi.Verb = "runas"; // Request elevation

            try
            {
                // Minimize the form before starting the process
                this.WindowState = FormWindowState.Minimized;

                Process process = Process.Start(psi);
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Application.Exit();
                }
                else
                {
                    MessageBox.Show("Failed to create restore point.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show($"Failed to run cmd: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestorePoint_Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevent the beep sound
                Save_Button_Click(sender, e);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
