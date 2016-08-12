using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cizelgeleme
{
    public class Schedule
    {
        private int[] jobSchedule;
        private int[] macSchedule;
        int j, p, m;
        float[,] startTimes, finishTimes;
        float[] idleTimes;
        float totalIdleTime;
        bool makeSpanCalculated, idleCalculated;
        float makeSpan;

        public Schedule(int[] js, int[] ms, int j, int p, int m)
        {
            this.j = j;
            this.p = p;
            this.m = m;
            jobSchedule = js;
            macSchedule = ms;
            
            makeSpanCalculated = false;
            idleCalculated = false;
            makeSpan = 0;
            startTimes = new float[j, p];
            finishTimes = new float[j, p];
            idleTimes = new float[m];
        }

        #region Makespan

        public float MakeSpan()
        {
            if (makeSpanCalculated)
                return makeSpan;

            int[] compProcs = new int[j];
            float[] totalTime = new float[m];

            int?[] lastJob = new int?[m];
            int?[] lastProc = new int?[m];

            for (int i = 0; i < jobSchedule.Length; i++)
            {
                int job = jobSchedule[i];
                int proc = compProcs[job];
                int a = job * p + proc;
                int mac = macSchedule[a];
                int oldProc = compProcs[job] > 0 ? compProcs[job] - 1 : -1;

                startTimes[job, proc] = Math.Max(totalTime[mac],
                                                oldProc == -1 ? 0 : finishTimes[job, oldProc]);
                //Idle time calculator
                idleTimes[mac] += startTimes[job, proc] - totalTime[mac];
                //
                //last process and job at the machine
                lastJob[mac] = job;
                lastProc[mac] = proc;
                //

                finishTimes[job, proc] = startTimes[job, proc] + Data.DataTable[job, proc, mac];

                totalTime[mac] = finishTimes[job, proc];
                compProcs[job]++;
            }

            float result = -1;
            for (int i = 0; i < j; i++)
            {
                result = Math.Max(finishTimes[i, p - 1], result);
            }
            makeSpan = result;
            //find last idle times
            for (int i = 0; i < m; i++)
            {
                if (lastProc[i] != null)
                    idleTimes[i] += makeSpan - finishTimes[(int)lastJob[i], (int)lastProc[i]];
                else
                    idleTimes[i] = makeSpan;
                if (i == m - 1)
                    idleTimes[i] = float.Parse(idleTimes[i].ToString("0.00"));

            }
            
            //
            makeSpanCalculated = true;
            return makeSpan;
        }
        #endregion

        #region Drawing

        public void DrawSchedule(Graphics grp, Control panel)
        {
            if (!makeSpanCalculated)
                MakeSpan();

            grp.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            int mac = m;
            int proc = p;
            int job = j;
            int horSpace = 40;
            int vertSpace = 60;
            panel.Height = (mac * 25 + vertSpace);
            int delWidth = 0;
            //Draw job delegate colors
            for (int i = 0; i < job; i++)
            {
                Rectangle rect = new Rectangle(50 * i + horSpace, 5, 15, 15);
                string text = "J" + (i + 1);
                Point tp = new Point(rect.Right + 3, rect.Y);
                using (Font font = new Font("Arial", 10))
                using (SolidBrush brush = new SolidBrush(Colors.JobColors[i]))
                {
                    grp.FillEllipse(brush, rect);
                    grp.DrawString(text, font, Brushes.Gray, tp);
                }
                if (i == job - 1)
                    delWidth = tp.X + 20;
            }
            panel.Width = (int)Math.Max(delWidth, makeSpan + horSpace + 15) + 30;

            //Draw measure lines and texts
            PointF itPo = Point.Empty;
            int idWidth = 0;
            int num = (int)((makeSpan + 5) / 25);
            for (int i = 0; i <= num; i++)
            {
                Point p1 = new Point(25 * i + horSpace, vertSpace - 3);
                Point p2 = new Point(25 * i + horSpace, vertSpace + (int)((mac - 0.5) * 25) + 5);
                using (Font font = new Font("Arial", 7))
                {
                    grp.DrawLine(Pens.DarkGray, p1, p2);
                    string text = (25 * i).ToString();
                    Size size = TextRenderer.MeasureText(text, font);
                    Point tp = new Point(p1.X - size.Width / 2 + 2, p1.Y - size.Height);
                    grp.DrawString(text, font, Brushes.DarkGray, tp);

                }
            }
            //Draw background rectangles [height = 15]
            RectangleF[] backRects = new RectangleF[mac];
            for (int i = 0; i < mac; i++)
            {
                backRects[i] = new RectangleF(horSpace, vertSpace + 25 * i, makeSpan + 5, 15);
                grp.FillRectangle(Brushes.LightGray, backRects[i]);
                using (Font font = new Font("Arial", 8, FontStyle.Bold))
                {
                    if (i == 0)
                    {
                        Size idSize = TextRenderer.MeasureText("Idle", font);
                        itPo = new PointF(backRects[i].X + backRects[i].Width + 10, vertSpace - 3 - idSize.Height);
                        grp.DrawString("Idle", font, Brushes.DarkSlateGray, itPo);
                        idWidth = TextRenderer.MeasureText("Idle", font).Width;
                    }
                    string idle = idleTimes[i].ToString("0.00");
                    Size size = TextRenderer.MeasureText(idle, font);
                    grp.DrawString(idle, font, Brushes.DarkGray, itPo.X + idWidth / 2 - size.Width / 2,
                                                                    backRects[i].Y + backRects[i].Height / 2 - size.Height / 2 + 2);
                }
            }
            //Draw machine texts
            for (int i = 0; i < mac; i++)
            {
                using (Font font = new Font("Arial", 10))
                {
                    string text = "M" + (i + 1);
                    Size size = TextRenderer.MeasureText(text, font);
                    PointF tp = new PointF(backRects[i].X - size.Width, backRects[i].Y);
                    grp.DrawString(text, font, Brushes.Gray, tp);

                }
            }
            //Draw process bars and texts
            for (int i = 0; i < job; i++)
            {
                for (int b = 0; b < proc; b++)
                {
                    int work = i;
                    int islem = b;
                    int o = i * p + b;
                    int makine = macSchedule[o];

                    float x = startTimes[i, b] + horSpace;
                    float y = backRects[makine].Y;
                    float height = 15;
                    float time = Data.DataTable[i, b, makine];
                    float normWidth = 0;
                    float fuzWidth = 0;
                        fuzWidth = 0;
                        normWidth = time;
                    
                    PointF ps1 = new PointF(x, y + 15);
                    PointF ps2 = new PointF(x + fuzWidth, ps1.Y - height);
                    PointF ps3 = new PointF(ps2.X + normWidth, ps2.Y);
                    PointF ps4 = new PointF(ps3.X, ps3.Y + height);

                    GraphicsPath path = new GraphicsPath();
                    path.AddLine(ps1, ps2);
                    path.AddLine(ps2, ps3);
                    path.AddLine(ps3, ps4);
                    path.CloseAllFigures();

                    RectangleF bar = new RectangleF(x, y, ps3.X - ps1.X, height);
                    using (SolidBrush brush = new SolidBrush(Colors.JobColors[i]))
                    {
                        grp.DrawPath(Pens.Black, path);
                        grp.FillPath(brush, path);
                    }
                    using (Font font = new Font("Arial", 7.5f))
                    {
                        string text = (i + 1) + "-" + (b + 1);
                        SizeF size = TextRenderer.MeasureText(text, font);
                        if (size.Width > bar.Width)
                        {
                            text = (b + 1).ToString();
                            size = TextRenderer.MeasureText(text,font);
                        }
                        PointF tp = new PointF(bar.X + bar.Width / 2 - size.Width / 2 + 3, bar.Y + height / 2 - size.Height / 2 + 1);
                        grp.DrawString(text, font, Brushes.Black, tp);
                    }

                }
            }
        }
        
        #endregion
        
        #region Repair
        Random rnd = new Random();
        public void Repair()
        {
            int num = p;
            int[] used = new int[j];
            List<int> pos = new List<int>();
            List<int> jobs = new List<int>();
            for (int i = 0; i < p * j; i++)
            {
                used[jobSchedule[i]]++;
                if (used[jobSchedule[i]] > num)
                    pos.Add(i);
            }
            for (int i = 0; i < used.Length; i++)
            {
                if (used[i] < num)
                {
                    for (int t = 0; t < num - used[i]; t++)
                    {
                        jobs.Add(i);
                    }
                }
            }
            for (int i = 0; i < pos.Count; i++)
            {
                int t = rnd.Next(0, jobs.Count);
                jobSchedule[pos[i]] = jobs[t];
                jobs.RemoveAt(t);
            }
        }
        #endregion

        #region Properties
        public float FitValue
        {
            get
            {
                return MakeSpan();
            }
        }
        public int[] MacSchedule
        {
            get { return macSchedule; }
            set { macSchedule = value; }
        }
        public int[] JobSchedule
        {
            get { return jobSchedule; }
            set { jobSchedule = value; }
        }
        public float TotalIdleTime
        {
            get
            {
                MakeSpan();
                if (idleCalculated)
                    return totalIdleTime;
                float total = 0;
                for (int i = 0; i < m; i++)
                {
                    total += idleTimes[i];
                }
                totalIdleTime = total;
                idleCalculated = true;
                return totalIdleTime;
            }
        }
        #endregion

        #region ToString
        
        public string JobsToString()
        {
            string text = "";
            for (int i = 0; i < j * p; i++)
            {
                text += (jobSchedule[i]+1) + "";
            }
            return text;
        }
        public string MacToString()
        {
            string text = "";
            for (int i = 0; i < j * p; i++)
            {
                text += (macSchedule[i] + 1) + "";
            }
            return text;
        }
        #endregion
    }
}
