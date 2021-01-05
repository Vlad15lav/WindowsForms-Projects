using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battleship
{
    public partial class Form1 : Form
    {
        //Настройки игры
        //static string Nick_Name = SystemInformation.ComputerName;
        //static string IP_Adress = "127.0.0.1";
        //static int Port = 8910;
        //static int VoiceLevel = 0;
        Graphics g;
        Graphics g_enemy;

        // Параметры игры
        bool CanShot = false; // Можно ли стрелять в противника
        bool isPlacement = false;

        const int N = 10;
        int[] ShipWeights = new int[10] { 1, 2, 2, 3, 3, 3, 4, 4, 4, 4 };
        int[] CountShips = new int[4] {4, 3, 2, 1};

        int[,] Map = new int[N, N]; // Расположение кораблей на карте игрока
        int[] HpMeShips = new int[N]; // Кол-во жиней под индексом
        bool[,] CanShotCell = new bool[N, N]; // Можно ли выстрелить

        int[,] Map_Enemy = new int[N, N]; // Расположение кораблей на карте противника
        int[] HpEnemyShips = new int[N]; // Кол-во жизней под индексом
        bool[,] CanShotPC = new bool[N, N]; // Можно ли стрелять компьютеру

        Queue<Point> PointsFree = new Queue<Point>(); // Точки которые нужно закрыть при убийстве корабля для игрока
        Queue<Point> PointsFreePC = new Queue<Point>(); // Точки которые нужно закрыть при убийстве коряля для противника

        Random Rand = new Random();
        int index_Ship = 1; // Индекс корабля который нужно добавить
        int x_ai = 0;
        int y_ai = 0;

        // Координаты первого ранения одного корабля
        int firstHitX = 0;
        int firstHitY = 0;
        int Num_AI_Shot = 0;

        int dir = 0; // Направление добавание компьютером
        List<int> dirs = new List<int>();

        int PC_Mode = 0; // В каком режими находется компьютер поиск корабля/добавенание
        Point[] PointsForFourShip = new Point[24];
        Point[] PointsForThreeShip = new Point[26];
        Point[] PointsForOneShip = new Point[50];

        public delegate void DrawFunction(int x, int y);

        public Form1()
        {
            InitializeComponent();
            Initialize();
        }

        void Initialize()
        {
            // Инициализируем Матрицу Карты игрока/соперника и прочие вспомогательные матрицы
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                {
                    Map[i, j] = 0;
                    Map_Enemy[i, j] = 0;
                    CanShotCell[i, j] = true;
                    CanShotPC[i, j] = true;
                }
            // Инициализируем Массив, который показывает кол-во жизней у индексов кораблей
            for (int i = 0; i < N; i++)
            {
                HpMeShips[i] = 0;
                HpEnemyShips[i] = 0;
            }
            // Инициализируем списки последовательностей ходов для поиска 4-ех полубного и т.д. для компьютера 
            int k = 0;
            // Заполняем кооржинаты для нахождения 4-ех палубного (PC)
            for (int i = 3, j = 0; i >= 0 && j < N; i--, j++)
            {
                PointsForFourShip[k++] = new Point(i, j);
                PointsForFourShip[k++] = new Point(N - 1 - i, N - 1 - j);
            }
            for (int i = 7, j = 0; i >= 0 && j < N; i--, j++)
            {
                PointsForFourShip[k++] = new Point(i, j);
                PointsForFourShip[k++] = new Point(N - 1 - i, N - 1 - j);
            }
            // Заполняем кооржинаты для нахождения 3-ех палубных (PC)
            k = 0;
            for (int i = 1, j = 0; i >= 0 && j < N; i--, j++)
            {
                PointsForThreeShip[k++] = new Point(i, j);
                PointsForThreeShip[k++] = new Point(N - 1 - i, N - 1 - j);
            }
            for (int i = 5, j = 0; i >= 0 && j < N; i--, j++)
            {
                PointsForThreeShip[k++] = new Point(i, j);
                PointsForThreeShip[k++] = new Point(N - 1 - i, N - 1 - j);
            }
            for (int i = 9, j = 0; i >= 0 && j < N; i--, j++)
            {
                PointsForThreeShip[k++] = new Point(i, j);
            }
            // Заполняем кооржинаты для нахождения 1-но палубных (PC)
            k = 0;
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    if ((i + j) % 2 == 0)
                        PointsForOneShip[k++] = new Point(i, j);

            // Добавляем направления
            dirs.Add(3);
            dirs.Add(2);
            dirs.Add(1);
            dirs.Add(0);


            g = YouField.CreateGraphics(); // Графика для рисование на поле игрока
            g_enemy = EnemyField.CreateGraphics(); // Графика для рисование на поле соперника
        }

        void RandomSwap(Point[] points)
        {
            for (int i = points.Length - 1; i >= 1; i--)
            {
                int j = Rand.Next(i + 1);
                var temp = points[j];
                points[j] = points[i];
                points[i] = temp;
            }
        }

        void RandomSwap(List<int> list_elements)
        {
            list_elements.Clear();
            list_elements.Add(3);
            list_elements.Add(2);
            list_elements.Add(1);
            list_elements.Add(0);
            for (int i = list_elements.Count - 1; i >= 1; i--)
            {
                int j = Rand.Next(i + 1);
                var temp = list_elements[j];
                list_elements[j] = list_elements[i];
                list_elements[i] = temp;
            }
        }

        void ResetBattlefield(int[,] map)
        // Очищаем расстановку своих кораблей
        {
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                {
                    map[i, j] = 0;
                    CanShotCell[i, j] = true;
                    CanShotPC[i, j] = true;
                }
            CountShips = new int[4] { 4, 3, 2, 1 };
            PointsFree.Clear(); // Очищаем очереди у свободных клеток игрока при убийстве корабля
            PointsFreePC.Clear(); // Очищаем очереди у свободных клеток игрока при убийстве корабля
        }

        private void RandomShip_Click(object sender, EventArgs e)
        {
            index_Ship = 1;
            YouField.Refresh(); // Удаляем графики на своем поле
            ResetBattlefield(Map);
            Set_Rand_Ship(Map, 4, 1);
            Set_Rand_Ship(Map, 3, 2);
            Set_Rand_Ship(Map, 2, 3);
            Set_Rand_Ship(Map, 1, 4);
            ShowMap();
        }

        void Set_Rand_Ship(int[,] map, int size_ship, int num_ships)
        ////////////// РАСТАНОВКА КОРАБЛЯ РАНДОМНО (ПОЛЕ БОЯ, ДЛИНА КОРАБЛЯ, КОЛ_ВО)
        {
            int x, y;
            int dir = 0;

            int Count_Tact = 0;
            int Count_Ship = 0;

            while (Count_Ship < num_ships)
            {
                Count_Tact++;
                if (Count_Tact > 1000)
                    // Если не удалось установить корабли,то останавливаем счетчик
                    break;

                x = Rand.Next(101) % N;
                y = Rand.Next(101) % N;

                int temp_x = x;
                int temp_y = y;

                bool setting_is_possible = true;

                // Генератор направления влево, вправо, вверх и вниз
                dir = Rand.Next(4);

                for (int i = 0; i < size_ship; i++)
                {
                    if (x < 0 || y < 0 || x >= N || y >= N)
                    // Проверка выхода за пределы массива
                    {
                        setting_is_possible = false;
                        break;
                    }

                    if (map[x, y] > 0)
                    {
                        setting_is_possible = false;
                        break;
                    }
                    bool check_flag = CheckCell(map, x, y);

                    if (check_flag)
                    {
                        setting_is_possible = false;
                        break;
                    }
                    switch (dir)
                    {
                        case 0:
                            x++;
                            break;
                        case 1:
                            y++;
                            break;
                        case 2:
                            x--;
                            break;
                        case 3:
                            y--;
                            break;
                    }
                }

                if (setting_is_possible)
                {
                    x = temp_x;
                    y = temp_y;

                    for (int i = 0; i < size_ship; i++) // Выставляем направление роста корабля
                    {
                        map[x, y] = index_Ship;////

                        switch (dir)
                        {
                            case 0:
                                x++;
                                break;
                            case 1:
                                y++;
                                break;
                            case 2:
                                x--;
                                break;
                            case 3:
                                y--;
                                break;
                        }
                    }
                    HpMeShips[index_Ship - 1] = size_ship;
                    HpEnemyShips[index_Ship - 1] = size_ship;
                    Count_Ship++;
                    index_Ship++; //Прибавляем номер корабля
                }
            }
        }

        bool Set_Ship(int x, int y, int[,] map, int dir, int size_ship)
        ////////////// РАСТАНОВКА КОРАБЛЯ (ПОЛЕ БОЯ, ДЛИНА КОРАБЛЯ, КОЛ_ВО)
        {
            int temp_x = x;
            int temp_y = y;

            bool setting_is_possible = true;

            for (int i = 0; i < size_ship; i++)
            {
                if (x < 0 || y < 0 || x >= N || y >= N)
                // Проверка выхода за пределы массива
                {
                    setting_is_possible = false;
                    break;
                }

                if (map[x, y] > 0)
                {
                    setting_is_possible = false;
                    break;
                }
                bool check_flag = CheckCell(map, x, y);

                if (check_flag)
                {
                    setting_is_possible = false;
                    break;
                }
                switch (dir)
                {
                    case 0:
                        x++;
                        break;
                    case 1:
                        y++;
                        break;
                }
            }

            if (setting_is_possible)
            {
                x = temp_x;
                y = temp_y;

                for (int i = 0; i < size_ship; i++) // Выставляем направление роста корабля
                {
                    map[x, y] = index_Ship;

                    switch (dir)
                    {
                        case 0:
                            x++;
                            break;
                        case 1:
                            y++;
                            break;
                    }
                }
                HpMeShips[index_Ship - 1] = size_ship;
                index_Ship++;
            }
            return setting_is_possible;
        }

        bool CheckCell(int[,] map, int x, int y)
        // Проверяет все соседние клетки
        {
            bool check_flag = false;
            try
            {
                if (map[x, y + 1] > 0)
                    check_flag = true;
            }
            catch { }
            try
            {
                if (map[x, y - 1] > 0)
                    check_flag = true;
            }
            catch { }
            try
            {
                if (map[x + 1, y] > 0)
                    check_flag = true;
            }
            catch { }
            try
            {
                if (map[x + 1, y + 1] > 0)
                    check_flag = true;
            }
            catch { }
            try
            {
                if (map[x + 1, y - 1] > 0)
                    check_flag = true;
            }
            catch { }
            try
            {
                if (map[x - 1, y] > 0)
                    check_flag = true;
            }
            catch { }
            try
            {
                if (map[x - 1, y + 1] > 0)
                    check_flag = true;
            }
            catch { }
            try
            {
                if (map[x - 1, y - 1] > 0)
                    check_flag = true;
            }
            catch { }
            return check_flag;
        }

        void FillCell(int[,] map, DrawFunction DrawFun, bool[,] CanShot, int x, int y)
        {
            try
            {
                if (map[x, y + 1] <= 0)
                {
                    DrawFun(x + 1, y + 2);
                    CanShot[x, y + 1] = false;
                }
            }
            catch { }
            try
            {
                if (map[x, y - 1] <= 0)
                {
                    DrawFun(x + 1, y);
                    CanShot[x, y - 1] = false;
                }
            }
            catch { }
            try
            {
                if (map[x + 1, y] <= 0)
                {
                    DrawFun(x + 2, y + 1);
                    CanShot[x + 1, y] = false;
                }
            }
            catch { }
            try
            {
                if (map[x + 1, y + 1] <= 0)
                {
                    DrawFun(x + 2, y + 2);
                    CanShot[x + 1, y + 1] = false;
                }
            }
            catch { }
            try
            {
                if (map[x + 1, y - 1] <= 0)
                {
                    DrawFun(x + 2, y);
                    CanShot[x + 1, y - 1] = false;
                }
            }
            catch { }
            try
            {
                if (map[x - 1, y] <= 0)
                {
                    DrawFun(x, y + 1);
                    CanShot[x, y + 1] = false;
                }
            }
            catch { }
            try
            {
                if (map[x - 1, y + 1] <= 0)
                {
                    DrawFun(x, y + 2);
                    CanShot[x - 1, y + 1] = false;
                }
            }
            catch { }
            try
            {
                if (map[x - 1, y - 1] <= 0)
                {
                    DrawFun(x, y);
                    CanShot[x - 1, y - 1] = false;
                }
            }
            catch { }
        }

        //////////////////// ГРАФИКА
        public void Draw(int x, int y) // Рисуем корабли на своем поле
        {
            g.FillRectangle(Brushes.Green, 35 + x * 29 + x / 2, 82 + y * 26 - y / 2, 29.5f, 26);
        }

        public void DrawMe(int x, int y) // Рисует промахи соперника
        {
            x--;
            y--;
            g.FillRectangle(Brushes.Blue, 35 + x * 29 + x / 2 + 6, 82 + y * 26 - y / 2 + 5, 29.5f / 2, 26 / 2);
        }
        public void DrawMeHit(int x, int y) // Рисует попадания соперника
        {
            x--;
            y--;
            g.FillRectangle(Brushes.Red, 35 + x * 29 + x / 2 + 6, 82 + y * 26 - y / 2 + 5, 29.5f / 2, 26 / 2);
        }

        public void DrawEnemy(int a, int b) // Рисует промахи игрока
        {
            g_enemy.FillRectangle(Brushes.Blue, 26 + (a - 1) * 29 + (a - 1) / 2 + 6, 82 + (b - 1) * 26 - (b - 1) / 2 + 5, 29.5f / 2, 26 / 2);
        }
        public void DrawEnemyHit(int a, int b) // Рисует попадания игрока
        {
            g_enemy.FillRectangle(Brushes.OrangeRed, 26 + (a - 1) * 29 + (a - 1) / 2, 82 + (b - 1) * 26 - (b - 1) / 2, 29.5f, 26);
        }

        void FillAllPoint()
        //Раскрашиваем все точки с листка PointsFree
        {
            while (PointsFree.Count() != 0)
            {
                Point temp = PointsFree.Dequeue();
                //Console.WriteLine(temp);
                //FillCellEnemy(Map_Enemy, temp.X - 1, temp.Y - 1);
                FillCell(Map_Enemy, DrawEnemy, CanShotCell, temp.X - 1, temp.Y - 1);
            }
        }

        void FillAllPointPC()
        //Раскрашиваем все точки с листка PointsFree
        {
            while (PointsFreePC.Count() != 0)
            {
                Point temp = PointsFreePC.Dequeue();
                //Console.WriteLine(temp);
                //FillCellMe(Map, temp.X, temp.Y); //
                FillCell(Map, DrawMe, CanShotPC, temp.X, temp.Y); //
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////// 

        // Начальная точка 62, 175
        void ShowMap() // Показать свои корабли
        {
            g = YouField.CreateGraphics();
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    if (Map[i, j] > 0)
                        Draw(i, j);
        }

        private void btnSinglePlayer_Click(object sender, EventArgs e)
        // Режим против компьютера
        {
            index_Ship = 1;
            YouField.Refresh(); // Удаляем графики на своем поле
            ResetBattlefield(Map);
            ResetRadioBtn();
            StartSingle.Visible = true;
            groupShips.Visible = true;
            comboBox1.Text = "Horizontally";
            isPlacement = true;
        }

        private void EnemyField_MouseClick(object sender, MouseEventArgs e)
        // СТРЕЛЬБА ПО ПРОТИВНИКУ
        {
            if (!CanShot)
                return;
            int x = e.X;
            int y = e.Y;
            if (x < 26 || y < 82 || x > 320 || y > 335)
                return;
            // Получаем номер клетки
            x = (x - 26) / 29 + 1;
            y = (y - 82) / 25 + 1;
            if (x == 11) x--;
            if (y == 11) y--;

            if (!CanShotCell[x - 1, y - 1]) // Стреляли ли уже в эту клетку
                return;
            else
                CanShotCell[x - 1, y - 1] = false;

            if (Map_Enemy[x - 1, y - 1] > 0)
            {
                DrawEnemyHit(x, y); // Помечаем попадание
                PointsFree.Enqueue(new Point(x, y)); // Добавляем точку вокруг которого надо будет расскрасить при убийстве корабля
                HpEnemyShips[Map_Enemy[x - 1, y - 1] - 1]--; // Вычитаем жизни корабля с индексом
                if (HpEnemyShips[Map_Enemy[x - 1, y - 1] - 1] <= 0) // Проверяем есть ли хп у корабля
                {
                    label8.Text = "Destroy";
                    FillAllPoint(); // Очищаем клетки вокруг корабля
                    if (CheckWinner())
                    {
                        CanShot = false;
                        label5.Text = "Enemy turn";
                        return;
                    }
                }
                else
                {
                    label8.Text = "Hit";
                }
            }
            else
            {
                DrawEnemy(x, y);
                label8.Text = "Miss";
                CanShot = false;
                label5.Text = "Enemy turn";
                AI_Shot();
            }
        }

        bool CheckWinner()
        {
            int HPPlayer = 0, HPEnemy = 0;
            for (int i = 0; i < N; i++)
            {
                HPEnemy += HpEnemyShips[i] * ShipWeights[i];
                HPPlayer += HpMeShips[i] * ShipWeights[i];
            }
            progressBar1.Value = 50 + (50 - HPEnemy) - (50 - HPPlayer);
            if (HPEnemy == 0)
            {
                MessageBox.Show("You are Winner!");
                return true;
            }
            if (HPPlayer == 0)
            {
                MessageBox.Show("You are loss!");
                return true;
            }
            return false;
        }

        void AI_Shot()
        {
            Thread.Sleep(1000);
            if (PC_Mode == 0) // Режим поиска кораблей
            {
                if (Num_AI_Shot <= 23) // Ищем 4-рех палубный
                {
                    x_ai = PointsForFourShip[Num_AI_Shot].X;
                    y_ai = PointsForFourShip[Num_AI_Shot].Y;
                    Num_AI_Shot++;
                }
                else if (Num_AI_Shot <= 49) // Ищем 3-рех палубные, если прошли по всем точкам для 4-рех палубного
                {
                    x_ai = PointsForThreeShip[Num_AI_Shot - 24].X;
                    y_ai = PointsForThreeShip[Num_AI_Shot - 24].Y;
                    Num_AI_Shot++;
                }
                else // Ищем 1-но палубные корабли
                {
                    x_ai = PointsForOneShip[Num_AI_Shot - 50].X;
                    y_ai = PointsForOneShip[Num_AI_Shot - 50].Y;
                    Num_AI_Shot++;
                }

                if (!CanShotPC[x_ai, y_ai])
                {
                    AI_Shot();
                    return;
                }

                if (Map[x_ai, y_ai] > 0) // Попали ли в какой либо корабль
                {
                    DrawMeHit(x_ai+1, y_ai+1);
                    PC_Mode = 1; // Ставим режим добиваная ранненого корабля
                    firstHitX = x_ai; // Сохраняем координаты попадание X 
                    firstHitY = y_ai; // Сохраняем координаты попадание Y

                    if (dirs.Count != 0)
                    {
                        dir = dirs[dirs.Count - 1];
                        dirs.RemoveAt(dirs.Count - 1);
                    }
                    label8.Text = "Hit";//
                    CanShotPC[x_ai, y_ai] = false;
                }
                else
                {
                    DrawMe(x_ai+1, y_ai+1);
                    label8.Text = "Miss";
                    CanShotPC[x_ai, y_ai] = false;
                    CanShot = true;
                    label5.Text = "Your turn";
                    return;
                }
            }
            else if (PC_Mode == 1) // Добиваем корабль
            {
                bool ChangeDir = false;
                if (dir == 0) // Движение влево
                {
                    if (x_ai > 0)
                        x_ai--;
                    else
                    {
                        ChangeDir = true;
                    }
                }
                else if (dir == 1) // Движение вправо
                {
                    if (x_ai < N - 1)
                        x_ai++;
                    else
                    {
                        ChangeDir = true;
                    }
                }
                else if (dir == 2) // Движение вверх
                {
                    if (y_ai > 0)
                        y_ai--;
                    else
                    {
                        ChangeDir = true;
                    }
                }
                else if (dir == 3) // Движение вниз
                {
                    if (y_ai < N - 1)
                        y_ai++;
                    else
                    {
                        ChangeDir = true;
                    }
                }

                if (ChangeDir || !CanShotPC[x_ai, y_ai]) // Проверяем нужно ли поменять направление
                {
                    if (dirs.Count != 0)
                    {
                        dir = dirs[dirs.Count - 1];
                        dirs.RemoveAt(dirs.Count - 1);
                    }
                    x_ai = firstHitX;
                    y_ai = firstHitY;
                    AI_Shot();
                    return;
                }
            }

            // Коря закрывает клетки после убийства PointsFreePC.Enqueue(new Point(x + 1, y + 1));
            if (Map[x_ai, y_ai] > 0 && HpMeShips[Map[firstHitX, firstHitY] - 1] > 1)
            {
                DrawMeHit(x_ai+1, y_ai+1);
                HpMeShips[Map[firstHitX, firstHitY] - 1]--; //
                PointsFreePC.Enqueue(new Point(x_ai, y_ai));
                label8.Text = "Hit";
                CanShotPC[x_ai, y_ai] = false;
            }
            else if (Map[x_ai, y_ai] > 0 && HpMeShips[Map[firstHitX, firstHitY] - 1] == 1)
            {
                HpMeShips[Map[firstHitX, firstHitY] - 1]--;
                DrawMeHit(x_ai+1, y_ai+1);
                label8.Text = "Destroy";
                PointsFreePC.Enqueue(new Point(x_ai, y_ai));
                FillAllPointPC();
                PC_Mode = 0;
                CanShotPC[x_ai, y_ai] = false;
                RandomSwap(dirs);
                if (CheckWinner())
                    return;
            }
            else
            {
                DrawMe(x_ai+1, y_ai+1);
                CanShotPC[x_ai, y_ai] = false;
                if (dirs.Count != 0)
                {
                    dir = dirs[dirs.Count - 1];
                    dirs.RemoveAt(dirs.Count - 1);
                }
                x_ai = firstHitX;
                y_ai = firstHitY;

                label8.Text = "Miss";
                CanShot = true;
                label5.Text = "Your turn";
                return;
            }

            AI_Shot();
        }

        private void ResetRadioBtn()
        {
            foreach (RadioButton RadioButton_X in groupShips.Controls.OfType<RadioButton>())
            {
                RadioButton_X.Checked = false;
                RadioButton_X.Enabled = false;
            }
            radioButton4.Checked = true;
        }

        private void StartSingle_Click(object sender, EventArgs e)
        {
            index_Ship = 1;
            Num_AI_Shot = 0;
            PC_Mode = 0;
            EnemyField.Refresh();
            ResetBattlefield(Map_Enemy);
            RandomSwap(PointsForFourShip);
            RandomSwap(PointsForThreeShip);
            RandomSwap(PointsForOneShip);
            Set_Rand_Ship(Map_Enemy, 4, 1);
            Set_Rand_Ship(Map_Enemy, 3, 2);
            Set_Rand_Ship(Map_Enemy, 2, 3);
            Set_Rand_Ship(Map_Enemy, 1, 4);
            CanShot = true;
            StartSingle.Visible = false;
            groupShips.Visible = false;
        }

        private void YouField_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isPlacement)
                return;

            int dir;
            int x = e.X;
            int y = e.Y;
            if (x < 26 || y < 82 || x > 320 || y > 335)
                return;
            // Получаем номер клетки
            x = (x - 26) / 29 + 1;
            y = (y - 82) / 25 + 1;
            if (x == 11) x--;
            if (y == 11) y--;

            if (comboBox1.Text == "Horizontally")
                dir = 0;
            else
                dir = 1;

            if (radioButton1.Checked && CountShips[0] > 0)
            {
                if (Set_Ship(x - 1, y - 1, Map, dir, 1)) CountShips[0]--;
                if (CountShips[0] == 0)
                {
                    radioButton1.Enabled = false;
                }
            }
            else if (radioButton2.Checked && CountShips[1] > 0)
            {
                if (Set_Ship(x - 1, y - 1, Map, dir, 2)) CountShips[1]--;
                if (CountShips[1] == 0)
                {
                    radioButton1.Checked = true;
                }
            }
            else if (radioButton3.Checked && CountShips[2] > 0)
            {
                if (Set_Ship(x - 1, y - 1, Map, dir, 3)) CountShips[2]--;
                if (CountShips[2] == 0)
                {
                    radioButton2.Checked = true;
                }
            }
            else if (radioButton4.Checked && CountShips[3] > 0)
            {
                if (Set_Ship(x - 1, y - 1, Map, dir, 4)) CountShips[3]--;
                if (CountShips[3] == 0)
                {
                    radioButton3.Checked = true;
                }
            }
            ShowMap();
        }
    }
}
