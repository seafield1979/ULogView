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
        #region Properties
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

        private object focusedObject;

        #endregion Properties


        //
        // Constructor
        //
        #region コンストラクタ
        public Form1()
        {
            InitializeComponent();
            Initialize(null);
        }

        public Form1(string logFilePath)
        {
            InitializeComponent();
            Initialize(logFilePath);
        }

        #endregion コンストラクタ

        //
        // Delegate Methods
        //
        #region Delegate
        public void InvalidateDelegate()
        {
            panel2.Invalidate();
        }
        #endregion Delegate

        //
        // Methods::
        //
        #region Normal
        private void Initialize(string logFilePath)
        {
            documentLV = new DocumentLV(panel2.Width, panel2.Height, areaTree, idListBox,
                    hScrollBar1, vScrollBar1, logFilePath, InvalidateDelegate);

            // マウスホイールのイベント登録
            this.MouseWheel += new MouseEventHandler(this.MainForm_MouseWheel);

            focusedObject = panel2;
        }

        // zoom::
        private void ZoomUp()
        {
            if (documentLV.logview.ZoomUp())
            {
                panel1.Invalidate();
            }
        }

        private void ZoomDown()
        {
            if (documentLV.logview.ZoomDown())
            {
                panel1.Invalidate();
            }
            ;
        }

        // pixtime::
        private void PixTimeZoomUp()
        {
            if (documentLV.logview.PixTimeZoomUp())
            {
                panel1.Invalidate();
            }
        }

        private void PixTimeZoomDown()
        {
            if (documentLV.logview.PixTimeZoomDown())
            {
                panel1.Invalidate();
            }
        }

        #endregion




        #region Event


        // form::
        #region Form
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine(String.Format("keydown:{0}", e.KeyValue));

            if (focusedObject == panel2)
            {

                switch ((Keys)e.KeyValue)
                {
                    case Keys.Left:
                        break;
                    case Keys.Up:
                        if (isControl)
                        {
                            ZoomDown();
                        }
                        else
                        {
                            PixTimeZoomDown();
                        }
                        break;
                    case Keys.Right:
                        break;
                    case Keys.Down:
                        if (isControl)
                        {
                            ZoomUp();
                        }
                        else
                        {
                            PixTimeZoomUp();
                        }
                        break;
                    case Keys.ControlKey:
                        isControl = true;
                        break;
                    case Keys.ShiftKey:
                        isShift = true;
                        break;
                    case Keys.PageDown:
                        if (documentLV.logview.ScrollDown())
                        {
                            panel1.Invalidate();
                        }
                        break;
                    case Keys.PageUp:
                        if (documentLV.logview.ScrollUp())
                        {
                            panel1.Invalidate();
                        }
                        break;
                }
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            Debug.WriteLine(String.Format("keypress:{0}", e.KeyChar));

            if (focusedObject == panel2)
            {
                switch ((Keys)e.KeyChar)
                {
                    case Keys.Left:
                        break;
                    case Keys.Control:
                        isControl = true;
                        break;
                    case Keys.Shift:
                        isShift = true;
                        break;
                }
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            Debug.WriteLine(String.Format("keyup:{0}", e.KeyValue));

            if (focusedObject == panel2)
            {
                switch ((Keys)e.KeyValue)
                {
                    case Keys.ControlKey:
                        isControl = false;
                        break;
                    case Keys.Shift:
                        isShift = false;
                        break;
                }
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

        #endregion Form








        // menu::
        #region Menu
        private void 終了XToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        #endregion Menu








        // panel2::
        #region Panel2

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
        /// ログ領域の描画イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            documentLV.Draw(g);
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

        /// <summary>
        /// マウスを移動したときのイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    panel2.Invalidate();
                }
                if (documentLV.logview.ScrollY(moveY) == true)
                {
                    panel2.Invalidate();
                }
            }
        }

        /// <summary>
        /// パネルをクリックしたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel2_Click(object sender, EventArgs e)
        {
            focusedObject = sender;
            var panel = (DoubleBufferingPanel)sender;
            panel.Focus();
        }


        /// <summary>
        /// リサイズ時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel2_Resize(object sender, EventArgs e)
        {
            const int barW = 21;

            var panel = (Panel)sender;

            // 自前で用意したスクロールバーのサイズを更新する
            vScrollBar1.SetBounds(panel.Size.Width - vScrollBar1.Width, 0, barW, panel.Size.Height - barW);
            hScrollBar1.SetBounds(0, panel.Size.Height - barW - 100, panel.Size.Width - barW, barW);

            // 画像サイズを更新
            if (documentLV != null)
            {
                documentLV.Resize(panel.Size.Width, panel.Size.Height);
            }

            //panel1.Invalidate();
        }
        #endregion Panel2













        // treeview::
        #region TreeView
        /// <summary>
        /// TreeViewの項目をクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void areaTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = e.Node;
            if (node == null || node.Tag == null)
            {
                return;
            }

            documentLV.SelectAreaTreeNode((LogArea)node.Tag);
        }

        #endregion TreeView














        // listbox::
        #region ListBox
        /// <summary>
        /// CheckedListBoxの項目をクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void idListBox_MouseClick(object sender, MouseEventArgs e)
        {

            Debug.WriteLine("{0}",e.Location);
        }

        /// <summary>
        /// リストボックスの項目をチェックしたときの処理
        /// all項目がチェックされたら全項目をON/OFFにする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        #endregion ListBox












        // scrollbar::
        #region ScrollBar

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            documentLV.logview.ScrollY(0);
            panel2.Invalidate();
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            panel2.Invalidate();
        }

        #endregion ScrollBar











        // tab::
        #region Tab
        private void tabControl1_Click(object sender, EventArgs e)
        {
            focusedObject = sender;
        }
        #endregion tab












        // button::
        #region Button

        // 表示時間をズームアップ（見える範囲を狭く）
        private void zoomUpButton_Click(object sender, EventArgs e)
        {
            documentLV.logview.PixTimeZoomUp();
        }

        // 表示時間をズームダウン（見える範囲を広い）
        private void zoomDownButton_Click(object sender, EventArgs e)
        {
            documentLV.logview.PixTimeZoomDown();
        }
        #endregion Button

        #endregion Event

        private void button1_Click(object sender, EventArgs e)
        {
            documentLV.logview.ToggleDirection();
            panel2.Invalidate();
        }

        // 全体ズームアップ
        private void button2_Click(object sender, EventArgs e)
        {
            documentLV.logview.ZoomUp();
        }

        // 全体ズームダウン
        private void button3_Click(object sender, EventArgs e)
        {
            documentLV.logview.ZoomDown();
        }
    }
}
 
 