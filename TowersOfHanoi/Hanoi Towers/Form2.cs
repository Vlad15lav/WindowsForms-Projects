using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hanoi_Towers
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        public void SetText(bool isEnglish)
        // Set FAQ text
        {
            if (isEnglish)
                label1.Text = "The goal of this game is to move the disks from the left side to the tower on the right side. The move is to move one disk to another rod, with the condition that in this tower it is the smallest (in size). Good luck!";
            else
                label1.Text = "Цель этой игры заключается в перемещении дисков с левой стороны на башню с правой стороны. Ход состоит в том, чтобы перенести один диск на другой стержень, с условием что в этой башне он самый маленький (по размеру). Удачи!";
        }
    }
}
