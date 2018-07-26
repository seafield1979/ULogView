using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;

namespace ULogView
{
    /**
     * Logviewの表示用のクラス
     */
    public class LogView
    {
        //
        // Enums
        //
        enum DrawDir : byte
        {
            Vertical = 0,
            Horizontal
        }

        //
        // Consts
        //
        // LogViewの表示方向
        const int ViewDirH = 0;
        const int ViewDirV = 1;

        //
        // Properties
        //
        private InvalidateForm delegateInvalidate;

        private LogArea rootArea;       // ルートエリア
        private LogArea currentArea;    // 表示中のエリア(配下のエリアも表示される)
        private LogIDs logIDs;          // LogのID情報
        private Lanes lanes;            // レーン情報
        private IconImages iconImages;  // アイコン画像

        private Image image;            // LogView描画先のImage
        private bool redrawFlag;        // 再描画フラグ(true:再描画あり / false:なし)

        private double topTime;         // 表示領域の先頭の時間
        private double pixPerTime;      // 100pixelあたりの時間 (例:100pix = 1s なら 1)
        private int topPos;             // 表示先頭座標(縦表示ならx、横表示ならy)

        private DrawDir drawDir;        // 表示方向(0: 縦 / 1:横)


        private Dictionary<int, Lane> dispLanes;   // 表示中のレーン(key:LaneId)

        // 
        // Constructor
        //
        public LogView() : this(1000, 1000, null)
        {   
        }

        public LogView(int width, int height, InvalidateForm invalidate1)
        {
            delegateInvalidate = invalidate1;
            Resize(width, height);
            dispLanes = null;
            drawDir = DrawDir.Horizontal;

            Init();
        }
      

        //
        // Methods
        //

        /**
         * 新しいログファイルを読み込んだ場合等の初期化処理
         */
        private void Init()
        {
            redrawFlag = true;
            delegateInvalidate();
        }

        /**
         * 表示領域のサイズが変更された
         */
        public void Resize(int width, int height)
        {
            if (width > 0 && height > 0)
            {
                image = new Bitmap(width, height);
                redrawFlag = true;
            }
        }

        /**
         * LogViewファイルを読み込む
         */
        public bool ReadLogFile(string filePath, TreeView areaTree, CheckedListBox idListBox)
        {
            LogReader reader = new LogReader();

            if (reader.ReadLogFile(filePath) == true)
            {
                rootArea = reader.AreaManager.RootArea;
                currentArea = rootArea;
                logIDs = reader.LogIDs;
                lanes = reader.Lanes;
                iconImages = reader.IconImages;

                DebugPrint();

                UpdateAreaTree(areaTree);
                UpdateLogIDListBox(idListBox);

                Init();
                SetLogArea(currentArea);
            }
            return true;
        }


        /**
         * 表示するエリアを設定する
         * 指定したエリアとその子エリアを表示するように設定する。
         * 
         * @input area : 表示エリア
         */
        private void SetLogArea(LogArea area)
        {
            try
            {
                currentArea = area;

                // 表示するレーンを判定
                dispLanes = LogAreaManager.GetDispLaneList(area, lanes);

                // ログの表示状態を初期状態に戻す
                LogAreaManager.ResetLogData(area);
            }
            catch(Exception e)
            {
                Debug.WriteLine("error " + e.Message);
            }
        }


        #region Draw

        public void Draw(Graphics g)
        {
            DrawBG(g);
            DrawLog(g);
        }

        public void DrawBG(Graphics g)
        {
            if (redrawFlag)
            {
                redrawFlag = false;

                Graphics g2 = Graphics.FromImage(image);
                g.FillRectangle(Brushes.Black, 0, 0, image.Width, image.Height);

                DrawLanes(g2);
#if false    // test
                Pen pen1 = new Pen(Color.Aqua, 10);

                g2.FillRectangle(Brushes.Red, 0, 0, 300, 300);
                g2.DrawLine(pen1, 50, 50, 100, 100);
                g2.Dispose();
#endif
            }

            g.DrawImage(image, 0, 0);

        }

        /**
         * レーンを描画
         */
        private void DrawLanes(Graphics g)
        {
            if (dispLanes == null || dispLanes.Count == 0)
            {
                return;
            }

            if (drawDir == DrawDir.Horizontal)
            {
                DrawLanesH(g);
            }
            else
            {
                DrawLanesV(g);
            }
        }

        private void DrawLanesH(Graphics g)
        {
            const int topX = 100;
            const int topY = 100;
            const int laneH = 60;
            int posX = topX;
            int posY = topY;
            Font font1 = new Font("Arial", 12);

            foreach (KeyValuePair<int,Lane> kvp in dispLanes)
            {
                // レーン名
                Lane lane = kvp.Value;
                g.DrawString(lane.Name, font1, Brushes.White, posX, posY);

                // レーン
                g.DrawLine(Pens.Aqua, posX, posY, posX + 300, posY);

                posY += laneH;
            }

            
        }

        private void DrawLanesV(Graphics g)
        {

        }

        public void DrawLog(Graphics g)
        {

        }


        #endregion

        #region UI
        
        /**
         * エリア表示用のTreeViewを更新する
         */
        public bool UpdateAreaTree(TreeView areaTree)
        {
            if (rootArea == null)
            {
                return false;
            }

            areaTree.Nodes.Clear();

            // 全エリアをTreeに追加
            areaTree.Nodes.Add("root");
            GetAreaNode(areaTree.Nodes[0], rootArea);

            return true;
        }

        private void GetAreaNode(TreeNode node, LogArea area)
        {
            if (area.ChildArea != null)
            {
                int i = 0;
                foreach ( LogArea childArea in area.ChildArea)
                {
                    TreeNode newNode = new TreeNode(childArea.Name);
                    newNode.Tag = childArea;
                    node.Nodes.Add(newNode);
                    GetAreaNode(node.Nodes[i], childArea);
                    i++;
                }
            }
        }

        /**
         * ログID用のCheckedListBoxを更新する
         */
        public bool UpdateLogIDListBox(CheckedListBox listBox)
        {
            listBox.Items.Clear();

            if (logIDs != null)
            {
                listBox.Items.Add("all", true);
                foreach (LogID logId in logIDs)
                {
                    listBox.Items.Add(logId, true);
                }
            }
            return true;
        }
        
        #endregion


        // 拡大率を上げる(見える範囲を狭くする)
        public void ZoomIn()
        {

        }

        // 拡大率を下げる(見える範囲を広くする)
        public void ZoomOut()
        {

        }

        /**
         * TreeViewのエリアが選択された時の処理
         * 
         * @input logArea : 選択されたエリア
         */
        public bool SelectArea(LogArea logArea)
        {
            Debug.WriteLine(logArea.Name);
            return true;
        }

        #region LogID

        #endregion

        #region Debug

        public void DebugPrint()
        {
            if (logIDs != null)
            {
                Debug.WriteLine(logIDs.ToString());
            }

            if (lanes != null)
            {
                Debug.WriteLine(lanes　);
            }

            if (iconImages != null)
            {
                Debug.WriteLine(iconImages);
            }

            if (rootArea != null)
            {
                Debug.WriteLine(rootArea);
            }
        }
        #endregion

    }
}
