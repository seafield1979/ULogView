using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ULogView
{
    public partial class Form1 : Form
    {
        //
        // Properties
        // 
        private DocumentLV documentLV;

        public DocumentLV DocumentLV
        {
            get { return documentLV; }
            set { documentLV = value; }
        }


        //
        // Constructor
        //
        public Form1()
        {
            InitializeComponent();
            Initialize();
        }

        //
        // Methods
        //
        #region Normal
        private void Initialize()
        {
            documentLV = new DocumentLV();
        }

        #endregion

        #region Event
        private void 終了XToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void panel2_DragEnter(object sender, DragEventArgs e)
        {
            // ファイルをドロップできるようにする
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void panel2_DragDrop(object sender, DragEventArgs e)
        {
            // ドロップしたファイルを取得
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            foreach (var fileName in fileNames)
            {
                System.Diagnostics.Debug.Print(fileName);
                documentLV.ReadLogFile(fileName);
            }
        }

        /**
         * ログ領域の描画
         */
        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            documentLV.Draw(g);

            
        }

        #endregion
    }
}
 
 