using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using AStarPathfinding.Model;

namespace AStarPathfinding
{
    public partial class FrmMain : Form
    {
        int ButtonCountX { get; set; } = 50;
        int ButtonCountY { get; set; } = 50;
        Size CanvasSize { get; set; }
        DrawPanel DrawPanel { get; set; }
        List<Line> GridLine { get; } = new List<Line>();
        Engine Engine { get; set; }

        Graphics canvas;
        bool isSettingBlock = false;
        string actionType = string.Empty;
        int? triggerButton;

        public FrmMain()
        {
            InitializeComponent();
            CanvasSize = new Size(600, 600);
            Size = new Size(CanvasSize.Width, CanvasSize.Height);
            //MaximumSize = new Size(Size.Width + 20, Size.Height + 20);
            MinimumSize = Size;

            DrawPanel = new DrawPanel()
            {
                Size = CanvasSize
            };
            DrawPanel.Paint += new PaintEventHandler(OnCanvasPaint);
            DrawPanel.MouseDown += new MouseEventHandler(OnMouseDown);
            DrawPanel.MouseUp += new MouseEventHandler(OnMouseUp);
            DrawPanel.MouseMove += new MouseEventHandler(OnMouseMove);
            Controls.Add(DrawPanel);
            canvas = DrawPanel.CreateGraphics();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            Engine = new Engine(new Size(ButtonCountX, ButtonCountY));

            //  Draw base line
            for (int x = 0; x <= ButtonCountX; x++)
            {
                GridLine.Add(new Line(Color.Black, 1f,
                    new Point(Math.Min(CanvasSize.Width - 1, x * (CanvasSize.Width / ButtonCountX)), 0),
                    new Point(Math.Min(CanvasSize.Width - 1, x * (CanvasSize.Width / ButtonCountX)), CanvasSize.Height)));
            }
            for (int y = 0; y <= ButtonCountY; y++)
            {
                GridLine.Add(new Line(Color.Black, 1f,
                    new Point(0, Math.Min(CanvasSize.Height - 1, y * (CanvasSize.Height / ButtonCountY))),
                    new Point(CanvasSize.Width, Math.Min(CanvasSize.Height - 1, y * (CanvasSize.Height / ButtonCountY)))));
            }

        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (triggerButton != null)
                return;


            switch (e.Button)
            {
                case MouseButtons.Left:
                    actionType = "X";
                    break;
                case MouseButtons.Right:
                    actionType = string.Empty;
                    break;
                default:
                    return;
            }

            isSettingBlock = true;

            triggerButton = (int)e.Button;

            TransferMouseLocationToIndex(e.X, e.Y, out int x, out int y);
            Engine.Map[x, y] = actionType;

            RefreshCanvas();
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if ((int)e.Button == triggerButton)
            {
                isSettingBlock = false;
                triggerButton = null;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!isSettingBlock)
                return;

            TransferMouseLocationToIndex(e.X, e.Y, out int x, out int y);
            Engine.Map[x, y] = actionType;

            RefreshCanvas();
        }

        private void OnCanvasPaint(object sender, PaintEventArgs e)
        {
            RefreshCanvas();
        }

        private void TransferMouseLocationToIndex(int mouseX, int mouseY, out int x, out int y)
        {
            x = mouseX / (CanvasSize.Width / ButtonCountX);
            y = mouseY / (CanvasSize.Height / ButtonCountY);
        }

        private void RefreshCanvas()
        {
            foreach (Line line in GridLine)
                line.Draw(canvas);

            for (int x = 0; x < Engine.Map.GetLength(0); x++)
            {
                for (int y = 0; y < Engine.Map.GetLength(1); y++)
                {
                    if (Engine.Map[x, y] != null)
                    {
                        Color color = new Color();
                        if (Engine.Map[x, y] == "X")
                        {
                            color = Color.Gray;
                        }
                        if (Engine.Map[x, y] == string.Empty)
                        {
                            color = BackColor;
                        }
                        DrawBlock(color, x, y, isFill: false);
                    }
                }
            }
        }

        private void DrawBlock(Color color, int x, int y, bool isFill = true)
        {
            var point = new Point(x * (CanvasSize.Width / ButtonCountX), y * (CanvasSize.Height / ButtonCountY));
            var size = new Size(CanvasSize.Width / ButtonCountX, CanvasSize.Height / ButtonCountY);
            if (isFill)
            {
                canvas.FillRectangle(new SolidBrush(color), new Rectangle(point, size));
            }
            else
            {
                canvas.FillRectangle(new SolidBrush(color), new Rectangle(point, size));
                canvas.DrawRectangle(new Pen(Color.Black), new Rectangle(point, size));
            }
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            RefreshCanvas();
        }
    }
}
