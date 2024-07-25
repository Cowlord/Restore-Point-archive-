using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
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

            // Load restore points when the form loads
            this.Load += new EventHandler(Form1_Load);

            // Handle delete key press for listBox1
            listBox1.KeyDown += new KeyEventHandler(listBox1_KeyDown);
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
            //LoadRestorePoints();
        }

        private void LoadRestorePoints()
        {
            try
            {
                ManagementScope scope = new ManagementScope("\\\\.\\root\\default");
                ObjectQuery query = new ObjectQuery("SELECT * FROM SystemRestore");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                ManagementObjectCollection queryCollection = searcher.Get();

                listBox1.Items.Clear();
                foreach (ManagementObject m in queryCollection)
                {
                    string description = m["Description"].ToString();
                    DateTime creationTime = ManagementDateTimeConverter.ToDateTime(m["CreationTime"].ToString());
                    string restorePointInfo = $"{description,-30} {creationTime.ToString("MM/dd/yy @ HH:mm"),30}";
                    listBox1.Items.Add(restorePointInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load restore points: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && listBox1.SelectedItem != null)
            {
                DialogResult result = MessageBox.Show("This will delete all restore points. Do you want to proceed?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        // Disable system protection
                        ProcessStartInfo disableProtection = new ProcessStartInfo();
                        disableProtection.FileName = "cmd.exe";
                        disableProtection.Arguments = "/c vssadmin delete shadows /all /quiet";
                        disableProtection.WindowStyle = ProcessWindowStyle.Hidden;
                        disableProtection.Verb = "runas"; // Request elevation

                        using (Process process = Process.Start(disableProtection))
                        {
                            process.WaitForExit();

                            if (process.ExitCode == 0)
                            {
                                MessageBox.Show("Deleted all restore points.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadRestorePoints();
                            }
                            else
                            {
                                MessageBox.Show($"Failed to delete restore points. Exit code: {process.ExitCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Win32Exception ex)
                    {
                        MessageBox.Show($"Failed to run cmd: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void RestorePoint_Text_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
