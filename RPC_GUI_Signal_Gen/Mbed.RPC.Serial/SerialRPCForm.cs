using org.mbed.RPC;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace Mbed.RPC.Serial
{
    public partial class SerialRPCForm : Form
    {
        private static System.Timers.Timer aTimer;
        RPCVariable<int> mode;
        RPCVariable<int> frequency;
        float commStatus;
        int mode_iter, freq_iter;
        string selectedPort;
        SerialRPC _serialRPC;    // mbed rpc handle
        public BackgroundWorker backgroundWorker1 = new BackgroundWorker();

        public SerialRPCForm()
        {
            InitializeComponent();
            //get list of active ports on the computer
            string[] ports = SerialPort.GetPortNames();
            serialComboBox.Items.AddRange(ports);
            trackBar1.Enabled = false;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int i = 0;
            while (true)
            {
                Thread.Sleep(250);
                backgroundWorker1.ReportProgress(i);
                i++;
                if (i == 9999) i = 0;
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int temp_freq = freq_iter; 
            int temp_mode = mode_iter;
            if (freq_iter >= 0)
            {
                temp_freq = freq_iter;
                temp_mode = mode_iter;
            }
            else
            {
                temp_freq = mode_iter;
                temp_mode = freq_iter;
            }

            freq_box.Text = temp_freq.ToString();
            freq_box.Refresh();

            if (temp_mode == -1) // sine
            {
                sine_alert.BackColor = Color.Lime;
                triangle_alert.BackColor = Color.FromArgb(192, 0, 0);
                square_alert.BackColor = Color.FromArgb(192, 0, 0);
            }
            else if (temp_mode == -2) // square
            {
                sine_alert.BackColor = Color.FromArgb(192, 0, 0);
                triangle_alert.BackColor = Color.FromArgb(192, 0, 0);
                square_alert.BackColor = Color.Lime;
            }
            else if (temp_mode == -3) // triangle
            {
                sine_alert.BackColor = Color.FromArgb(192, 0, 0);
                triangle_alert.BackColor = Color.Lime;
                square_alert.BackColor = Color.FromArgb(192, 0, 0);
            }

            sine_alert.Refresh();
            square_alert.Refresh();
            triangle_alert.Refresh();

            Debug.Print("Freq_iter: " + temp_freq);
            Debug.Print("mode: " + temp_mode);

            trackBar1.Invoke(new Action(() => trackBar1.Value = (int)temp_freq));
        }

        private void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(50);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void SerialRPCForm_Load(object sender, EventArgs e)
        {
            //disable controls until com port is connected
            groupBox3.Enabled = false;
            groupBox4.Enabled = false;
            groupBox1.Enabled = false;

            //initialize status variables
            statusLabel.Text = "Not connected!";
        }

        private void serialComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedPort = serialComboBox.SelectedItem.ToString();

            try
            {
                //Create an mbed object for communication over USB (serial)
                _serialRPC = new SerialRPC(selectedPort, 9600);

                //GetObjects a = new GetObjects(_serialRPC);

                // Signal Generator Mode (0-sine, 1-square, 2-triangle)
                mode = new RPCVariable<int>(_serialRPC, "MODE");
                frequency = new RPCVariable<int>(_serialRPC, "FREQ");
                SetTimer();

                //enable controls after com port is connected
                groupBox3.Enabled = true;
                groupBox1.Enabled = true;
                groupBox4.Enabled = true;

                //MessageBox.Show(selectedPort +" connected to Mbed", "Mbed Connected!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                commStatus = 1;
                serialComboBox.Enabled = false;
                statusLabel.Text = "Mbed connected to " + selectedPort;
                backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
                backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler( backgroundWorker1_ProgressChanged);
                backgroundWorker1.WorkerReportsProgress = true;
                backgroundWorker1.RunWorkerAsync();


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                commStatus = 0;
                
                if (_serialRPC != null) _serialRPC.delete();
                
                //disable controls if mbed is disconnected
                groupBox3.Enabled = false;
                groupBox1.Enabled = false;
                groupBox4.Enabled = false;

                statusLabel.Text = "Not connected!";
            }
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e) {
            string[] freq_raw = frequency.read_String().Split(' ');
            string[] mode_raw = mode.read_String().Split(' ');

            // Adjust the trackbar
            if (Regex.Matches(mode_raw[0], @"[a-zA-Z]").Count == 0 && (mode_raw[0].Length > 0))
            {
                mode_iter = int.Parse(mode_raw[0], CultureInfo.InvariantCulture.NumberFormat);
            }

            if (Regex.Matches(freq_raw[0], @"[a-zA-Z]").Count == 0 && (freq_raw[0].Length > 0))
            {
                freq_iter = int.Parse(freq_raw[0], CultureInfo.InvariantCulture.NumberFormat);
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            try
            {
                //delete objects before exit
                if (_serialRPC != null) _serialRPC.delete();

                Debug.Print("Complete");
            }
            catch (NullReferenceException ex)
            {
                Debug.Print("No Reference: " + ex.Message);
            }
            this.Close();
        }
    }
}