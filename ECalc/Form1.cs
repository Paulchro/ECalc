using ECalc.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace ECalc
{
    public partial class Form1 : Form
    {
       
        string fuelTypeStart = "";
        BackgroundWorker bgw = new BackgroundWorker();
     
        int dataCount = 0;
        decimal quantEnd = 0;
        decimal totalEnd = 0;
        decimal fpaEnd = 0;
        decimal netEnd = 0;
        public Form1()
        {
            InitializeComponent();
            pictureBox1.Hide();
            dateFrom.Format = DateTimePickerFormat.Custom;
            dateFrom.CustomFormat = "dd/MM/yyyy HH:mm:ss tt";
            dateTo.Format = DateTimePickerFormat.Custom;
            dateTo.CustomFormat = "dd/MM/yyyy HH:mm:ss tt";
        }

        private void calculateBtn_Click(object sender, EventArgs e)
        {
            bgw.WorkerReportsProgress = true;
            bgw.WorkerSupportsCancellation = true;
            bgw.DoWork += bgw_DoWork;
            bgw.ProgressChanged += bgw_ProgressChanged;
            bgw.RunWorkerCompleted += bgw_RunWorkerCompleted;
            if (!Directory.Exists(sourcePath.Text))
            {
                MessageBox.Show("Wrong Path", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DirectoryInfo dirInfoA = new DirectoryInfo(sourcePath.Text);
         
          
            quanTb.Text = "";
            totalTb.Text = "";
            fpaTb.Text = "";
            netTb.Text = "";
            pathBtn.Enabled = false;
            resetBtn.Enabled = false;
            calculateBtn.Enabled = false;
            fuelTypeStart = fuelType.Text;
            if (bgw.IsBusy != true)
            {
                bgw.RunWorkerAsync();
            }
            
        }
        private static int ProductSpec(string x)
        {
            int taxis = 0;
            switch (x)
            {
                case "DIESEL":
                    taxis = 20;
                    break;
                case "UNLEADED 95":
                    taxis = 10;
                    break;
                case "UNLEADED 95+":
                    taxis = 11;
                    break;
                case "UNLEADED 100":
                    taxis = 12;
                    break;
                case "LRP":
                    taxis =13;
                    break;
                case "DIESEL Premium":
                    taxis = 21;
                    break;
                case "Πετρέλαιο Θέρμανσης":
                    taxis = 30;
                    break;
                case "HEATING OIL+":
                    taxis = 31;
                    break;
                case "LPG":
                    taxis = 40;
                    break;
                case "CNG":
                    taxis = 50;
                    break;
            }
            return taxis;
        }
        public static void ResetAllControls(Control form)
        {
            foreach (Control control in form.Controls)
            {
                if (control is TextBox)
                {
                    TextBox textBox = (TextBox)control;
                    textBox.Text = null;
                }

                if (control is ComboBox)
                {
                    ComboBox comboBox = (ComboBox)control;
                    if (comboBox.Items.Count > 0)
                        comboBox.SelectedIndex = 0;
                }

                if (control is DateTimePicker)
                {
                    DateTimePicker dateTimePicker = (DateTimePicker)control;
                    dateTimePicker.Value = DateTime.Now;
                }
            }
        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            ResetAllControls(this);
        }

        private void pathBtn_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                sourcePath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            quantEnd = 0;
            totalEnd = 0;
            fpaEnd = 0;
            netEnd = 0;
          
            int temp = 0;
            pictureBox1.Invoke((MethodInvoker)delegate
            {
                pictureBox1.Show();
            });
  
            if (sourcePath.Text == String.Empty || !Directory.Exists(sourcePath.Text) || fuelTypeStart == "")
            {
                MessageBox.Show("Something is wrong, try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
          
            List<string> lastSplitedLine;

            string filepath = sourcePath.Text;
            int i = 0;
            DirectoryInfo dirInfo = new DirectoryInfo(filepath);
            
            var files = dirInfo.EnumerateFiles("*.txt", SearchOption.AllDirectories).AsParallel().Where(s=>s.FullName.EndsWith("e.txt") && s.LastWriteTime >= dateFrom.Value && s.LastWriteTime <= dateTo.Value);
            var t = files.Count();
            pictureBox1.Invoke((MethodInvoker)delegate
            {
                pictureBox1.Hide();
            });
            List<string> fileNames = new List<string>();
            foreach (var file in files)
            {
                
                string lastLin = "";
                if (bgw.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    using (StreamReader sr = new StreamReader(file.FullName))
                    {
                        lastLin = sr.ReadLine();
                    }

                    lastSplitedLine = lastLin.Trim().Split(';').ToList();
                    var ts = lastSplitedLine[3].Trim();
                    if(fuelTypeStart == "ALL" && lastSplitedLine[3].Trim() != string.Empty)
                    {
                        if (lastSplitedLine[8] == "173" && DateTime.ParseExact(lastSplitedLine[4], "yyyyMMddHHmm", System.Globalization.CultureInfo.CurrentCulture) >= dateFrom.Value && DateTime.ParseExact(lastSplitedLine[4], "yyyyMMddHHmm", System.Globalization.CultureInfo.CurrentCulture) <= dateTo.Value)
                        {
                            quantEnd += Convert.ToDecimal(lastSplitedLine[3].Replace('.', ',').Split('!').Last());
                            netEnd += Convert.ToDecimal(lastSplitedLine[13].Replace('.', ','));
                            totalEnd += Convert.ToDecimal(lastSplitedLine[20].Replace('.', ','));
                            fpaEnd += Convert.ToDecimal(lastSplitedLine[18].Replace('.', ','));
                            dataCount++;
                        }
                    }
                    else if(lastSplitedLine[3].Trim() != string.Empty)
                    {
                        if (lastSplitedLine[8] == "173" && DateTime.ParseExact(lastSplitedLine[4], "yyyyMMddHHmm", System.Globalization.CultureInfo.CurrentCulture) >= dateFrom.Value && DateTime.ParseExact(lastSplitedLine[4], "yyyyMMddHHmm", System.Globalization.CultureInfo.CurrentCulture) <= dateTo.Value && Convert.ToInt32(lastSplitedLine[3].Split('!').First().Replace('?', ' ').Trim()) == ProductSpec(fuelTypeStart))
                        {
                            quantEnd += Convert.ToDecimal(lastSplitedLine[3].Replace('.', ',').Split('!').Last());
                            netEnd += Convert.ToDecimal(lastSplitedLine[13].Replace('.', ','));
                            totalEnd += Convert.ToDecimal(lastSplitedLine[20].Replace('.', ','));
                            fpaEnd += Convert.ToDecimal(lastSplitedLine[18].Replace('.', ','));
                            dataCount++;
                            fileNames.Add(file.ToString());
                        }
                    }
                  
                    
                    int percentage = (i + 1) * 100 / t;

                    if (percentage > temp)
                    {
                        bgw.ReportProgress(percentage);
                        temp = percentage;
                        percentageLb.Invoke((MethodInvoker)delegate
                        {
                            percentageLb.Text = percentage.ToString() + " " + "%";
                        });
                    }
                  
                    i++;
                }
            }
            fileNames.Sort();
            var asd = fileNames;
        }

        private void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.PerformStep();
        }

        private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                MessageBox.Show("Operation canceled", "Stopped", MessageBoxButtons.OK, MessageBoxIcon.Information);
                pathBtn.Enabled = true;
                resetBtn.Enabled = true;
                calculateBtn.Enabled = true;
                progressBar1.Value = 0;
                BackgroundWorker bgw = sender as BackgroundWorker;
                bgw.DoWork -= bgw_DoWork;
                bgw.RunWorkerCompleted -= bgw_RunWorkerCompleted;
                bgw.Dispose();
            }
            else if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                pathBtn.Enabled = true;
                resetBtn.Enabled = true;
                calculateBtn.Enabled = true;
                progressBar1.Value = 0;
                BackgroundWorker bgw = sender as BackgroundWorker;
                bgw.DoWork -= bgw_DoWork;
                bgw.RunWorkerCompleted -= bgw_RunWorkerCompleted;
                bgw.Dispose();
            }
            else
            {
                quanTb.Invoke((MethodInvoker)delegate
                {
                    quanTb.Text = quantEnd.ToString();
                });
                totalTb.Invoke((MethodInvoker)delegate
                {
                    totalTb.Text = totalEnd.ToString();
                });
                netTb.Invoke((MethodInvoker)delegate
                {
                    netTb.Text = netEnd.ToString();
                });
                fpaTb.Invoke((MethodInvoker)delegate
                {
                    fpaTb.Text = fpaEnd.ToString();
                });
                label9.Invoke((MethodInvoker)delegate
                {
                    label9.Text = dataCount.ToString();
                });
                pathBtn.Enabled = true;
                resetBtn.Enabled = true;
                calculateBtn.Enabled = true;
                progressBar1.Value = 0;
                BackgroundWorker bgw = sender as BackgroundWorker; 
                bgw.DoWork -= bgw_DoWork; 
                bgw.RunWorkerCompleted -= bgw_RunWorkerCompleted; 
                bgw.Dispose();
            }
        }
      
        private void cancelBtn_Click(object sender, EventArgs e)
        {
            bgw.CancelAsync();
            
        }

    }
}
