using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
namespace Cizelgeleme
{
    public partial class MainForm : Form
    {

        #region --------Variables--------

        InputBoxProvider provider = new InputBoxProvider();
        Panel boxPanel;
        Random rnd;
        int mac, proc, job;
        bool best;
        GeneticMachine genetik;
        #endregion

        #region --------Related to Form--------

        public MainForm()
        {
            InitializeComponent();
            openFileDialog.Filter = "Xml Dosyası | *.xml";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.FileName = "sch_data.xml";
            textBox1.Text = Path.Combine(openFileDialog.InitialDirectory, "sch_data.xml");
            btnPrepare.Click += btnPrepare_Click;
            btnHideBoxes.Click += btnHideBoxes_Click;
            btnStart.Click += btnStart_Click;
            btnStop.Click += btnStop_Click;
            btnHideRes.Click += btnHideRes_Click;
            panel1.Paint += panel1_Paint;
            btnClearBoxes.Click += btnClearBoxes_Click;
            btnRandom.Click += btnRandom_Click;
            btnSampleData.Click += btnSampleData_Click;
            this.SizeChanged += MainForm_SizeChanged;
            btnExport.Click += btnExport_Click;
            btnStop.Enabled = false;
            resPanel.VisibleChanged += resPanel_VisibleChanged;
            rnd = new Random();
            SetDoubleBuffered(resPanel);
            SetDoubleBuffered(panel1);
            cbCOTypes.SelectedIndex = 2;
            cbSelTypes.SelectedIndex = 0;
            cbMutTypes.SelectedIndex = 0;
            resPanel.AutoScroll = true;
            resPanel.MouseEnter += resPanel_MouseEnter;
            resPanel.Height = this.Height - btnHideRes.Top - 40;
            this.Load += MainForm_Load;
            btnExportXml.Click += btnExportXml_Click;
            btnLoadXml.Click += btnLoadXml_Click;
            btnSelectPath.Click += btnSelectPath_Click;
        }

        void btnSelectPath_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                textBox1.Text = openFileDialog.FileName;
        }

        void btnLoadXml_Click(object sender, EventArgs e)
        {
            ImportDataFromXml(textBox1.Text);
        }

        void btnExportXml_Click(object sender, EventArgs e)
        {
            ExportDataToXML(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\sch_data.xml");
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            btnPrepare.PerformClick();
        }


        void chkFuzzy_CheckedChanged(object sender, EventArgs e)
        {
            btnPrepare.PerformClick();
        }

        void btnExport_Click(object sender, EventArgs e)
        {
            if (genetik != null && !genetik.Stopped)
                return;
            this.ExportMathModel();
        }

        void resPanel_MouseEnter(object sender, EventArgs e)
        {
            resPanel.Focus();
        }


        void btnSampleData_Click(object sender, EventArgs e)
        {
            nmudjob.Value = 5;
            nmudproc.Value = 4;
            nmudmac.Value = 3;
            createSampleData();
        }

        void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (boxPanel != null)
            {
                boxPanel.Width = this.Width - boxPanel.Left - 40;
            }
            if (resPanel != null)
            {
                if (boxPanel != null)
                    resPanel.Width = boxPanel.Width;
                resPanel.Height = this.Height - btnHideRes.Top - 60;
            }
        }
        #endregion

        #region --------Visibility--------

        void resPanel_VisibleChanged(object sender, EventArgs e)
        {
            btnHideRes.Text = resPanel.Visible ? "<<" : ">>";
        }
        void boxPanel_VisibleChanged(object sender, EventArgs e)
        {
            btnHideBoxes.Text = boxPanel.Visible ? "<<" : ">>";
        }

        #endregion

        #region --------Paint--------

        void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (genetik != null && genetik.Best != null)
            {
                genetik.Best.DrawSchedule(e.Graphics, panel1);
            }
        }
        #endregion

        #region --------Click Events--------
        void btnRandom_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < mac + 1; i++)
            {
                for (int j = 1; j < proc + 1; j++)
                {
                    for (int k = 1; k < job + 1; k++)
                    {
                        string name = "j" + k + "p" + j + "m" + i;
                        int a = rnd.Next(0, 151);
                        boxPanel.Controls["j" + k].Controls["boxer"].Controls[name].Text = (a < 20 ? "X" : a.ToString());
                    }
                }
            }
        }
        void btnClearBoxes_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < mac + 1; i++)
            {
                for (int j = 1; j < proc + 1; j++)
                {
                    for (int k = 1; k < job + 1; k++)
                    {
                        string name = "j" + k + "p" + j + "m" + i;
                        boxPanel.Controls["j" + k].Controls["boxer"].Controls[name].Text = "";
                    }
                }
            }
        }
        void btnHideRes_Click(object sender, EventArgs e)
        {
            resPanel.Visible = !resPanel.Visible && best;
        }

        void btnStop_Click(object sender, EventArgs e)
        {
            genetik.Stop();
            btnExport.Enabled = true;
            btnStop.Enabled = false;
        }
        Stopwatch stp = new Stopwatch();
        void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                btnExport.Enabled = false;
                Data.DataTable = getDatas();
                Colors.GenerateRandomHSV(job);
                if (genetik != null && !genetik.Stopped)
                {
                    genetik.Stop();
                    Thread.Sleep(100);
                }

                int popSize = popnmud.Value.ToInt();
                genetik = new GeneticMachine(job, proc, mac, popSize);

                genetik.MutOdd = mutnmud.Value.ToInt();
                genetik.GroupSize = groupnmud.Value.ToInt();
                genetik.MinTimeOdd = nmudMinTime.Value.ToInt();

                genetik.SelectionType = (SelectionTypes)cbSelTypes.SelectedIndex;
                genetik.CrossOver = (COTypes)cbCOTypes.SelectedIndex;
                genetik.MutationTypes = (MutationTypes)cbMutTypes.SelectedIndex;

                genetik.Refresh = chkRefresh.Checked;
                genetik.BestValueChanged += genetik_BestValueChanged;
                genetik.ProgressChanged += genetik_ProgressChanged;
                best = true;
                resPanel.Visible = true;
                resPanel.Width = boxPanel.Width;
                resPanel.Height = this.Height - btnHideRes.Top - 60;
                boxPanel.Visible = false;
                stp.Reset();
                stp.Start();
                genetik.Start();
                btnStop.Enabled = true;
                btnHideRes.Text = "<<";
            }
            catch
            {
                MessageBox.Show("Veri tablosu hatalı");
            }
        }
        void btnHideBoxes_Click(object sender, EventArgs e)
        {
            if (boxPanel != null)
                boxPanel.Visible = !boxPanel.Visible;
        }

        void btnPrepare_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            this.Controls.Remove(boxPanel);
            if (boxPanel != null)
                boxPanel.Dispose();

            mac = nmudmac.Value.ToInt();
            proc = nmudproc.Value.ToInt();
            job = nmudjob.Value.ToInt();

            boxPanel = provider.CreateJobBoxes(mac, proc, job);
            Application.DoEvents();
            boxPanel.Left = btnHideBoxes.Right + 10;
            boxPanel.Top = btnHideBoxes.Top;
            boxPanel.Width = this.Width - boxPanel.Left - 40;
            boxPanel.AutoScroll = false;
            boxPanel.VerticalScroll.Enabled = false;
            boxPanel.VerticalScroll.Visible = false;
            boxPanel.VerticalScroll.Maximum = 0;
            boxPanel.AutoScroll = true;

            boxPanel.VisibleChanged += boxPanel_VisibleChanged;
            this.Controls.Add(boxPanel);
            SetDoubleBuffered(boxPanel);
            resPanel.Visible = false;
            Cursor = Cursors.Arrow;
        }
        #endregion

        #region --------Genetic Events--------

        void genetik_ProgressChanged(object sender, EventArgs e)
        {
            if (genetik != null)
                lblProgress.Text = "%" + genetik.Progress.ToString("0.00");
        }
        //296
        void genetik_BestValueChanged(object sender, EventArgs e)
        {
            if (genetik.Best != null)
            {
                lblSpan.Text = genetik.Best.MakeSpan().ToString();
                lblBestTime.Text = stp.Elapsed.TotalSeconds.ToString("00") + "." + stp.Elapsed.Milliseconds.ToString("00") + " sn";
                lblIdleTime.Text = genetik.Best.TotalIdleTime.ToString();
                resPanel.Invalidate();
            }
        }
        #endregion

        #region --------Methods--------

        float[, ,] getDatas()
        {
            float[, ,] vals = new float[job, proc, mac];
            for (int i = 1; i < mac + 1; i++)
            {
                for (int j = 1; j < proc + 1; j++)
                {
                    for (int k = 1; k < job + 1; k++)
                    {
                        float span = 0;
                        for (int z = 0; z < 3; z++)
                        {
                            int val = 0;
                            string text = getBoxControl(boxPanel, k, j, i).Text;
                            if (text.ToLower() == "x" || string.IsNullOrEmpty(text))
                                val = 10000;
                            else
                                val = text.ToInt();
                            span += val;
                        }
                        vals[k - 1, j - 1, i - 1] = span / 3;
                    }
                }
            }
            return vals;
        }
        Control getBoxControl(Panel boxContainer, int job, int proc, int mac)
        {
            string name = "j" + job + "p" + proc + "m" + mac;
            return boxContainer.Controls["j" + job].Controls["boxer"].Controls[name];
        }

        void createSampleData()
        {
            this.Controls.Remove(boxPanel);
            if (boxPanel != null)
                boxPanel.Dispose();

            mac = 3;
            proc = 4;
            job = 5;

            boxPanel = provider.CreateJobBoxes(mac, proc, job);
            boxPanel.Left = btnHideBoxes.Right + 10;
            boxPanel.Top = btnHideBoxes.Top;
            boxPanel.Width = this.Width - boxPanel.Left - 40;
            boxPanel.AutoScroll = false;
            boxPanel.VerticalScroll.Enabled = false;
            boxPanel.VerticalScroll.Visible = false;
            boxPanel.VerticalScroll.Maximum = 0;
            boxPanel.AutoScroll = true;

            boxPanel.VisibleChanged += boxPanel_VisibleChanged;
            this.Controls.Add(boxPanel);
            SetDoubleBuffered(boxPanel);
            resPanel.Visible = false;

            getBoxControl(boxPanel, 1, 1, 1).Text = "70";
            getBoxControl(boxPanel, 1, 1, 2).Text = "X";
            getBoxControl(boxPanel, 1, 1, 3).Text = "40";
            getBoxControl(boxPanel, 1, 2, 1).Text = "X";
            getBoxControl(boxPanel, 1, 2, 2).Text = "30";
            getBoxControl(boxPanel, 1, 2, 3).Text = "X";
            getBoxControl(boxPanel, 1, 3, 1).Text = "30";
            getBoxControl(boxPanel, 1, 3, 2).Text = "X";
            getBoxControl(boxPanel, 1, 3, 3).Text = "60";
            getBoxControl(boxPanel, 1, 4, 1).Text = "20";
            getBoxControl(boxPanel, 1, 4, 2).Text = "40";
            getBoxControl(boxPanel, 1, 4, 3).Text = "X";

            getBoxControl(boxPanel, 2, 1, 1).Text = "80";
            getBoxControl(boxPanel, 2, 1, 2).Text = "120";
            getBoxControl(boxPanel, 2, 1, 3).Text = "X";
            getBoxControl(boxPanel, 2, 2, 1).Text = "X";
            getBoxControl(boxPanel, 2, 2, 2).Text = "X";
            getBoxControl(boxPanel, 2, 2, 3).Text = "40";
            getBoxControl(boxPanel, 2, 3, 1).Text = "70";
            getBoxControl(boxPanel, 2, 3, 2).Text = "140";
            getBoxControl(boxPanel, 2, 3, 3).Text = "X";
            getBoxControl(boxPanel, 2, 4, 1).Text = "80";
            getBoxControl(boxPanel, 2, 4, 2).Text = "X";
            getBoxControl(boxPanel, 2, 4, 3).Text = "40";

            getBoxControl(boxPanel, 3, 1, 1).Text = "100";
            getBoxControl(boxPanel, 3, 1, 2).Text = "150";
            getBoxControl(boxPanel, 3, 1, 3).Text = "80";
            getBoxControl(boxPanel, 3, 2, 1).Text = "X";
            getBoxControl(boxPanel, 3, 2, 2).Text = "20";
            getBoxControl(boxPanel, 3, 2, 3).Text = "60";
            getBoxControl(boxPanel, 3, 3, 1).Text = "20";
            getBoxControl(boxPanel, 3, 3, 2).Text = "X";
            getBoxControl(boxPanel, 3, 3, 3).Text = "40";
            getBoxControl(boxPanel, 3, 4, 1).Text = "60";
            getBoxControl(boxPanel, 3, 4, 2).Text = "30";
            getBoxControl(boxPanel, 3, 4, 3).Text = "X";

            getBoxControl(boxPanel, 4, 1, 1).Text = "X";
            getBoxControl(boxPanel, 4, 1, 2).Text = "90";
            getBoxControl(boxPanel, 4, 1, 3).Text = "50";
            getBoxControl(boxPanel, 4, 2, 1).Text = "60";
            getBoxControl(boxPanel, 4, 2, 2).Text = "X";
            getBoxControl(boxPanel, 4, 2, 3).Text = "20";
            getBoxControl(boxPanel, 4, 3, 1).Text = "X";
            getBoxControl(boxPanel, 4, 3, 2).Text = "70";
            getBoxControl(boxPanel, 4, 3, 3).Text = "120";
            getBoxControl(boxPanel, 4, 4, 1).Text = "90";
            getBoxControl(boxPanel, 4, 4, 2).Text = "60";
            getBoxControl(boxPanel, 4, 4, 3).Text = "30";

            getBoxControl(boxPanel, 5, 1, 1).Text = "100";
            getBoxControl(boxPanel, 5, 1, 2).Text = "X";
            getBoxControl(boxPanel, 5, 1, 3).Text = "150";
            getBoxControl(boxPanel, 5, 2, 1).Text = "X";
            getBoxControl(boxPanel, 5, 2, 2).Text = "70";
            getBoxControl(boxPanel, 5, 2, 3).Text = "140";
            getBoxControl(boxPanel, 5, 3, 1).Text = "50";
            getBoxControl(boxPanel, 5, 3, 2).Text = "80";
            getBoxControl(boxPanel, 5, 3, 3).Text = "X";
            getBoxControl(boxPanel, 5, 4, 1).Text = "40";
            getBoxControl(boxPanel, 5, 4, 2).Text = "60";
            getBoxControl(boxPanel, 5, 4, 3).Text = "80";

            boxPanel.VisibleChanged += boxPanel_VisibleChanged;
            this.Controls.Add(boxPanel);
            SetDoubleBuffered(boxPanel);
            resPanel.Visible = false;
        }
        public static void SetDoubleBuffered(System.Windows.Forms.Control c)
        {
            //Taxes: Remote Desktop Connection and painting
            //http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;

            System.Reflection.PropertyInfo aProp =
                  typeof(System.Windows.Forms.Control).GetProperty(
                        "DoubleBuffered",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

            aProp.SetValue(c, true, null);
        }

        string exportableDataText()
        {
            float[, ,] data = getDatas();
            int tj = data.GetLength(0);
            int tp = data.GetLength(1);
            int tm = data.GetLength(2);

            string text = "";
            for (int j = 0; j < tp; j++)
            {
                for (int k = 0; k < tm; k++)
                {
                    text += space(8) + (j + 1) + "." + (k + 1);
                }
            }
            text += Environment.NewLine;
            for (int i = 0; i < tj; i++)
            {
                text += (i + 1).ToString() + space(8);
                for (int j = 0; j < tp; j++)
                {
                    for (int k = 0; k < tm; k++)
                    {
                        string me = data[i, j, k].ToString();
                        text += data[i, j, k] + space(8 - (me.Length - 3));
                    }
                }
                text += i == tj - 1 ? ";" : Environment.NewLine;
            }
            return text;
        }

        void ExportMathModel()
        {

            #region mathematical model

            string model =
"sets" + Environment.NewLine +
"i jobs           /1*" + job + "/" + Environment.NewLine +
"j processes      /1*" + proc + "/" + Environment.NewLine +
"k machines       /1*" + mac + "/" + Environment.NewLine +
"o orders         /1*" + job * proc + "/" + Environment.NewLine + Environment.NewLine +

"**kumelerin kopyasini al\r\n" +
"alias(i,ii);" + Environment.NewLine +
"alias(j,jj);" + Environment.NewLine +
"alias(k,kk);" + Environment.NewLine +
"alias(o,oo);\r\n" + Environment.NewLine +

"table t(i,j,k) veri tablosu\r\n" + Environment.NewLine +
exportableDataText() + Environment.NewLine + "\r\n" +
"parameter L buyuk sayi /100000/\r\n" + Environment.NewLine +

"binary variables" + Environment.NewLine +
"x(i,j,k) is-process ikilisinin makineye atanma durumu" + Environment.NewLine +
"a(i,j,o) is-process ikilisinin is siralamasindaki konumunu belirten ikili degisken;\r\n" + Environment.NewLine +

"positive variables" + Environment.NewLine +
"s(i,j,k) is-proses ikilisinin k makinesinde calisma zamani: x(i.j.k) = 0 ise s(i.j.k) = 0)" + Environment.NewLine +
"c(i,j,k) is-proses ikilisinin k makinesinde bitis zamani: x(i.j.k) = 0 ise c(i.j.k) = 0;\r\n" + Environment.NewLine +
"variables" + Environment.NewLine +
"z amac fonksiyonu;\r\n" + Environment.NewLine +

"equations" + Environment.NewLine +

"eq1 her is-proses ikilisi bir makineye atanmali" + Environment.NewLine +
"eq2 her is-proses ikilisi bir makineye atanmali " + Environment.NewLine +
"eq3 her sira numarasi sadece bir is-proses ikilisine ait olmali" + Environment.NewLine +
"eq4 proses bitis zamani = baslangic zamani + calisma zamani" + Environment.NewLine +
"eq5 is-proses k makinesinde calismiyorsa s(i.j.k) = c(i.j.k) = 0" + Environment.NewLine +
"eq6 i-j ve ii-jj ayni makinede calisiyorsa ve i-j nin sira numarasi daha buyukse s(i.j.k) >= c(ii.jj.k)" + Environment.NewLine +
"eq7 i-j nin bir onceki prosesinin sira numarasi i-j ninkinden buyuk olamaz" + Environment.NewLine +
"eq8 i-j nin baslama zamani bir onceki prosesinin bitisinden buyuktur" + Environment.NewLine +
"eq9 makespan: tum i-j lerin bitis zamanindan buyuk yada esittir;\r\n" + Environment.NewLine +


"eq1(i,j)..                                       sum(k, x(i,j,k)) =e= 1;" + Environment.NewLine +

"eq2(i,j)..                                       sum(o, a(i,j,o)) =e= 1;" + Environment.NewLine +

"eq3(o)..                                         sum((i,j), a(i,j,o)) =e= 1;" + Environment.NewLine +

"eq4(i,j,k)..                                     c(i,j,k) =g= s(i,j,k) + t(i,j,k) - (1 - x(i,j,k)) * L;" + Environment.NewLine +

"eq5(i,j,k)..                                     s(i,j,k) + c(i,j,k) =l= L * x(i,j,k);" + Environment.NewLine +

"eq6(i,j,o,k,ii,jj,oo)$(ord(oo) lt ord(o))..      s(i,j,k) + L * (4 - a(i,j,o) - a(ii,jj,oo) - x(i,j,k) - x(ii,jj,k)) =g= c(ii,jj,k);" + Environment.NewLine +

"eq7(i,j,o)$(ord(j) gt 1)..                       sum(oo$(ord(oo) ge ord(o)), a(i,j-1,oo)) =l= 1 - a(i,j,o);" + Environment.NewLine +

"eq8(i,j)$(ord(j) gt 1)..                         sum(k, s(i,j,k)) =g= sum(k, c(i,j-1,k));" + Environment.NewLine +

"eq9(i,j)$(ord(j) eq card(j))..                   z =g= sum(k, c(i,j,k));\r\n" + Environment.NewLine +

"model ciz /all/;" + Environment.NewLine +
"option optcr = 0;" + Environment.NewLine +
"solve ciz using mip min z;" + Environment.NewLine +
"display x.l, z.l,a.l,c.l,s.l;";
            #endregion

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\scheduling_model.gms";
            TextWriter writer = new StreamWriter(path);
            writer.WriteLine(model);
            writer.Close();
            Process.Start(path);
            MessageBox.Show("Model dosyasi masaustune kaydedildi: scheduling_model.gms");
        }
        static string space(int n)
        {
            string text = "";
            for (int i = 0; i < n; i++)
            {
                text += " ";
            }
            return text;
        }
        #endregion

        void ExportDataToXML(string destpath)
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("data");
            Data.DataTable = getDatas();
            int job = Data.DataTable.GetLength(0);
            int mac = Data.DataTable.GetLength(1);
            int proc = Data.DataTable.GetLength(2);

            for (int i = 0; i < job; i++)
            {
                XElement jobEl = new XElement("job");
                jobEl.Add(new XAttribute("id", i + 1));
                for (int j = 0; j < mac; j++)
                {
                    XElement procEl = new XElement("process");
                    procEl.Add(new XAttribute("id", j + 1));
                    for (int k = 0; k < proc; k++)
                    {
                        XElement macEl = new XElement("machine");
                        macEl.Add(new XAttribute("id", k + 1));
                        macEl.Add(new XAttribute("time", Data.DataTable[i, j, k] == 10000 ? -1 : Data.DataTable[i, j, k]));
                        procEl.Add(macEl);
                    }
                    jobEl.Add(procEl);
                }
                root.Add(jobEl);
            }
            doc.Add(root);
            doc.Save(destpath);
            MessageBox.Show("Çıktı kaydedildi\r\n\r\n" + destpath);
        }
        void CreateBoxes(int j,int p,int m)
        {
            this.Controls.Remove(boxPanel);
            if (boxPanel != null)
                boxPanel.Dispose();

            boxPanel = provider.CreateJobBoxes(m, p, j);
            boxPanel.Left = btnHideBoxes.Right + 10;
            boxPanel.Top = btnHideBoxes.Top;
            boxPanel.Width = this.Width - boxPanel.Left - 40;
            boxPanel.AutoScroll = false;
            boxPanel.VerticalScroll.Enabled = false;
            boxPanel.VerticalScroll.Visible = false;
            boxPanel.VerticalScroll.Maximum = 0;
            boxPanel.AutoScroll = true;

            boxPanel.VisibleChanged += boxPanel_VisibleChanged;
            this.Controls.Add(boxPanel);
            SetDoubleBuffered(boxPanel);
            resPanel.Visible = false;
        }
        void ImportDataFromXml(string file)
        {
            if (!File.Exists(file))
            {
                MessageBox.Show("Dosya yerinde bulunamadı!", "HATA");
                return;
            }
            Cursor = Cursors.WaitCursor;
            Application.DoEvents();
            XDocument doc = XDocument.Load(file);
            IEnumerable<XElement> jobEls = doc.Root.Elements("job");
            IEnumerable<XElement> procEls = doc.Root.Element("job").Elements("process");
            IEnumerable<XElement> macEls = doc.Root.Element("job").Element("process").Elements("machine");
            nmudjob.Value = job = jobEls.Count();
            nmudproc.Value = proc = procEls.Count();
            nmudmac.Value = mac = macEls.Count();
            CreateBoxes(job, proc, mac);
            Application.DoEvents();
            int a = 0;
            int b = 0;
            int c = 0;
            foreach (XElement _job in jobEls)
            {
                b = 0;
                foreach (XElement _proc in _job.Elements("process"))
                {
                    c = 0;
                    foreach (XElement _mac in _proc.Elements("machine"))
                    {
                        float time = float.Parse(_mac.Attribute("time").Value);
                        getBoxControl(boxPanel, a + 1, b + 1, c + 1).Text = (time == -1 ? "X" : time.ToString());
                        c++;
                    }
                    b++;
                }
                a++;
            }
            Cursor = Cursors.Arrow;
        }
    }
    public static class ExtMethods
    {
        public static int ToInt(this decimal val)
        {
            return Convert.ToInt32(val);
        }
        public static int ToInt(this string text)
        {
            return Int32.Parse(text);
        }
        public static string ConvertIfInteger(this float number)
        {
            if (number == (int)number)
                return ((int)number).ToString("0");
            else
                return number.ToString("0.00");
        }
    }
}
