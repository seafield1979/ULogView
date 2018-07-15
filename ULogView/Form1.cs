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
            documentLV = new DocumentLV(areaTree, idListBox, panel2.Width, panel2.Height );
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
    }
}
 
 