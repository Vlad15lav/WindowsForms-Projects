using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Enigma
{
    class ButtonCircle : Button
    {
        protected override void OnPaint(PaintEventArgs pavent)
        {
            GraphicsPath graphics = new GraphicsPath();
            graphics.AddEllipse(0, 0, 50, 50);
            this.Region = new System.Drawing.Region(graphics);
            base.OnPaint(pavent);
        }
    }
}
