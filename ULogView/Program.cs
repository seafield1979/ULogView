using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ULogView.Utility;

namespace ULogView
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
#if true
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 0) {
                Application.Run(new Form1(args[0]));
            }
#else
            IniFileManager.Singleton.ReadFromFile();

            //IniFileManager.Singleton.SetString("section1", "key1", "hoge");
            //IniFileManager.Singleton.SetString("section1", "key2", "hoge2");
            //IniFileManager.Singleton.SetData("section1", "key3", "100");
            //IniFileManager.Singleton.SetData("section1", "key4", "True");

            //string[] array1 = new string[] { "aaa", "bbb", "ccc" };
            //IniFileManager.Singleton.SetArray("section2", "array1", array1);

            string[] array2;
            IniFileManager.Singleton.GetArray("section2", "array1", out array2);

            IniFileManager.Singleton.WriteToFile();
#endif
        }
    }
}
