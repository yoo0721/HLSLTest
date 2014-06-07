using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using SlimDX.Windows;

namespace SlimDX_WINDOWS
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

            Form1 renderForm = new Form1();

            MessagePump.Run(renderForm, renderForm.engine.MainLoop);
        }
    }
}
