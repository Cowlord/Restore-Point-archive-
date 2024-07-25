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
        private const string PlaceholderText = "Enter Restore Point Name...";

        public Form1()
        {
            InitializeComponent();
            RestorePoint_Text.KeyDown += new KeyEventHandler(RestorePoint_Text_KeyDown);
            RestorePoint_Text.KeyPress += new KeyPressEventHandler(RestorePoint_Text_KeyPress);
            RestorePoint_Text.Leave += new EventHandler(RestorePoint_Text_Leave);
            RestorePoint_Text.Text = PlaceholderText;
            RestorePoint_Text.ForeColor = Color.Gray;
        }

        private void Save_Button_Click(object sender, EventArgs e)
        {
            string restorePointName = RestorePoint_Text.Text;
            if (restorePointName == PlaceholderText)
            {
                MessageBox.Show("Please enter a valid restore point name.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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

        private void RestorePoint_Text_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (RestorePoint_Text.Text == PlaceholderText)
            {
                RestorePoint_Text.Text = "";
                RestorePoint_Text.ForeColor = Color.Black;
            }
        }

        private void RestorePoint_Text_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RestorePoint_Text.Text))
            {
                RestorePoint_Text.Text = PlaceholderText;
                RestorePoint_Text.ForeColor = Color.Gray;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void RestorePoint_Text_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
