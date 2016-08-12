using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cizelgeleme
{
    class InputBoxProvider
    {
        int space;
        int w;
        int ex;
        public InputBoxProvider()
        {
            space = 2;
            w = 20;
            ex = 10;
        }
        Panel CreateInputBox(int macs, int procs, int jobIndex)
        {
            int n = macs * procs;
            TextBox[] boxes = new TextBox[n];
            Panel grup = new Panel()
            {
                Width = (w + ex) * macs + (macs - 1) * space,
                Height = w * procs + (procs - 1) * space,
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
                        Location = new Point(j * (w + ex + space), i * (w + space)),
                        Name = "j" + jobIndex + "p" + (i + 1) + "m" + (j + 1),
                        Size = new Size(w + ex, w),
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
        Panel CreateMachineBox(int macs)
        {
            Panel grup = new Panel()
            {
                Width = macs * (w + ex) + (macs - 1) * space,
                Height = 15
            };
                for (int i = 0; i < macs; i++)
                {
                    Label lbl = new Label()
                    {
                        AutoSize = false,
                        Width = w,
                        Height = 15,
                        Text = "M" + (i + 1),
                        Location = new Point(i * (w + ex + space), 0),
                        Font = new Font("Arial", 6.75f),
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    grup.Controls.Add(lbl);
                }
            return grup;
        }
        Panel CreateProcessBox(int procs)
        {
            Panel grup = new Panel()
            {
                Width = w,
                Height = procs * w + (procs - 1)
            };
            for (int i = 0; i < procs; i++)
            {
                Label lbl = new Label()
                {
                    AutoSize = false,
                    Width = w,
                    Height = w,
                    Text = "P" + (i + 1),
                    Location = new Point(0, i * (w + space)),
                    Font = new Font("Arial", 6.75f),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                grup.Controls.Add(lbl);
            }
            return grup;
        }
        Label CreateJobTitleLabel(int jobIndex, int macs)
        {
            Label lbl = new Label();
            lbl.AutoSize = false;
            lbl.Width = macs * (w + ex) + (macs - 1) * space; //Bosluklarla beraber toplam genislik: bosluk = 1px
            lbl.Text = "J" + jobIndex;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Font = new Font("Arial", 10, FontStyle.Bold);
            return lbl;
        }

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

    }
}
