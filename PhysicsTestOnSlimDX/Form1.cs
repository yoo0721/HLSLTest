﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PhysicsTestOnSlimDX
{
    public partial class Form1 : Form
    {
        public Engine3D engine;
        Particle particle;
        bool isCreated = false;
        public Form1()
        {
            InitializeComponent();
            engine = new Engine3D();
        }

        ~Form1()
        {
            engine.Dispose();
            particle.Dispose();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            
            particle = new Particle(engine);
            engine.SetRenderObject(particle);
            engine.OnInitialize(this);
            isCreated = true;
        }
        
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D:
                    engine.camera.MoveLR(-1.0f);
                    break;
                case Keys.A:
                    engine.camera.MoveLR(1.0f);
                    break;
                case Keys.W:
                    engine.camera.MoveFB(1.0f);
                    break;
                case Keys.S:
                    engine.camera.MoveFB(-1.0f);
                    break;
                case Keys.Q:
                    engine.camera.MoveUD(1.0f);
                    break;
                case Keys.E:
                    engine.camera.MoveUD(-1.0f);
                    break;
                case Keys.Left:
                    engine.camera.RotationAxis(0.1f);
                    break;
                case Keys.Right:
                    engine.camera.RotationAxis(-0.1f);
                    break;
                case Keys.Up:
                    engine.camera.RotationZX(-0.1f);
                    break;
                case Keys.Down:
                    engine.camera.RotationZX(0.1f);
                    break;
                case Keys.D1:
                    engine.camera.Speed += 1.0f;
                    break;
                case Keys.D2:
                    engine.camera.Speed -= 1.0f;
                    break;
            }
        }
    }
}
