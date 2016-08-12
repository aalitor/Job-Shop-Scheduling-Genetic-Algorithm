using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cizelgeleme
{
    #region AltoTextBox

    public class AltoTextBox : Control
    {
        int radius = 15;
        public TextBox box = new TextBox();
        GraphicsPath shape;
        GraphicsPath innerRect;
        Color br;
        public AltoTextBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            box.Parent = this;
            Controls.Add(box);

            box.BorderStyle = BorderStyle.None;
            box.TextAlign = HorizontalAlignment.Left;
            box.Font = Font;

            BackColor = Color.Transparent;
            ForeColor = Color.DimGray;
            br = Color.White;
            box.BackColor = br;
            Text = null;
            Font = new Font("Comic Sans MS", 11);
            Size = new Size(135, 33);
            DoubleBuffered = true;
            box.KeyDown += box_KeyDown;
            box.TextChanged += box_TextChanged;
            box.MouseDoubleClick += box_MouseDoubleClick;
        }

        void box_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;

            box.SelectAll();
        }

        void box_TextChanged(object sender, EventArgs e)
        {
            Text = box.Text;
        }
        public void SelectAll()
        {
            box.SelectAll();
        }
        void box_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                box.SelectionStart = 0;
                box.SelectionLength = Text.Length;
            }
        }
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            box.Text = Text;
        }
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            box.Font = Font;
            Invalidate();
        }
        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            box.ForeColor = ForeColor;
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            shape = new RoundedRectangleF(Width, Height, radius).Path;
            innerRect = new RoundedRectangleF(Width - 0.5f, Height - 0.5f, radius, 0.5f, 0.5f).Path;
            if (box.Height >= Height - 4)
                Height = box.Height + 4;
            box.Location = new Point(radius - 5, Height / 2 - box.Font.Height / 2);
            box.Width = Width - (int)(radius * 1.5);

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            Bitmap bmp = new Bitmap(Width, Height);
            Graphics grp = Graphics.FromImage(bmp);
            e.Graphics.DrawPath(Pens.Gray, shape);
            using (SolidBrush brush = new SolidBrush(br))
                e.Graphics.FillPath(brush, innerRect);

            base.OnPaint(e);
        }
        public Color Br
        {
            get
            {
                return br;
            }
            set
            {
                br = value;
                if (br != Color.Transparent)
                    box.BackColor = br;
                Invalidate();
            }
        }
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = Color.Transparent;
            }
        }
    }

    #endregion

    #region RoundedRectangle
    public class RoundedRectangleF
    {

        Point location;
        float radius;
        GraphicsPath grPath;
        float x, y;
        float width, height;


        public RoundedRectangleF(float width, float height, float radius, float x = 0, float y = 0)
        {

            location = new Point(0, 0);
            this.radius = radius;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            grPath = new GraphicsPath();
            if (radius <= 0)
            {
                grPath.AddRectangle(new RectangleF(x, y, width, height));
                return;
            }
            RectangleF upperLeftRect = new RectangleF(x, y, 2 * radius, 2 * radius);
            RectangleF upperRightRect = new RectangleF(width - 2 * radius - 1, x, 2 * radius, 2 * radius);
            RectangleF lowerLeftRect = new RectangleF(x, height - 2 * radius - 1, 2 * radius, 2 * radius);
            RectangleF lowerRightRect = new RectangleF(width - 2 * radius - 1, height - 2 * radius - 1, 2 * radius, 2 * radius);

            grPath.AddArc(upperLeftRect, 180, 90);
            grPath.AddArc(upperRightRect, 270, 90);
            grPath.AddArc(lowerRightRect, 0, 90);
            grPath.AddArc(lowerLeftRect, 90, 90);
            grPath.CloseAllFigures();

        }
        public RoundedRectangleF()
        {
        }
        public GraphicsPath Path
        {
            get
            {
                return grPath;
            }
        }
        public RectangleF Rect
        {
            get
            {
                return new RectangleF(x, y, width, height);
            }
        }
        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                radius = value;
            }
        }

    }
    #endregion
}
