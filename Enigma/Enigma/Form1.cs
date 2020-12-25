using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.IO;

namespace Enigma
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeValue();
        }

        // A - 65 Z - 90 code symbol
        char[] PlugboardInput; // Switch (Enigma 4 version)
        char[] PlugboardOutput;
        // Reflector backward
        string Reflector = "YRUHQSLDPXNGOKMIEBFZCWVJAT";
        // Reflector forward
        string Rotor1 = "EKMFLGDQVZNTOWYHXUSPAIBRCJ";
        int Rotor1Rotate = 17;
        string Rotor2 = "AJDKSIRUXBLHWTMCQGZNPYFVOE";
        int Rotor2Rotate = 5;
        string Rotor3 = "BDFHJLCPRTXVZNYEIWGAKMUSQO";
        int Rotor3Rotate = 22;
        string Rotor4 = "ESOVPZJAYQUIRHXLNFTGKDCMWB";
        int Rotor4Rotate = 10;
        string Rotor5 = "VZBRGITYUPSDMHLXAWMJQOFECK";
        int Rotor5Rotate = 0;

        // Value for turning the next rotor
        int RotorSetTwoRotate;
        int RotorSetThreeRotate;

        // Value on the rotor
        string RotorSetOne;
        string RotorSetTwo;
        string RotorSetThree;

        // Rotor belt
        int[] RotorOnePosition;
        int[] RotorTwoPosition;
        int[] RotorThreePosition;

        // Current position
        int PosOne;
        int PosTwo;
        int PosThree;

        Label[] LabelsLetter = new Label[26];

        SoundPlayer btnDown; // Sound button down
        SoundPlayer btnUp; // Sound button up

        void InitializeValue()
        {
            Stream str = Properties.Resources.BtnDown; // Recording default sound
            btnDown = new SoundPlayer(str);
            str = Properties.Resources.BtnUp;
            btnUp = new SoundPlayer(str);

            PosOne = PosTwo = PosThree = 0; // Start position
            SetRotors(Rotor1, Rotor2, Rotor2Rotate, Rotor3, Rotor3Rotate);
            PlugboardInput = new char[26];
            PlugboardOutput = new char[26];
            RotorOnePosition = new int[26];
            RotorTwoPosition = new int[26];
            RotorThreePosition = new int[26];
            for (int i = 0, j = 65; i < 26; i++, j++)
            {
                RotorOnePosition[i] = RotorTwoPosition[i] = RotorThreePosition[i] = j;
                PlugboardInput[i] = PlugboardOutput[i] = (char)j;
                dataGridView1.Rows.Add(PlugboardInput[i], PlugboardOutput[i]);
            }
            RotorRotate(0, 1);
            RotorRotate(0, 2);
            RotorRotate(0, 3);
            comboBox1.Text = comboBox2.Text = comboBox3.Text = "A";
            comboBox4.Text = "I";
            comboBox5.Text = "II";
            comboBox6.Text = "III";

            LabelsLetter[0] = label14;
            LabelsLetter[1] = label10;
            LabelsLetter[2] = label8;
            LabelsLetter[3] = label16;
            LabelsLetter[4] = label28;
            LabelsLetter[5] = label17;
            LabelsLetter[6] = label18;
            LabelsLetter[7] = label19;
            LabelsLetter[8] = label23;
            LabelsLetter[9] = label20;
            LabelsLetter[10] = label21;
            LabelsLetter[11] = label13;
            LabelsLetter[12] = label12;
            LabelsLetter[13] = label11;
            LabelsLetter[14] = label22;
            LabelsLetter[15] = label1;
            LabelsLetter[16] = label30;
            LabelsLetter[17] = label27;
            LabelsLetter[18] = label15;
            LabelsLetter[19] = label26;
            LabelsLetter[20] = label24;
            LabelsLetter[21] = label9;
            LabelsLetter[22] = label29;
            LabelsLetter[23] = label7;
            LabelsLetter[24] = label2;
            LabelsLetter[25] = label25;
        }

        int GetLetter(int index)
        // Engima encrypt
        {
            // index 0 - 26
            // Turn rotor in begin
            PosThree++;
            if (PosThree == RotorSetThreeRotate) // Position for the next rotation of the rotor
            {
                PosTwo++;
                if (PosTwo == RotorSetTwoRotate) // Position for the next rotation of the rotor
                {
                    PosOne++;
                    PosOne %= 26;
                    comboBox1.Text = ((char)(PosOne + 65)).ToString();
                    RotorRotate(PosOne, 1);
                }
                PosTwo %= 26;
                comboBox2.Text = ((char)(PosTwo + 65)).ToString();
                RotorRotate(PosTwo, 2);
            }
            PosThree %= 26;
            comboBox3.Text = ((char)(PosThree + 65)).ToString();
            RotorRotate(PosThree, 3);
            // Search for a letter
            // Forward pass
            int code = (int)PlugboardOutput[index] - 65; // Char to int 0-32
            code = Math.Abs(code + 26 + RotorThreePosition[0] - 65) % 26; // The first rotor
            code = Convert.ToInt32(RotorSetThree[code]) - 65;
            code = Math.Abs(code + 26 + (RotorTwoPosition[0] - RotorThreePosition[0])) % 26; // The second rotor
            code = Convert.ToInt32(RotorSetTwo[code]) - 65;
            code = Math.Abs(code + 26 + (RotorOnePosition[0] + 26 - RotorTwoPosition[0])) % 26; // The third rotor
            code = Convert.ToInt32(RotorSetOne[code]) - 65;
            code = Math.Abs(code + 26 - RotorOnePosition[0] + 65) % 26;
            code = Convert.ToInt32(Reflector[code]) - 65; // Reflector
            code = Math.Abs(code + 26 + RotorOnePosition[0] - 65) % 26 + 65;

            // Backward pass
            // The third rotor
            for (int i = 0; i < 26; i++)
                if (RotorSetOne[i] == (char)code)
                {
                    code = i;
                    break;
                }
            // The second rotor
            code = Math.Abs(code + 26 * 2 - (RotorOnePosition[0] + 26 - RotorTwoPosition[0])) % 26 + 65;
            for (int i = 0; i < 26; i++)
                if (RotorSetTwo[i] == (char)code)
                {
                    code = i;
                    break;
                }
            // The first rotor
            code = Math.Abs(code + 26 * 2 - (RotorTwoPosition[0] + 26 - RotorThreePosition[0])) % 26 + 65;

            for (int i = 0; i < 26; i++)
                if (RotorSetThree[i] == (char)code)
                {
                    code = i;
                    break;
                }
            // Output result
            code = Math.Abs(code + 26 - RotorThreePosition[0] + 65) % 26;
            code = PlugboardOutput[code] - 65;
            return code;
        }

        void RotorRotate(int x, int num)
        // Rotate rotor
        {
            int j = 65;
            int i = 0;
            int X = x % 26;
            if (num == 1)
            {
                PosOne = X;
                RotateGearOne();
            }
            else if (num == 2)
            {
                PosTwo = X;
                RotateGearTwo();
            }
            else
            {
                PosThree = X;
                RotateGearThree();
            }
            for (int k = 26 - X; k < 26; k++)
            {
                if (num == 1)
                    RotorOnePosition[k] = j++;
                else if (num == 2)
                    RotorTwoPosition[k] = j++;
                else
                    RotorThreePosition[k] = j++;
            }
            for (; i < 26 - X; i++)
            {
                if (num == 1)
                    RotorOnePosition[i] = j++;
                else if (num == 2)
                    RotorTwoPosition[i] = j++;
                else
                    RotorThreePosition[i] = j++;
            }
        }

        void SetRotors (string one, string two, int RotTwo, string three, int RotThree)
        // Set rotor values
        {
            RotorSetOne = one;

            RotorSetTwo = two;
            RotorSetTwoRotate = RotTwo;

            RotorSetThree = three;
            RotorSetThreeRotate = RotThree;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RotorRotate((int)comboBox1.Text[0] - 65, 1);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            RotorRotate((int)comboBox2.Text[0] - 65, 2);
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            RotorRotate((int)comboBox3.Text[0] - 65, 3);
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        // Set txt rotor value for the first rotor
        {
            switch(comboBox4.Text)
            {
                case "I":
                    RotorSetOne = Rotor1;
                    break;
                case "II":
                    RotorSetOne = Rotor2;
                    break;
                case "III":
                    RotorSetOne = Rotor3;
                    break;
                case "IV":
                    RotorSetOne = Rotor4;
                    break;
                case "V":
                    RotorSetOne = Rotor5;
                    break;
            }
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        // Set txt rotor value for the second rotor
        {
            switch (comboBox5.Text)
            {
                case "I":
                    RotorSetTwo = Rotor1;
                    RotorSetTwoRotate = Rotor1Rotate;
                    break;
                case "II":
                    RotorSetTwo = Rotor2;
                    RotorSetTwoRotate = Rotor2Rotate;
                    break;
                case "III":
                    RotorSetTwo = Rotor3;
                    RotorSetTwoRotate = Rotor3Rotate;
                    break;
                case "IV":
                    RotorSetTwo = Rotor4;
                    RotorSetTwoRotate = Rotor4Rotate;
                    break;
                case "V":
                    RotorSetTwo = Rotor5;
                    RotorSetTwoRotate = Rotor5Rotate;
                    break;
            }
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        // Set txt rotor value for the third rotor
        {
            switch (comboBox6.Text)
            {
                case "I":
                    RotorSetThree = Rotor1;
                    RotorSetThreeRotate = Rotor1Rotate;
                    break;
                case "II":
                    RotorSetThree = Rotor2;
                    RotorSetThreeRotate = Rotor2Rotate;
                    break;
                case "III":
                    RotorSetThree = Rotor3;
                    RotorSetThreeRotate = Rotor3Rotate;
                    break;
                case "IV":
                    RotorSetThree = Rotor4;
                    RotorSetThreeRotate = Rotor4Rotate;
                    break;
                case "V":
                    RotorSetThree = Rotor5;
                    RotorSetThreeRotate = Rotor5Rotate;
                    break;
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        // Switching panel
        {
            if (e.RowIndex == -1)
                return;
            if (dataGridView1.Rows[e.RowIndex].Cells[1].Value == null)
            {
                dataGridView1.Rows[e.RowIndex].Cells[1].Value = PlugboardOutput[e.RowIndex];
                return;
            }

            int Count = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString().Length;
            char LetterChange = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString().ToUpper()[Count - 1];

            if (LetterChange < 'A' || LetterChange > 'Z')
            // Set switching letter
            {
                dataGridView1.Rows[e.RowIndex].Cells[1].Value = PlugboardOutput[e.RowIndex];
                return;
            }
            else if (LetterChange == PlugboardInput[e.RowIndex])
            // Check repeat
            {
                int index = (int)PlugboardOutput[e.RowIndex] - 65;
                dataGridView1.Rows[index].Cells[1].Value = PlugboardInput[index];
                PlugboardOutput[e.RowIndex] = PlugboardInput[e.RowIndex];
                PlugboardOutput[index] = PlugboardInput[index];
                return;
            }
            else if (PlugboardOutput[(int)LetterChange - 65] != PlugboardInput[(int)LetterChange - 65])
            // Checking the reverse side
            {
                dataGridView1.Rows[e.RowIndex].Cells[1].Value = PlugboardOutput[e.RowIndex];
                return;
            }

            dataGridView1.Rows[e.RowIndex].Cells[1].Value = LetterChange; // Check A B C A | A B C D
            PlugboardOutput[e.RowIndex] = LetterChange;
            dataGridView1.Rows[(int)LetterChange - 65].Cells[1].Value = (char)(e.RowIndex + 65); // Check D B C A | A B C D
            PlugboardOutput[(int)LetterChange - 65] = (char)(e.RowIndex + 65);

        }

        private void button1_Click(object sender, EventArgs e)
        // Reset value on switches
        {
            for (int i = 0, j = 65; i < 26; i++, j++)
            {
                PlugboardOutput[i] = (char)j;
                dataGridView1.Rows[i].Cells[1].Value = (char)j;
            }
        }

        void RotateGearOne()
        // Rotate anim for the first rotor
        {
            Image img = pictureBox2.Image;
            img.RotateFlip(RotateFlipType.Rotate90FlipNone);
            pictureBox2.Image = img;
        }

        void RotateGearTwo()
        // Rotate anim for the second rotor
        {
            Image img = pictureBox3.Image;
            img.RotateFlip(RotateFlipType.Rotate90FlipNone);
            pictureBox3.Image = img;
        }

        void RotateGearThree()
        // Rotate anim for the third rotor
        {
            Image img = pictureBox4.Image;
            img.RotateFlip(RotateFlipType.Rotate90FlipNone);
            pictureBox4.Image = img;
        }

        bool CheckBtnInput = false;
        int IndexLetter = 0;

        private void buttonCircle1_Click(object sender, EventArgs e)
        // Click event, animation
        {
            Button btn = (Button)sender;
            btn.Location = new Point(btn.Location.X, btn.Location.Y - 10);
            CheckBtnInput = false;
            LabelsLetter[IndexLetter].ForeColor = Color.White;
            btnUp.Play();
        }

        private void buttonCircle1_MouseDown(object sender, MouseEventArgs e)
        // MouseDown event, animation
        {
            CheckBtnInput = true;
            Button btnhold = (Button)sender;
            btnhold.Location = new Point(btnhold.Location.X, btnhold.Location.Y + 10);
            IndexLetter = GetLetter((int)(btnhold.Text[0]) - 65);
            textBox2.Text += btnhold.Text[0];
            textBox1.Text += (char)(IndexLetter + 65);
            LabelsLetter[IndexLetter].ForeColor = Color.Yellow;
            btnDown.Play();
        }

        private void buttonCircle1_MouseLeave(object sender, EventArgs e)
        // Activating the lamp
        {
            if (!CheckBtnInput)
                return;
            Button btn = (Button)sender;
            btn.Location = new Point(btn.Location.X, btn.Location.Y - 10);
            CheckBtnInput = false;
            LabelsLetter[IndexLetter].ForeColor = Color.White;
            btnUp.Play();
        }

        private void button2_Click(object sender, EventArgs e)
        // Clear field
        {
            textBox1.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        // Copy text in the buffer
        {
            Clipboard.SetDataObject(textBox1.Text);
        }
    }
}