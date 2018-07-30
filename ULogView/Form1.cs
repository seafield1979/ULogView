using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace ULogView
{
    public delegate void InvalidateForm();

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

        public Form1(string logFilePath) : this()
        {
            documentLV.ReadLogFile(logFilePath);
        }

        //
        // Delegate Methods
        //
        public void InvalidateDelegate()
        {
            panel2.Invalidate();
        }
        
        //
        // Methods
        //
        #region Normal
        private void Initialize()
        {
            documentLV = new DocumentLV(areaTree, idListBox, panel2.Width, panel2.Height, InvalidateDelegate);
            //panel2.VerticalScroll.Enabled = true;
            //panel2.VerticalScroll.Visible = true;
            //panel2.VerticalScroll.Minimum = 0;
            //panel2.VerticalScroll.Maximum = 1000;
            //panel2.VerticalScroll.Value = 500;

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


        /**
         * TreeViewの項目をクリック
         */
        private void areaTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = e.Node;
            if (node == null || node.Tag == null)
            {
                return;
            }

            documentLV.SelectAreaTreeNode((LogArea)node.Tag);
        }

        /**
         * CheckedListBoxの項目をクリック
         */
        private void idListBox_MouseClick(object sender, MouseEventArgs e)
        {

            Debug.WriteLine("{0}",e.Location);
        }
        #endregion

        private void idListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            String s = idListBox.Items[e.Index].ToString();

            if (s != null && s.Equals("all"))
            {
                //全項目のチェックを all の項目のチェックに合わせる
                bool check = e.NewValue == CheckState.Checked ? true : false;
                for (int i = 0; i < idListBox.Items.Count; i++)
                {
                    if (idListBox.Items[i].ToString().Equals("all"))
                    {
                        continue;
                    }
                    idListBox.SetItemChecked(i, check);
                }
            }
        }

        private void panel2_Click(object sender, EventArgs e)
        {
            //panel2.Invalidate();
        }

        private void panel2_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                Console.WriteLine("垂直方向に{0}スクロールされました",
                    e.NewValue - e.OldValue);
            }
            else if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                Console.WriteLine("水平方向に{0}スクロールされました",
                    e.NewValue - e.OldValue);
            }
        }

        private void panel2_Resize(object sender, EventArgs e)
        {

        }
    }
}
 
 