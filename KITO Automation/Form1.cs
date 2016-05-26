using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace KITO_Automation
{
    public partial class Form1 : Form
    {
        const int chunkSize = 65536;
        bool running = false;
        string fpath = "";
        string fext = "";
        int[] aArr;
        int num;
        List<ZipFile> zipFiles = new List<ZipFile>();
        public Form1()
        {
            InitializeComponent();
            readKey();
        }

        private void readKey()
        {
            string filename = @"automation.cfg";
            if (File.Exists(filename))
            {
                StreamReader sr = new StreamReader("automation.cfg");
                this.keyBox.Text = sr.ReadLine();
                this.textBox1.Text = sr.ReadLine();
                this.OutputFileTextBox.Text = sr.ReadLine();
                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine();
                    DataGridViewRow row = new DataGridViewRow();
                    DataGridViewCheckBoxCell c1 = new DataGridViewCheckBoxCell();
                    DataGridViewCheckBoxCell c2 = new DataGridViewCheckBoxCell();
                    c2.Value = true;
                    DataGridViewTextBoxCell c3 = new DataGridViewTextBoxCell();
                    c3.Value = Path.GetFileName(line);
                    ZipFile zf = new ZipFile(line, row, c1, c2);
                    row.Cells.Add(c1);
                    row.Cells.Add(c2);
                    row.Cells.Add(c3);

                    dataGridView1.Rows.Add(row);
                    zipFiles.Add(zf);
                }
                sr.Close();
            }
            else
            {
                this.keyBox.Text = this.textBox1.Text;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string filename = @"automation.cfg";
            StreamWriter sw = new StreamWriter(filename);
            sw.WriteLine(this.keyBox.Text);
            sw.WriteLine(this.textBox1.Text);
            sw.WriteLine(this.OutputFileTextBox.Text);
            foreach (ZipFile zf in zipFiles)
            {
                sw.WriteLine(zf.Filename);
            }
            sw.Close();
        }

        private void InputFileButton_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.ShowDialog();
        }

        private void OutputFileButton_Click(object sender, EventArgs e)
        {
            DialogResult r = this.folderBrowserDialog1.ShowDialog();
            if (r == System.Windows.Forms.DialogResult.OK)
            {
                this.OutputFileTextBox.Text = this.folderBrowserDialog1.SelectedPath;
            }
        }

        private void DoTheMap_Click(object sender, EventArgs e)
        {
            if (!running)
            {
                if (zipFiles.Count == 0 || this.OutputFileTextBox.Text == "")
                {
                    MessageBox.Show("No zip files or no output folder");
                    return;
                }
                if (!Directory.Exists(this.OutputFileTextBox.Text))
                {
                    MessageBox.Show("Output folder does not exist?");
                    return;
                }
                string kString = this.keyBox.Text;
                if (kString.IndexOfAny(",".ToCharArray()) != -1)
                {
                    string[] sArr = kString.Split(",".ToCharArray());
                    aArr = sArr.Select(x => int.Parse(x.Trim())).ToArray();
                }
                if (aArr.Length == 0)
                {
                    MessageBox.Show("No xor keys entered.");
                    return;
                }
                num = aArr.Length - 1;
                fext = textBox1.Text;
                if (fext.Length == 0)
                {
                    MessageBox.Show("No output file extension.");
                    return;
                }
                dataGridView1.ReadOnly = true;
                bool ok = false;
                foreach (ZipFile zf in zipFiles)
                {
                    if ((bool)zf.Do.Value)
                        ok = true;
                    zf.Done.Value = false;
                }
                if (!ok)
                {
                    MessageBox.Show("Nothing to do.");
                    dataGridView1.ReadOnly = false;
                    return;
                }
                fpath = this.OutputFileTextBox.Text;
                running = true;
                this.DoTheMap.Text = "STOP";
                backgroundWorker1.RunWorkerAsync();
            }
            else
            {
                this.DoTheMap.Text = "STOPPING";
                running = false;
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            for (int i = 0; i < openFileDialog1.FileNames.Length; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                DataGridViewCheckBoxCell c1 = new DataGridViewCheckBoxCell();
                DataGridViewCheckBoxCell c2 = new DataGridViewCheckBoxCell();
                c2.Value = true;
                DataGridViewTextBoxCell c3 = new DataGridViewTextBoxCell();
                c3.Value = openFileDialog1.SafeFileNames[i];
                ZipFile zf = new ZipFile(this.openFileDialog1.FileNames[i], row, c1, c2);
                row.Cells.Add(c1);
                row.Cells.Add(c2);
                row.Cells.Add(c3);

                dataGridView1.Rows.Add(row);
                zipFiles.Add(zf);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            byte[] chunk;
            int fsize;
            try
            {
                foreach (ZipFile zp in zipFiles)
                {
                    if (((bool)zp.Do.Value) == false) continue;
                    if (!running) break;
                    FileStream fsr = new FileStream(zp.Filename, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fsr);
                    FileStream fsw = new FileStream(fpath + "\\" + Path.GetFileNameWithoutExtension(zp.Filename) + "." + fext, FileMode.Create, FileAccess.Write);
                    BinaryWriter bw = new BinaryWriter(fsw);
                    zp.SizeTotal = (int)fsr.Length;
                    fsize = (int)fsr.Length;
                    backgroundWorker1.ReportProgress(0, zp);
                    chunk = br.ReadBytes(chunkSize);
                    int i = 0;
                    int y = 0;
                    zp.SizeDone = chunk.Length;
                    while (chunk.Length > 0)
                    {
                        if (!running) break;
                        for (i = 0; i < chunk.Length; i++)
                        {
                            /*if (11987456 == (y + i))
                                i = i;*/
                            chunk[i] ^= (byte)(aArr[(y + i) & num]);
                        }
                        bw.Write(chunk);
                        y += chunk.Length;
                        chunk = br.ReadBytes(chunkSize);
                        zp.SizeDone += chunk.Length;
                        backgroundWorker1.ReportProgress(1, zp);
                    }
                    backgroundWorker1.ReportProgress(2, zp);
                    bw.Flush();
                    bw.Close();
                    fsw.Close();
                    br.Close();
                    fsr.Close();
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("Caught an error: " + ee.Message);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ZipFile zf = (ZipFile)e.UserState;
            switch (e.ProgressPercentage)
            {
                case 0:
                    tLineBox.Text = zf.SizeTotal.ToString();
                    uLineBox.Text = zf.SizeDone.ToString();
                    break;
                case 1:
                    uLineBox.Text = zf.SizeDone.ToString();
                    break;
                case 2:
                    uLineBox.Text = zf.SizeDone.ToString();
                    zf.Done.Value = true;
                    break;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.dataGridView1.ReadOnly = false;
            this.DoTheMap.Enabled = true;
            running = false;
            this.DoTheMap.Text = "DO IT";
        }

        private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            ZipFile _zf = null;
            foreach (ZipFile zf in zipFiles)
            {
                if (zf.Row == e.Row)
                    _zf = zf;
            }
            if (_zf != null)
                zipFiles.Remove(_zf);
        }
    }

    class ZipFile
    {
        public string Filename;
        public DataGridViewRow Row;
        public DataGridViewCheckBoxCell Done;
        public DataGridViewCheckBoxCell Do;
        public int SizeTotal;
        public int SizeDone;

        public ZipFile(string Filename, DataGridViewRow Row, DataGridViewCheckBoxCell c1, DataGridViewCheckBoxCell c2)
        {
            this.Filename = Filename;
            this.Row = Row;
            this.Done = c1;
            this.Do = c2;
            this.SizeTotal = 0;
            this.SizeDone = 0;
        }
    }
}
