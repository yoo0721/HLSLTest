﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PhysicsTestOnSlimDX
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form;
            using (form = new Form1())
            {
                SlimDX.Windows.MessagePump.Run(form, form.engine.MainLoop);
            }
        }
    }
}
