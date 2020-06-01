using System;
using System.Collections.Generic;
using System.Drawing;
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
        Color blockColor = Color.Black;
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

            isSettingBlock = true;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    blockColor = Color.Gray;
                    break;
                case MouseButtons.None:
                    break;
                case MouseButtons.Right:
                    blockColor = Color.White;
                    break;
                case MouseButtons.Middle:
                    break;
                case MouseButtons.XButton1:
                    break;
                case MouseButtons.XButton2:
                    break;
                default:
                    return;
            }

            triggerButton = (int)e.Button;

            TransferMouseLocationToIndex(e.X, e.Y, out int x, out int y);
            if (blockColor == Color.White)
            {
                Engine.Map[x, y] = null;
            }
            else
            {
                Engine.Map[x, y] = blockColor.Name;
            }
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
            TransferMouseLocationToIndex(e.X, e.Y, out int x, out int y);
            if (blockColor == Color.White)
            {
                Engine.Map[x, y] = null;
            }
            else
            {
                Engine.Map[x, y] = blockColor.Name;
            }
            if (isSettingBlock)
                RefreshCanvas();
        }

        private void OnCanvasPaint(object sender, PaintEventArgs e)
        {
            //RefreshCanvas();
        }

        private void TransferMouseLocationToIndex(int mouseX, int mouseY, out int x, out int y)
        {
            x = mouseX / (CanvasSize.Width / ButtonCountX);
            y = mouseY / (CanvasSize.Height / ButtonCountY);
        }

        private void RefreshCanvas()
        {
            DrawPanel.Invalidate();

            foreach (Line line in GridLine)
                line.Draw(canvas);

            for (int x = 0; x < Engine.Map.GetLength(0); x++)
            {
                for (int y = 0; y < Engine.Map.GetLength(1); y++)
                {
                    if (Engine.Map[x, y] != null)
                    {
                        Color brushColor = new Color();
                        if (Engine.Map[x, y] == Color.Gray.Name)
                        {
                            brushColor = Color.Gray;
                        }
                        if (Engine.Map[x, y] == Color.White.Name)
                        {
                            brushColor = Color.White;
                        }
                        DrawBlock(brushColor, x, y);
                        Console.WriteLine($@"{x}:{y}");
                    }
                }
            }
        }

        private void DrawBlock(Color color, int x, int y)
        {
            var point = new Point(x * (CanvasSize.Width / ButtonCountX), y * (CanvasSize.Height / ButtonCountY));
            var size = new Size(CanvasSize.Width / ButtonCountX, CanvasSize.Height / ButtonCountY);
            canvas.FillRectangle(new SolidBrush(color), new Rectangle(point, size));
        }
    }
}
