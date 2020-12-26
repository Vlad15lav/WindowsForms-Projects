using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Resources;

namespace Hanoi_Towers
{
    public partial class Form1 : Form
    {
        // Stack for towers
        static Stack<int> OneTower = new Stack<int>();
        static Stack<int> TwoTower = new Stack<int>();
        static Stack<int> ThreeTower = new Stack<int>();
        PictureBox[] TowerBlins = new PictureBox[8];
        // Weight disk on top of the tower
        static int FrontOne = 1;
        static int FrontTwo = 10;
        static int FrontThree = 10;
        int SizeGame = 8; // Number of disks
        int NumRows = 1; // Number of row for write step
        // Start position for disks
        static int StartPos;
        static int StartPosX;
        static int StartPosY;

        bool isEnglish = true; // Language App

        string FirstWord = "Move ";
        string LastWord = " to ";

        bool isAIHelp = false; // Selected AI Help
        public static Form1 Instance { get; private set; }
        Form2 Faq = new Form2(); // Form FAQ window

        Queue<int> numblin = new Queue<int>();
        Queue<Point> pointblin = new Queue<Point>();

        public Form1()
        {
            InitializeComponent();
            Instance = this;
            InitializeBlins();
            StartGame();
        }

        void InitializeBlins()
        // Write disk img in mass
        {
            TowerBlins[0] = pictureBox8;
            TowerBlins[1] = pictureBox7;
            TowerBlins[2] = pictureBox6;
            TowerBlins[3] = pictureBox5;
            TowerBlins[4] = pictureBox4;
            TowerBlins[5] = pictureBox3;
            TowerBlins[6] = pictureBox2;
            TowerBlins[7] = pictureBox1;
        }

        void StartGame()
        // Start settings and attributes
        {
            // Hide disk
            pictureBox12.Visible = false;
            pictureBox8.Visible = false;
            pictureBox7.Visible = false;
            pictureBox6.Visible = false;
            pictureBox5.Visible = false;
            pictureBox4.Visible = false;
            pictureBox3.Visible = false;
            pictureBox2.Visible = false;
            pictureBox1.Visible = false;
            SizeGame = trackBar1.Value;
            // Required disks showing
            if (SizeGame > 0)
                pictureBox8.Visible = true;
            if (SizeGame > 1)
                pictureBox7.Visible = true;
            if (SizeGame > 2)
                pictureBox6.Visible = true;
            if (SizeGame > 3)
                pictureBox5.Visible = true;
            if (SizeGame > 4)
                pictureBox4.Visible = true;
            if (SizeGame > 5)
                pictureBox3.Visible = true;
            if (SizeGame > 6)
                pictureBox2.Visible = true;
            if (SizeGame > 7)
                pictureBox1.Visible = true;
            // Start position disks
            for (int i = 0, j = SizeGame - 1; i < SizeGame; i++)
            {
                int X = 15 * (i + 9 - SizeGame);
                int Y = 380 - 40 * (i + 1);
                Point Pos = new Point(X, Y);
                TowerBlins[j].Location = Pos;
                j--;
            }
            InitializeValue();
            CheckControl(); // Get control for mouse
        }

        public void InitializeValue()
        // Reset variables
        {
            this.Controls.SetChildIndex(pictureBox9, 1000);
            this.Controls.SetChildIndex(pictureBox10, 1100);
            this.Controls.SetChildIndex(pictureBox11, 1200);
            this.Controls.SetChildIndex(pictureBox12, 1300);
            dataGridView1.Rows.Clear();
            NumRows = 1;
            OneTower.Clear();
            TwoTower.Clear();
            ThreeTower.Clear();
            OneTower.Push(10);
            TwoTower.Push(10);
            ThreeTower.Push(10);
            FrontOne = 1;
            FrontTwo = 10;
            FrontThree = 10;
            for (int i = 8; i > 0; i--)
                if (SizeGame >= i)
                    OneTower.Push(i);
        }

        // Arg: Number of disk, from tower, to tower, free tower
        void Towers(int number, int from, int to, int free)
        {
            if (number == 0)
                return;
            Towers(number - 1, from, free, to);
            int x;
            if (to == 1)
                x = 100;
            else if (to == 2)
                x = 500;
            else
                x = 900;
            StartPos = from;
            StartPosX = TowerBlins[number - 1].Location.X;
            StartPosY = TowerBlins[number - 1].Location.Y;
            int num = Convert.ToInt32(TowerBlins[number - 1].Tag.ToString());
            //TowerBlins[number - 1].Location = ControlMover.GetPos(x, num);
            numblin.Enqueue(number - 1);
            pointblin.Enqueue(ControlMover.GetPos(x, num));

            Towers(number - 1, free, to, from);
        }

        void AIHelp()
        {
            StartGame();
            // Remove control, AI solution
            ControlMover.Remove(pictureBox1);
            ControlMover.Remove(pictureBox2);
            ControlMover.Remove(pictureBox3);
            ControlMover.Remove(pictureBox4);
            ControlMover.Remove(pictureBox5);
            ControlMover.Remove(pictureBox6);
            ControlMover.Remove(pictureBox7);
            ControlMover.Remove(pictureBox8);
            Towers(trackBar1.Value, 1, 3, 2);
        }

        void AddRow(int from, int to)
        // Write step
        {
            dataGridView1.Rows.Add(NumRows++, FirstWord + from.ToString() + LastWord + to.ToString());
        }

        public void CheckControl()
        // Check disks for control
        {
            ControlMover.Remove(pictureBox1);
            ControlMover.Remove(pictureBox2);
            ControlMover.Remove(pictureBox3);
            ControlMover.Remove(pictureBox4);
            ControlMover.Remove(pictureBox5);
            ControlMover.Remove(pictureBox6);
            ControlMover.Remove(pictureBox7);
            ControlMover.Remove(pictureBox8);
            FrontOne = FrontOneBlin();
            FrontTwo = FrontTwoBlin();
            FrontThree = FrontThreeBlin();

            if (FrontOne == 10 && FrontTwo == 10)
            {
                if (isEnglish)
                    MessageBox.Show("You won!");
                else
                    MessageBox.Show("Вы победили!");
                pictureBox12.Image = Properties.Resources.victory;
                pictureBox12.Visible = true;
            }
            // Possible disks for control
            if (FrontOne == 1 || FrontTwo == 1 || FrontThree == 1)
                ControlMover.Add(pictureBox8);
            if (FrontOne == 2 || FrontTwo == 2 || FrontThree == 2)
                ControlMover.Add(pictureBox7);
            if (FrontOne == 3 || FrontTwo == 3 || FrontThree == 3)
                ControlMover.Add(pictureBox6);
            if (FrontOne == 4 || FrontTwo == 4 || FrontThree == 4)
                ControlMover.Add(pictureBox5);
            if (FrontOne == 5 || FrontTwo == 5 || FrontThree == 5)
                ControlMover.Add(pictureBox4);
            if (FrontOne == 6 || FrontTwo == 6 || FrontThree == 6)
                ControlMover.Add(pictureBox3);
            if (FrontOne == 7 || FrontTwo == 7 || FrontThree == 7)
                ControlMover.Add(pictureBox2);
            if (FrontOne == 8 || FrontTwo == 8 || FrontThree == 8)
                ControlMover.Add(pictureBox1);
        }

        // The weight of the front disk | The left tower
        static int FrontOneBlin()
        {
            if (OneTower.Count() == 0)
                return 10;
            int num = OneTower.Pop();
            OneTower.Push(num);
            return num;
        }

        // The weight of the front disk | The center tower
        static int FrontTwoBlin()
        {
            if (TwoTower.Count() == 0)
                return 10;
            int num = TwoTower.Pop();
            TwoTower.Push(num);
            return num;
        }

        // The weight of the front disk | The right tower
        static int FrontThreeBlin()
        {
            if (ThreeTower.Count() == 0)
                return 10;
            int num = ThreeTower.Pop();
            ThreeTower.Push(num);
            return num;
        }

        // Control obj class (mouse)
        public static class ControlMover
        {
            public static bool ChangeCursor { get; set; }
            public static bool AllowMove { get; set; }
            public static bool AllowResize { get; set; }
            public static bool BringToFront { get; set; }
            public static int ResizingMargin { get; set; }
            public static int MinSize { get; set; }

            private static Point startMouse;
            private static Point startLocation;
            private static Size startSize;
            private static bool resizing = false;
            static Cursor oldCursor;

            static ControlMover()
            {
                ResizingMargin = 5;
                MinSize = 10;
                ChangeCursor = false;
                AllowMove = true;
                AllowResize = true;
                BringToFront = true;
            }

            public static void Add(Control ctrl)
            // Add control possibility for obj
            {
                ctrl.MouseDown += ctrl_MouseDown;
                ctrl.MouseUp += ctrl_MouseUp;
                ctrl.MouseMove += ctrl_MouseMove;
            }

            private static void ctrl_MouseUp(object sender, MouseEventArgs e)
            // Method mouseUp
            {
                if (e.Button != MouseButtons.Left)
                    return;
                var ctrl = (sender as Control);
                ctrl.Cursor = oldCursor;
                int num = Convert.ToInt32(ctrl.Tag.ToString());
                ctrl.Location = GetPos(ctrl.Location.X, num);
            }

            public static void Remove(Control ctrl)
            // Remove control possibility for obj
            {
                ctrl.MouseDown -= ctrl_MouseDown;
                ctrl.MouseUp -= ctrl_MouseUp;
                ctrl.MouseMove -= ctrl_MouseMove;
            }

            static void ctrl_MouseMove(object sender, MouseEventArgs e)
            // Method move mouse
            {
                var ctrl = sender as Control;
                if (ChangeCursor)
                {
                    if ((e.X >= ctrl.Width - ResizingMargin) && (e.Y >= ctrl.Height - ResizingMargin) && AllowResize)
                        ctrl.Cursor = Cursors.SizeNWSE;
                    else
                    if (AllowMove)
                        ctrl.Cursor = Cursors.SizeAll;
                    else
                        ctrl.Cursor = Cursors.Default;
                }

                if (e.Button != MouseButtons.Left)
                    return;

                var l = ctrl.PointToScreen(e.Location);
                var dx = l.X - startMouse.X;
                var dy = l.Y - startMouse.Y;

                if (Math.Max(Math.Abs(dx), Math.Abs(dy)) > 1)
                {
                    if (resizing)
                    {
                        if (AllowResize)
                        {
                            ctrl.Size = new Size(Math.Max(MinSize, startSize.Width + dx), Math.Max(MinSize, startSize.Height + dy));
                            ctrl.Cursor = Cursors.SizeNWSE;
                            if (BringToFront) ctrl.BringToFront();
                        }
                    }
                    else
                    {
                        if (AllowMove)
                        {
                            Point newLoc = startLocation + new Size(dx, dy);
                            if (newLoc.X < 0) newLoc = new Point(0, newLoc.Y);
                            if (newLoc.Y < 0) newLoc = new Point(newLoc.X, 0);
                            ctrl.Location = newLoc;
                            ctrl.Cursor = Cursors.SizeAll;
                            if (BringToFront) ctrl.BringToFront();
                        }
                    }
                }
            }

            static void ctrl_MouseDown(object sender, MouseEventArgs e)
            // Method Mouse down
            {
                if (e.Button != MouseButtons.Left)
                    return;
                var ctrl = sender as Control;

                if (ctrl.Location.X < 333) // Save number of tower
                    StartPos = 1; // The left tower
                else if (ctrl.Location.X > 666)
                    StartPos = 3; // Right tower
                else
                    StartPos = 2; // Center tower
                StartPosX = ctrl.Location.X;
                StartPosY = ctrl.Location.Y;
                resizing = (e.X >= ctrl.Width - ResizingMargin) && (e.Y >= ctrl.Height - ResizingMargin) && AllowResize;
                startSize = ctrl.Size;
                startMouse = ctrl.PointToScreen(e.Location);
                startLocation = ctrl.Location;
                oldCursor = ctrl.Cursor;
            }

            static bool CheckMove(int num, int Num_Blin)
            {
                // Check correct selection of the tower for the disk
                if (num == 1)
                {
                    if (Num_Blin > FrontOne || num == StartPos)
                        return false;
                }
                else if (num == 2)
                {
                    if (Num_Blin > FrontTwo || num == StartPos)
                        return false;
                }
                else
                {
                    if (Num_Blin > FrontThree || num == StartPos)
                        return false;
                }
                return true;
            }

            public static Point GetPos(int x, int Num_Blin)
            // Set coordinets for disk (MouseUP event)
            // Check correct move position (return start position)
            {

                Point Pos = new Point(); // Position obj

                bool check = true; // Can set select of tower

                if (x < 333) // MouseUp on left tower
                {
                    check = CheckMove(1, Num_Blin); // Is it possible to place it here
                    Pos.X = 15 * (9 - Num_Blin); // Calculating position X
                    Pos.Y = 380 - 40 * OneTower.Count(); // Calculating position Y
                    if (check || Form1.Instance.isAIHelp)
                    // If is correct selected tower then to change values
                    {
                        Form1.Instance.AddRow(StartPos, 1); // Save step
                        OneTower.Push(Num_Blin); // Add weight disk
                        if (StartPos == 2) // Remove weight disk front tower
                            TwoTower.Pop();
                        else if (StartPos == 3)
                            ThreeTower.Pop();
                    }
                }
                else if (x > 666) // MouseUp on right tower
                {
                    check = CheckMove(3, Num_Blin);
                    Pos.X = 665 + 15 * (9 - Num_Blin);
                    Pos.Y = 380 - 40 * ThreeTower.Count();
                    if (check || Form1.Instance.isAIHelp)
                    {
                        Form1.Instance.AddRow(StartPos, 3);
                        ThreeTower.Push(Num_Blin);
                        if (StartPos == 1)
                            OneTower.Pop();
                        else if (StartPos == 2)
                            TwoTower.Pop();
                    }
                }
                else // MouseUp on center tower
                {
                    check = CheckMove(2, Num_Blin);
                    Pos.X = 335 + 15 * (9 - Num_Blin);
                    Pos.Y = 380 - 40 * TwoTower.Count();
                    if (check || Form1.Instance.isAIHelp)
                    {
                        Form1.Instance.AddRow(StartPos, 2);
                        TwoTower.Push(Num_Blin);
                        if (StartPos == 1)
                            OneTower.Pop();
                        else if (StartPos == 3)
                            ThreeTower.Pop();
                    }
                }
                if (!check && !Form1.Instance.isAIHelp) // Return disk on start position
                {
                    Pos.X = StartPosX;
                    Pos.Y = StartPosY;
                }
                if (!Form1.Instance.isAIHelp) // Change control for disks
                    Form1.Instance.CheckControl();
                return Pos;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (isEnglish)
                label1.Text = "Game Size: " + trackBar1.Value.ToString();
            else
                label1.Text = "Размер игры: " + trackBar1.Value.ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            TowerBlins[numblin.Dequeue()].Location = pointblin.Dequeue();
            if (!(numblin.Count > 0))
            {
                pictureBox12.Visible = false;
                timer2.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        // Start game (Player Mode)
        {
            isAIHelp = false;
            StartGame();
        }

        private void button4_Click(object sender, EventArgs e)
        // Translate Rus/Eng
        {
            isEnglish = !isEnglish;
            if (isEnglish)
            {
                button1.Text = "AI Help";
                button2.Text = "Start New Game";
                button3.Text = "FAQ";
                FirstWord = "Move ";
                LastWord = " to ";
                label1.Text = "Game Size: " + trackBar1.Value.ToString();
                label1.Font = new Font(label1.Font.Name, 14, label1.Font.Style);
                button4.Image = Properties.Resources.english_flag;
            }
            else
            {
                button1.Text = "Помощь ПК";
                button2.Text = "Новая игра";
                button3.Text = "Правила";
                FirstWord = "Перемещение ";
                LastWord = " на ";
                label1.Text = "Размер игры: " + trackBar1.Value.ToString();
                label1.Font = new Font(label1.Font.Name, 11, label1.Font.Style);
                button4.Image = Properties.Resources.russia_26896_640;

            }
        }

        private void button3_Click(object sender, EventArgs e)
        // FAQ form window
        {
            Faq = new Form2();
            Faq.Show();
            Faq.SetText(isEnglish);
        }

        private void button1_Click(object sender, EventArgs e)
        // AI Mode
        {
            timer2.Enabled = true;
            isAIHelp = true;
            AIHelp();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }
    }
}
