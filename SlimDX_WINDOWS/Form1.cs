using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SlimDX_WINDOWS
{
    public partial class Form1 : Form
    {
        public D3D11ENGINE engine;
        public Form1()
        {
            InitializeComponent();
            engine = new D3D11ENGINE();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            engine.OnInitialize(this);
        }
    }
}
