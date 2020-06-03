using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using AStarPathfinding.Model;
using static AStarPathfinding.Engine;
using static AStarPathfinding.Location;

namespace AStarPathfinding
{
    public partial class FrmMain : Form, IStatesChangeRecall
    {
        int BoardWidth { get; set; } = 100;
        int BoardHeight { get; set; } = 100;
        Size CanvasSize { get; set; }
        DrawPanel DrawPanel { get; set; }
        List<Line> GridLine { get; } = new List<Line>();
        Engine Engine { get; set; }
        Maze Maze { get; set; }

        bool IStatesChangeRecall.IsEngineRunning
        {
            get => isEngineRunning;
            set
            {
                isEngineRunning = value;
                Invoke(new Action(() =>
                {
                    btnRun.Enabled = !isEngineRunning;
                    btnRandom.Enabled = !isEngineRunning;
                    btnClear.Enabled = !isEngineRunning;
                    btnGenerateMaze.Enabled = !isEngineRunning;
                }));
            }
        }

        Graphics canvas;
        bool isSettingBlock = false;
        LocationType actionType = LocationType.SPACE;
        int? triggerButton;
        private bool isEngineRunning = false;

        public FrmMain()
        {
            InitializeComponent();
            CanvasSize = new Size(500, 500);
            Size = new Size(CanvasSize.Width, CanvasSize.Height + 74);
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
            Engine = new Engine(new Size(BoardWidth, BoardHeight), this);

            //  Draw base line
            for (int x = 0; x <= BoardWidth; x++)
            {
                GridLine.Add(new Line(Color.Black, 1f,
                    new Point(Math.Min(CanvasSize.Width - 1, x * (CanvasSize.Width / BoardWidth)), 0),
                    new Point(Math.Min(CanvasSize.Width - 1, x * (CanvasSize.Width / BoardWidth)), CanvasSize.Height)));
            }
            for (int y = 0; y <= BoardHeight; y++)
            {
                GridLine.Add(new Line(Color.Black, 1f,
                    new Point(0, Math.Min(CanvasSize.Height - 1, y * (CanvasSize.Height / BoardHeight))),
                    new Point(CanvasSize.Width, Math.Min(CanvasSize.Height - 1, y * (CanvasSize.Height / BoardHeight)))));
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
                    if (Engine.Map[x, y].Type == LocationType.START_POINT)
                    {
                        actionType = LocationType.END_POINT;
                    }
                    else
                    {
                        actionType = LocationType.START_POINT;
                    }
                    break;
                case MouseButtons.Left:
                    actionType = LocationType.WALL;
                    isSettingBlock = true;
                    triggerButton = (int)e.Button;
                    break;
                case MouseButtons.Right:
                    actionType = LocationType.SPACE;
                    isSettingBlock = true;
                    triggerButton = (int)e.Button;
                    break;
                default:
                    return;
            }

            Engine.Map[x, y].Type = actionType;

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
            Engine.Map[x, y].Type = actionType;

            RefreshCanvas();
        }

        private void OnCanvasPaint(object sender, PaintEventArgs e)
        {
            if (Maze != null)
            {
                // Create base maze
                for (int x = 0; x < Engine.Map.GetLength(0); x++)
                {
                    for (int y = 0; y < Engine.Map.GetLength(1); y++)
                    {
                        Engine.Map[x, y].Status = LocationStatus.NULL;
                        if (x % 2 == 1 && y % 2 == 1)
                        {
                            Engine.Map[x, y].Type = LocationType.WALL;
                        }
                        else
                        {
                            Engine.Map[x, y].Type = LocationType.SPACE;
                        }
                    }
                }
                //  Transfer maze to block base map
                for (int y = 0; y < Maze.Board.GetLength(0); y++)
                {
                    for (int x = 0; x < Maze.Board.GetLength(1); x++)
                    {
                        Engine.Map[y * 2, x * 2].Type = LocationType.SPACE;
                        if (Maze.Board[y, x].EastWall && ((x * 2 + 1) >= 0))
                        {
                            Engine.Map[x * 2 + 1, y * 2].Type = LocationType.WALL;
                        }
                        if (Maze.Board[y, x].SouthWall && ((y * 2 + 1) >= 0))
                        {
                            Engine.Map[x * 2, y * 2 + 1].Type = LocationType.WALL;
                        }
                        if (Maze.Board[y, x].WestWall && ((x * 2 - 1) >= 0))
                        {
                            Engine.Map[x * 2 - 1, y * 2].Type = LocationType.WALL;
                        }
                        if (Maze.Board[y, x].NorthWall && ((y * 2 - 1) >= 0))
                        {
                            Engine.Map[x * 2, y * 2 - 1].Type = LocationType.WALL;
                        }
                    }
                }
                var start = Maze.Start;
                var end = Maze.End;
                Engine.Map[start.X * 2, start.Y * 2].Type = LocationType.START_POINT;
                Engine.Map[end.X * 2, end.Y * 2].Type = LocationType.END_POINT;
            }

            canvas = e.Graphics;

            if (Maze == null)
            {
                foreach (Line line in GridLine)
                    line.Draw(canvas);
            }

            for (int x = 0; x < Engine.Map.GetLength(0); x++)
            {
                for (int y = 0; y < Engine.Map.GetLength(1); y++)
                {
                    switch (Engine.Map[x, y].Type)
                    {
                        case LocationType.SPACE:
                            switch (Engine.Map[x, y].Status)
                            {
                                case LocationStatus.NULL:
                                    DrawBlock(x, y);
                                    break;
                                case LocationStatus.SEARCHED:
                                    DrawBlock(x, y, color: Color.LightGray);
                                    break;
                                case LocationStatus.PATH:
                                    DrawBlock(x, y, color: Color.Green);
                                    break;
                                case LocationStatus.ERROR:
                                    DrawBlock(x, y, color: Color.Pink);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case LocationType.WALL:
                            DrawBlock(x, y, color: Color.Gray);
                            break;
                        case LocationType.START_POINT:
                            DrawBlock(x, y, color: Color.Blue);
                            break;
                        case LocationType.END_POINT:
                            DrawBlock(x, y, color: Color.Red);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void TransferMouseLocationToIndex(int mouseX, int mouseY, out int x, out int y)
        {
            x = Math.Max(0, Math.Min(BoardWidth - 1, mouseX / (CanvasSize.Width / BoardWidth)));
            y = Math.Max(0, Math.Min(BoardHeight - 1, mouseY / (CanvasSize.Height / BoardHeight)));
        }

        private void RefreshCanvas()
        {
            DrawPanel.Invalidate();
        }

        private void DrawBlock(int x, int y, Color? color = null, bool isFill = true)
        {
            var point = new Point(x * (CanvasSize.Width / BoardWidth), y * (CanvasSize.Height / BoardHeight));
            var size = new Size(CanvasSize.Width / BoardWidth, CanvasSize.Height / BoardHeight);
            if (isFill)
            {
                canvas.FillRectangle(new SolidBrush(color ?? Color.Transparent), new Rectangle(point, size));
            }
            else
            {
                canvas.DrawRectangle(new Pen(Color.Black), new Rectangle(point, size));
            }
        }

        public void OnStatusUpdated()
        {
            RefreshCanvas();
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            RefreshCanvas();
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Engine.Evolve();
            }).Start();
        }

        private void BtnRandom_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            var start = new Point(r.Next(0, Engine.Map.GetLength(0)), r.Next(0, Engine.Map.GetLength(1)));
            var target = new Point(r.Next(0, Engine.Map.GetLength(0)), r.Next(0, Engine.Map.GetLength(1)));
            for (int x = 0; x < Engine.Map.GetLength(0); x++)
            {
                for (int y = 0; y < Engine.Map.GetLength(1); y++)
                {
                    Engine.Map[x, y].Status = LocationStatus.NULL;
                    if (x == start.X && y == start.Y)
                    {
                        Engine.Map[x, y].Type = LocationType.START_POINT;
                    }
                    else if (x == target.X && y == target.Y)
                    {
                        Engine.Map[x, y].Type = LocationType.END_POINT;
                    }
                    else
                    {
                        int type = r.Next(0, 3);
                        if (type == 0)
                        {
                            Engine.Map[x, y].Type = LocationType.WALL;
                        }
                        else
                        {
                            Engine.Map[x, y].Type = LocationType.SPACE;
                        }
                    }
                }
            }
            RefreshCanvas();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            Maze = null;
            for (int x = 0; x < Engine.Map.GetLength(0); x++)
            {
                for (int y = 0; y < Engine.Map.GetLength(1); y++)
                {
                    Engine.Map[x, y].Type = LocationType.SPACE;
                    Engine.Map[x, y].Status = LocationStatus.NULL;
                }
            }
            RefreshCanvas();
        }

        private void BtnGenerateMaze_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                Maze = new Maze(BoardWidth / 2, BoardHeight / 2, this);
                Maze.Generate();
                RefreshCanvas();
                Maze = null;
            }).Start();
        }
    }
}
