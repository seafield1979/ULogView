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
    public delegate void UpdateScrollV(int value, int max, int largePage);
    public delegate void UpdateScrollH(int value, int max, int largePage);
    
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
        private bool isMouseDown;
        private Point mouseDownPos;
        private Point mouseOldPos;

        private bool isControl;
        private bool isShift;

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
            documentLV = new DocumentLV(panel2.Width, panel2.Height, areaTree, idListBox, hScrollBar1, vScrollBar1, InvalidateDelegate);

            // マウスホイールのイベント登録
            this.MouseWheel += new MouseEventHandler(this.MainForm_MouseWheel);

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

        /// <summary>
        /// マウスホイールをスクロール
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            // ホイール量は e.Delta
            if (documentLV.logview.ScrollY(-e.Delta) == true)
            {
                panel2.Invalidate();
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

        #region ScrollBar
        //private void panel2_Scroll(object sender, ScrollEventArgs e)
        //{
        //    if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
        //    {
        //        documentLV.ScrollV(e.NewValue);
        //    }
        //    else if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
        //    {
        //        documentLV.ScrollH(e.NewValue);
        //    }
        //}

        private void panel2_Resize(object sender, EventArgs e)
        {
            const int barW = 21;

            var panel = (Panel)sender;

            // 自前で用意したスクロールバーのサイズを更新する
            vScrollBar1.SetBounds(panel.Size.Width - vScrollBar1.Width, 0, barW, panel.Size.Height - barW);
            hScrollBar1.SetBounds(0, panel.Size.Height - barW - 100, panel.Size.Width - barW, barW);

            // 画像サイズを更新
            documentLV.Resize(panel.Size.Width, panel.Size.Height);

            //panel1.Invalidate();
        }
        #endregion ScrollBar

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            panel2.Invalidate();
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            panel2.Invalidate();
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDown = true;
            mouseDownPos = e.Location;
            mouseOldPos = e.Location;
        }

        private void panel2_MouseLeave(object sender, EventArgs e)
        {
            isMouseDown = false;
        }

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
        }
        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                int moveX = e.X - mouseOldPos.X;
                int moveY = e.Y - mouseOldPos.Y;
                mouseOldPos.X = e.X;
                mouseOldPos.Y = e.Y;

                // ホイール量は e.Delta
                if (documentLV.logview.ScrollX(moveX) == true)
                {
                    panel1.Invalidate();
                }
                if (documentLV.logview.ScrollY(moveY) == true)
                {
                    panel1.Invalidate();
                }
            }
        }

    }
}
 
 