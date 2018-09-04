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
     * LogViewの表示情報
     */
    struct LVDrawParam
    {
        public Size imageSize;              // LogViewの表示領域(Image)の幅、高さ
        public double topTime;              // 先頭の時間(先頭ログの時間)
        public double endTime;              // 末尾の時間(末尾ログの時間)
        public double endTime2;             // 末尾の時間(endTimeからtopTimeを引いた値)
        public double dispTopTime;          // 表示先頭の時間(先頭位置が0原点の時間)
        public double dispEndTime;          // 表示末尾の時間(先頭位置が0原点の時間)
        //public int topPos;                  // 表示先頭座標(縦表示ならx、横表示ならy)
        
        public UInt64 dispAreaLen;             // ログ表示領域のピクセル数
        
        public DrawDirection drawDir;       // 表示方向(0: 縦 / 1:横)
        public float zoomRate;              // ズーム率(1=100%, 1.5=150%)
        public LogViewPixTime pixTime;      // 1pixelあたりの時間 (例:100pix = 1s なら 0.01)

        public void Init()
        {
            zoomRate = 0;
            drawDir = DrawDirection.Horizontal;
            dispTopTime = 0;
            topTime = 0;
            endTime = 0;

            pixTime = new LogViewPixTime();
        }

        /**
         * ログの向きに合わせて、ログ進行方向の画像サイズを取得する
         */
        public int LogImageWidth()
        {
            return drawDir == DrawDirection.Horizontal ? imageSize.Width : imageSize.Height;
        }
    }



    /**
     * Logviewの表示用のクラス
     */
    public class LogView
    {
        //
        // Consts
        //
        Color colorLane = Color.White;
        Color colorLaneTitle = Color.White;
        Color colorTime = Color.White;
        Color colorDebug = Color.Yellow;
        Color colorBG = Color.Black;

        const int fontSizeLane = 10;
        const int fontSizeDebug = 10;
        
        //
        // Properties pp::
        //
        private InvalidateForm delegateInvalidate;
        private UpdateScrollV delegateScrollV;
        private UpdateScrollH delegateScrollH;

        private LogViewOption lvOption;

        private LogArea rootArea;           // ルートエリア
        private LogArea currentArea;        // 表示中のエリア(配下のエリアも表示される)
        private LogIDs logIDs;              // LogのID情報
        private Lanes lanes;                // レーン情報
        private IconImages iconImages;      // アイコン画像

        private Image image;                // LogView描画先のImage
        private bool redrawFlag;            // 再描画フラグ(true:再描画あり / false:なし)
        private LVDrawParam dparam;         // 描画用の情報
        
        private Dictionary<int, Lane> dispLanes;   // 表示中のレーン(key:LaneId)

        private int laneFontSize;

        // Pen
        private Pen penLane;
        private Pen penLaneTitle;
        private Pen penTime;
        
        // Brush
        private Brush brushBG;
        private Brush brushDebugText;

        // Font
        private Font fontLaneText;
        private Font fontDebug;

        // 
        // Constructor ct::
        //
        public LogView() : this(1000, 1000, null, null, null)
        {   
        }

        public LogView(int width, int height, InvalidateForm invalidate1, UpdateScrollV _delegateScrollV, UpdateScrollH _delegateScrollH)
        {
            lvOption = LogViewOption.GetObject();
            delegateInvalidate = invalidate1;
            delegateScrollV = _delegateScrollV;
            delegateScrollH = _delegateScrollH;
            dparam.imageSize = new Size(width, height);
            Resize(width, height);
            dispLanes = null;

            dparam.Init();
            dparam.drawDir = DrawDirection.Horizontal;
            dparam.zoomRate = 1.0f;

            // create pens & brushes
            penLane = new Pen(colorLane);
            penLaneTitle = new Pen(colorLaneTitle);
            penTime = new Pen(colorTime);
            
            brushBG = new SolidBrush(colorBG);
            brushDebugText = new SolidBrush(colorDebug);

            fontLaneText = new Font("Arial", fontSizeLane);
            fontDebug = new Font("Arial", fontSizeDebug);

            Init();
            UpdateZoom();
        }
      

        //
        // Methods mt::
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
                UpdateZoom();
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

                // 先頭のログの時間を取得
                GetTopEndLogTime(area);
                dparam.endTime2 = dparam.endTime - dparam.topTime;
                dparam.dispAreaLen = dparam.pixTime.timeToPix( dparam.endTime );

                // ログの表示状態を初期状態に戻す
                LogAreaManager.ResetLogData(area);
            }
            catch(Exception e)
            {
                Debug.WriteLine("error " + e.Message);
            }
        }

        /**
         * 指定したエリア以下のログのうち最初のログの時間を取得する
         * @input area : 先頭のエリア
         * @output 先頭のログの時間
         */
        private void GetTopEndLogTime(LogArea area)
        {
            // topTimeとendTimeを更新
            if (area.TopTime < dparam.topTime)
            {
                dparam.topTime = area.TopTime;
            }
            if (area.EndTime > dparam.endTime)
            {
                dparam.endTime = area.EndTime;
            }
            
            if (area.ChildArea != null)
            {
                foreach(LogArea cArea in area.ChildArea)
                {
                    GetTopEndLogTime(cArea);
                }
            }
        }

        #region Zoom

        
        /**
         * ズーム率が変わったときの処理
         */
        private void UpdateZoom()
        {
            // 表示領域が変わる
            laneFontSize = (int)(10 * lvOption.ZoomRate);

            UpdatePixTime();
        }

        /**
         * ログの表示範囲が変わったときの処理
         */
        private void UpdatePixTime()
        {
            // 表示される範囲が変わるので表示領域の末尾の時間が変わる
            dparam.dispEndTime = dparam.dispTopTime + dparam.pixTime.pixToTime(dparam.LogImageWidth());
        }

        /**
         * 表示領域の更新があったときの処理
         * ズーム率
         * ログ表示範囲
         * 画面スクロール等
         */
        private void UpdateDisp()
        {
            // 表示領域から末尾の時間を求める

            // 先頭から末尾までのpixelを求める
            int pixTop = (int)dparam.pixTime.timeToPix(dparam.dispTopTime);
            int allPixLen = (int)dparam.pixTime.timeToPix(dparam.endTime - dparam.topTime);
            int pagePixLen = (int)dparam.pixTime.timeToPix(dparam.dispEndTime - dparam.dispTopTime);

            // スクロールバー更新
            if (dparam.drawDir == DrawDirection.Horizontal)
            {
                delegateScrollH(pixTop, allPixLen, pagePixLen);
            }
            else
            {
                delegateScrollV(pixTop, allPixLen, pagePixLen);
            }
        }

        #endregion Zoom




        #region Draw

        public void Draw(Graphics g)
        {
            DrawBG(g);
            DrawLog(g);
        }

        public void DrawBG(Graphics g)
        {
            //if (redrawFlag)
            {
                redrawFlag = false;

                Graphics g2 = Graphics.FromImage(image);
                g2.FillRectangle(Brushes.Black, 0, 0, image.Width, image.Height);

                DrawParams(g2, 10, 10);
                DrawLanes(g2);

                g2.Dispose();
            }

            g.DrawImage(image, 0, 0);

        }

        /**
         * 表示用の情報を描画 for Debug
         */
        private void DrawParams(Graphics g, int _x, int _y)
        {
            int x = _x;
            int y = _y;
            const int fontSize = 10;

            g.DrawString(String.Format("TopTime:{0}", dparam.topTime), fontDebug, brushDebugText, x, y);
            y += fontSize + 4;
            g.DrawString(String.Format("EndTime:{0}", dparam.endTime), fontDebug, brushDebugText, x, y);
            y += fontSize + 4;
            g.DrawString(String.Format("DispTopTime:{0}", dparam.dispTopTime), fontDebug, brushDebugText, x, y);
            y += fontSize + 4;
            g.DrawString(String.Format("DispEndTime:{0}", dparam.dispEndTime), fontDebug, brushDebugText, x, y);
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

            if (dparam.drawDir == DrawDirection.Horizontal)
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
            int laneW = 300;
            int laneH = 60;
            int posX = topX;
            int posY = topY;
            
            // 領域のサイズ
            laneW = (int)dparam.pixTime.timeToPix(dparam.endTime);
            Debug.WriteLine("laneW2:{0}", laneW);

            foreach (KeyValuePair<int,Lane> kvp in dispLanes)
            {
                // レーン名(右寄せ)
                Lane lane = kvp.Value;
                StringFormat sf1 = new StringFormat();
                sf1.Alignment = StringAlignment.Far;
                sf1.LineAlignment = StringAlignment.Center;

                g.DrawString(lane.Name, fontLaneText, Brushes.White, posX, posY + laneH / 2, sf1);
                
                // レーン
                g.DrawLine(Pens.Aqua, posX, posY, posX + laneW, posY);
                g.DrawLine(Pens.Aqua, posX, posY + laneH, posX, posY);

                posY += laneH;
            }
            // 閉じレーン
            g.DrawLine(Pens.Aqua, posX, posY, posX + laneW, posY);

        }

        private void DrawLanesV(Graphics g)
        {

        }

        public void DrawLog(Graphics g)
        {

        }


        #endregion

        #region UI
        // UI::
        
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

        public void ScrollV(int value)
        {

        }

        public void ScrollH(int value)
        {

        }

        public void PageDown()
        {

        }

        public void PageUp()
        {

        }
        
        #endregion


        // 拡大率を上げる(見える範囲を狭くする)
        public void LVZoomIn()
        {

        }

        // 拡大率を下げる(見える範囲を広くする)
        public void LVZoomOut()
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
