using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AStarPathfinding.Model;
using static AStarPathfinding.Engine;

namespace AStarPathfinding
{
    public partial class FrmMain : Form, IStatesChangeRecall
    {
        int ButtonCountX { get; set; } = 50;
        int ButtonCountY { get; set; } = 50;
        Size CanvasSize { get; set; }
        DrawPanel DrawPanel { get; set; }
        List<Line> GridLine { get; } = new List<Line>();
        Engine Engine { get; set; }

        bool IStatesChangeRecall.IsEngineRunning
        {
            get => isEngineRunning;
            set
            {
                isEngineRunning = value;
                Invoke(new Action(() => btnRun.Enabled = !isEngineRunning));
            }
        }

        Graphics canvas;
        bool isSettingBlock = false;
        string actionType = string.Empty;
        int? triggerButton;
        private bool isEngineRunning = false;

        public FrmMain()
        {
            InitializeComponent();
            CanvasSize = new Size(600, 600);
            Size = new Size(CanvasSize.Width, CanvasSize.Height + 75);
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
            Engine = new Engine(new Size(ButtonCountX, ButtonCountY), this);

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
            if (isEngineRunning)
                return;
            if (triggerButton != null)
                return;

            TransferMouseLocationToIndex(e.X, e.Y, out int x, out int y);

            switch (e.Button)
            {
                case MouseButtons.Middle:
                    if (Engine.Map[x, y] == "A")
                    {
                        actionType = "B";
                    }
                    else
                    {
                        actionType = "A";
                    }
                    break;
                case MouseButtons.Left:
                    actionType = "X";
                    isSettingBlock = true;
                    triggerButton = (int)e.Button;
                    break;
                case MouseButtons.Right:
                    actionType = null;
                    isSettingBlock = true;
                    triggerButton = (int)e.Button;
                    break;
                default:
                    return;
            }

            Engine.Map[x, y] = actionType;

            RefreshCanvas();
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (isEngineRunning)
                return;
            if ((int)e.Button == triggerButton)
            {
                isSettingBlock = false;
                triggerButton = null;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isEngineRunning)
                return;
            if (!isSettingBlock)
                return;

            TransferMouseLocationToIndex(e.X, e.Y, out int x, out int y);
            Engine.Map[x, y] = actionType;

            RefreshCanvas();
        }

        private void OnCanvasPaint(object sender, PaintEventArgs e)
        {
            canvas = e.Graphics;

            foreach (Line line in GridLine)
                line.Draw(canvas);

            for (int x = 0; x < Engine.Map.GetLength(0); x++)
            {
                for (int y = 0; y < Engine.Map.GetLength(1); y++)
                {
                    switch (Engine.Map[x, y])
                    {
                        case "A":
                            DrawBlock(x, y, color: Color.Blue);
                            break;
                        case "B":
                            DrawBlock(x, y, color: Color.Red);
                            break;
                        case "X":
                            DrawBlock(x, y, color: Color.Gray);
                            break;
                        case "*":
                            DrawBlock(x, y, color: Color.LightGray);
                            break;
                        case "_":
                            DrawBlock(x, y, color: Color.Green);
                            break;
                        case null:
                            DrawBlock(x, y, isFill: false);
                            break;
                        default:
                            DrawBlock(x, y, color: Color.Pink);
                            break;
                    }
                }
            }
        }

        private void TransferMouseLocationToIndex(int mouseX, int mouseY, out int x, out int y)
        {
            x = Math.Max(0, Math.Min(ButtonCountX - 1, mouseX / (CanvasSize.Width / ButtonCountX)));
            y = Math.Max(0, Math.Min(ButtonCountY - 1, mouseY / (CanvasSize.Height / ButtonCountY)));
        }

        private void RefreshCanvas()
        {
            DrawPanel.Invalidate();
        }

        private void DrawBlock(int x, int y, Color? color = null, bool isFill = true)
        {
            var point = new Point(x * (CanvasSize.Width / ButtonCountX), y * (CanvasSize.Height / ButtonCountY));
            var size = new Size(CanvasSize.Width / ButtonCountX, CanvasSize.Height / ButtonCountY);
            if (isFill)
            {
                canvas.FillRectangle(new SolidBrush(color ?? Color.Transparent), new Rectangle(point, size));
            }
            else
            {
                canvas.DrawRectangle(new Pen(Color.Black), new Rectangle(point, size));
            }
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            RefreshCanvas();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Engine.Main();
            }).Start();
            RefreshCanvas();
        }

        public void RefreshUi()
        {
            RefreshCanvas();
        }
    }
}
