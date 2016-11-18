using System;
using System.Drawing;
using System.Windows.Forms;

namespace Scheduling
{
    class InputBoxProvider
    {
        int space;
        int width;
        int extra;
        public InputBoxProvider()
        {
            space = 2;
            width = 22;
            extra = 10;
        }

        #region Create TextBoxes for specific job box
        Panel CreateInputBox(int macs, int procs, int jobIndex)
        {
            int n = macs * procs;
            TextBox[] boxes = new TextBox[n];
            Panel grup = new Panel()
            {
                Width = (width + extra) * macs + (macs - 1) * space,
                Height = width * procs + (procs - 1) * space,
                Name = "boxer"
            };
            int a = 0;
            for (int i = 0; i < procs; i++)
            {
                for (int j = 0; j < macs; j++)
                {
                    boxes[a] = new TextBox()
                    {
                        Font = new Font("Arial", 8f),
                        Location = new Point(j * (width + extra + space), i * (width + space)),
                        Name = "j" + jobIndex + "p" + (i + 1) + "m" + (j + 1),
                        Size = new Size(width + extra, width),
                        Multiline = false,
                        TextAlign = HorizontalAlignment.Center
                    };
                    boxes[a].TextChanged += InputBoxProvider_TextChanged;
                    grup.Controls.Add(boxes[a]);
                    a++;
                }
            }
            return grup;
        }
        #endregion

        #region TextChanged event for each TextBox
        void InputBoxProvider_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;

            if (tb.Text.ToLower() == "x")
            {
                tb.Text = "X";
                tb.ForeColor = Color.FromArgb(255, 122, 122);
                tb.SelectionStart = tb.Text.Length;
            }
            else
                tb.ForeColor = tb.ForeColor = Color.Black;
        }
        #endregion

        #region Machine Labels
        Panel CreateMachineBox(int macs)
        {
            Panel grup = new Panel()
            {
                Width = macs * (width + extra) + (macs - 1) * space,
                Height = 15
            };
            for (int i = 0; i < macs; i++)
            {
                Label lbl = new Label()
                {
                    AutoSize = false,
                    Width = width,
                    Height = 15,
                    Text = "M" + (i + 1),
                    Location = new Point(i * (width + extra + space) + 6, 0),
                    Font = new Font("Arial", 6.75f),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                grup.Controls.Add(lbl);
            }
            return grup;
        } 
        #endregion

        #region Process Labels
        Panel CreateProcessBox(int procs)
        {
            Panel grup = new Panel()
            {
                Width = width,
                Height = procs * width + (procs - 1)
            };
            for (int i = 0; i < procs; i++)
            {
                Label lbl = new Label()
                {
                    AutoSize = false,
                    Width = width,
                    Height = width,
                    Text = "P" + (i + 1),
                    Location = new Point(0, i * (width + space)),
                    Font = new Font("Arial", 6.75f),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                grup.Controls.Add(lbl);
            }
            return grup;
        } 
        #endregion

        #region Job Labels
        Label CreateJobTitleLabel(int jobIndex, int macs)
        {
            Label lbl = new Label();
            lbl.AutoSize = false;
            lbl.Width = macs * (width + extra) + (macs - 1) * space; //Bosluklarla beraber toplam genislik: bosluk = 1px
            lbl.Text = "J" + jobIndex;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Font = new Font("Arial", 10, FontStyle.Bold);
            return lbl;
        } 
        #endregion

        #region Create specific job box
        Panel CreateJobBox(int macs, int procs, int jobIndex)
        {
            Label title = CreateJobTitleLabel(jobIndex, macs);
            Panel macBox = CreateMachineBox(macs);
            Panel boxes = CreateInputBox(macs, procs, jobIndex);
            Panel container = new Panel()
            {
                Width = macBox.Width,
                Height = title.Height + macBox.Height + boxes.Height,
                Name = "j" + jobIndex
            };
            title.Location = new Point(0, 0);
            macBox.Location = new Point(0, title.Bottom);
            boxes.Location = new Point(0, macBox.Bottom);

            container.Controls.Add(title);
            container.Controls.Add(macBox);
            container.Controls.Add(boxes);
            return container;
        } 
        #endregion

        #region Create all job boxes
        public Panel CreateJobBoxes(int mac, int proc, int job)
        {
            Panel procBox = CreateProcessBox(proc);
            procBox.Location = new Point(0, 40);
            Panel container = new Panel();
            container.BackColor = Color.Transparent;
            container.Controls.Add(procBox);
            int x = 0;
            for (int i = 0; i < job; i++)
            {
                Panel jobBox = CreateJobBox(mac, proc, i + 1);
                jobBox.Name = "j" + (i + 1);
                jobBox.Location = new Point(procBox.Width + x + (i == 0 ? 0 : 10), 0);
                container.Controls.Add(jobBox);
                if (i == job - 1)
                {
                    container.Height = jobBox.Bottom + 30;
                }
                x = jobBox.Right - procBox.Width;
            }
            return container;
        } 
        #endregion

    }
}
