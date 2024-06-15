using System;
using System.Windows.Forms;
using Renci.SshNet;
using System.Threading;

namespace SSH_Window
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            string[] switchIPs = txtSwitchIPs.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string enablePassword = txtEnablePassword.Text;
            string[] commands = txtCommands.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var ip in switchIPs)
            {
                ExecuteCommandsOnSwitch(ip, username, password, enablePassword, commands);
            }
        }

        private void ExecuteCommandsOnSwitch(string ip, string username, string password, string enablePassword, string[] commands)
        {
            try
            {
                using (var client = new SshClient(ip, username, password))
                {
                    client.Connect();

                    if (client.IsConnected)
                    {
                        AppendOutput($"Connected to switch {ip}.");

                        using (var shellStream = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024))
                        {
                            WriteToShellStream(shellStream, "enable");
                            Thread.Sleep(2000);
                            var response = ReadFromShellStream(shellStream);
                            AppendOutput(response);

                            WriteToShellStream(shellStream, enablePassword);
                            Thread.Sleep(2000);
                            response = ReadFromShellStream(shellStream);
                            AppendOutput(response);

                            foreach (var command in commands)
                            {
                                WriteToShellStream(shellStream, command);
                                Thread.Sleep(2000);
                                response = ReadFromShellStream(shellStream);
                                AppendOutput($"Command: {command}\nResult: {response}\n");
                            }
                        }

                        AppendOutput("Finished executing commands.");
                    }
                    else
                    {
                        AppendOutput($"Failed to connect to switch {ip}.");
                    }
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"An error occurred with switch {ip}: {ex.Message}");
            }
        }

        private void WriteToShellStream(ShellStream stream, string command)
        {
            stream.WriteLine(command);
            stream.Flush();
        }

        private string ReadFromShellStream(ShellStream stream)
        {
            string response = string.Empty;
            while (stream.DataAvailable)
            {
                response += stream.Read();
            }
            return response;
        }

        private void AppendOutput(string text)
        {
            txtOutput.AppendText(text + Environment.NewLine);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
