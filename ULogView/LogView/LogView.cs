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
    /// <summary>
    /// 
    /// </summary>
    public enum ELogViewDir : int
    {
        Vertical = 0,
        Horizontal
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

        // new
        private int topMarginX = 140;
        private int topMarginY = 120;
        private int bottomMarginX = 100;
        private int bottomMarginY = 100;

        private int intervalX = 100;
        private int intervalY = 100;

        //
        // Properties pp::
        //
        private InvalidateForm delegateInvalidate;
        
        private LogViewOption lvOption;

        private LogArea rootArea;           // ルートエリア
        private LogArea currentArea;        // 表示中のエリア(配下のエリアも表示される)
        private LogIDs logIDs;              // LogのID情報
        private Lanes lanes;                // レーン情報
        private IconImages iconImages;      // アイコン画像

        private Image image;                // LogView描画先のImage
        private bool redrawFlag;            // 再描画フラグ(true:再描画あり / false:なし)
        
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
        // Properties
        //
        #region Properties

        // 表示の方向
        private ELogViewDir direction;

        public ELogViewDir Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        // 全体の先頭の時間
        private double topTime;
        
        // 全体の最後の時間
        private double endTime;

        // 表示範囲の先頭の時間
        private double dispTopTime;

        // 表示範囲の末尾の時間
        private double dispEndTime;

        // 全体のズーム率
        private ZoomRate zoomRate;

        // 1ピクセル当たりの時間
        private LogViewPixTime pixTime;

        // 全体のログ進行方向の領域の長さ
        private UInt64 dispAreaLen;

        private HScrollBar scrollBarH;
        private VScrollBar scrollBarV;

        private ScrollBar scrollBar1;       // 縦モードのときは水平スクロールバー。横モードのときは垂直スクロールバー
        private ScrollBar scrollBar2;       // 縦モードのときは垂直スクロールバー。横モードのときは水平スクロールバー

        #endregion

        // 
        // Constructor ct::
        //
        public LogView() : this(1000, 1000, 0, null, null, null)
        {   
        }

        public LogView(int width, int height, ELogViewDir direction, InvalidateForm invalidateForm, HScrollBar scrollBarH, VScrollBar scrollBarV)
        {
            lvOption = LogViewOption.GetObject();

            delegateInvalidate = invalidateForm;
            pixTime = new LogViewPixTime();
            zoomRate = new ZoomRate();
            
            this.scrollBarH = scrollBarH;
            this.scrollBarV = scrollBarV;

            this.direction = direction;
            if (direction == ELogViewDir.Vertical)
            {
                scrollBar1 = scrollBarH;
                scrollBar2 = scrollBarV;
            }
            else
            {
                scrollBar1 = scrollBarV;
                scrollBar2 = scrollBarH;
            }

            topTime = 0.0;
            endTime = 10.0;
            dispTopTime = 0.0;
            dispEndTime = GetDispEndTime();

            scrollBar1.LargeChange = width - (topMarginX + bottomMarginX);
            scrollBar2.LargeChange = height - (topMarginY + bottomMarginY);

            scrollBar1.Maximum = 1000;
            scrollBar2.Maximum = (int)pixTime.timeToPix(dispEndTime);

            Resize(width, height);

            // create pens & brushes
            penLane = new Pen(colorLane);
            penLaneTitle = new Pen(colorLaneTitle);
            penTime = new Pen(colorTime);
            
            brushBG = new SolidBrush(colorBG);
            brushDebugText = new SolidBrush(colorDebug);

            fontLaneText = new Font("Arial", fontSizeLane);
            fontDebug = new Font("Arial", fontSizeDebug);
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
                if (image != null)
                {
                    if (image.Width == width && image.Height == height)
                    {
                        return;
                    }
                    image.Dispose();
                }

                image = new Bitmap(width, height);
                redrawFlag = true;
                SetDirection(direction);
            }

            delegateInvalidate();
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
        public void SetLogArea(LogArea area)
        {
            try
            {
                currentArea = area;

                // 表示するレーンを判定
                dispLanes = LogAreaManager.GetDispLaneList(area, lanes);

                // 先頭のログの時間を取得
                GetTopEndLogTime(area);

                dispTopTime = topTime;
                dispEndTime = GetDispEndTime();

                dispAreaLen = pixTime.timeToPix(endTime);

                ChangeZoomRate();

                // ログの表示状態を初期状態に戻す
                LogAreaManager.ResetLogData(area);
            }
            catch(Exception e)
            {
                Debug.WriteLine("error " + e.Message);
            }
        }

        /// <summary>
        /// 指定したエリアを表示する際の先頭と末尾のログの時間を更新する
        /// </summary>
        /// <param name="area"></param>
        private void GetTopEndLogTime(LogArea area)
        {
            topTime = 100000000;
            endTime = 0;
            GetTopEndLogTime2(area);
        }
        private void GetTopEndLogTime2(LogArea area)
        {
            // topTimeとendTimeを更新
            if (area.TopTime < topTime)
            {
                topTime = area.TopTime;
            }
            if (area.EndTime > endTime)
            {
                endTime = area.EndTime;
            }
            
            if (area.ChildArea != null)
            {
                foreach(LogArea cArea in area.ChildArea)
                {
                    GetTopEndLogTime2(cArea);
                }
            }
        }

        /// <summary>
        /// ログのタイムライン進行方向を設定する。
        /// </summary>
        /// <param name="direction"></param>
        public void SetDirection(ELogViewDir direction)
        {
            this.direction = direction;

            if (direction == ELogViewDir.Vertical)
            {
                scrollBar1 = scrollBarH;
                scrollBar2 = scrollBarV;
            }
            else
            {
                scrollBar1 = scrollBarV;
                scrollBar2 = scrollBarH;
            }

            ChangeZoomRate();
        }

        /**
         * 表示領域の端の時間
         * 画面の一番端のピクセルの時間
         */
        private double GetDispEndTime()
        {
            double time = dispTopTime + (pixTime.pixToTime(scrollBar2.LargeChange + 200)) / zoomRate.Value;

            return time < endTime ? time : endTime;
        }

        // 画面全体のズーム率を設定する
        public void setZoomRate(ZoomRate zoomRate)
        {
            this.zoomRate = zoomRate;
        }

        // 指定の整数値にズーム率をかけた結果を取得
        private int getZoomValue(int value)
        {
            return (int)(value * zoomRate.Value);
        }

        // 指定の浮動小数値にズーム率をかけた結果を取得
        public float getZoomValue(float value)
        {
            return (float)(value * zoomRate.Value);
        }

        // scroll::
        #region Scroll
        private void UpdateScrollY()
        {
            //dispTopTime = topTime + pixTime.pixToTime(scrollBar2.Value) / zoomRate.Value;

            dispEndTime = GetDispEndTime();
        }

        // X方向にスクロールする
        public bool ScrollX(int delta)
        {
            return ScrollXY(scrollBarH, delta);
        }

        // Y方向にスクロールする
        public bool ScrollY(int delta)
        {
            return ScrollXY(scrollBarV, delta);
        }

        // X/Yのスクロール処理(両対応)
        public bool ScrollXY(ScrollBar sb, int delta)
        {
            int oldValue = sb.Value;

            if (sb.Value + delta > sb.Maximum - sb.LargeChange)
            {
                sb.Value = sb.Maximum - sb.LargeChange;
            }
            else if (sb.Value + delta < 0)
            {
                sb.Value = 0;
            }
            else if (sb.Value + delta < sb.Maximum)
            {
                sb.Value += delta;
            }

            if (oldValue != sb.Value)
            {
                return true;
            }
            return false;
        }

        // 下に１ページ分スクロール
        public bool ScrollDown()
        {
            return ScrollY(scrollBar2.LargeChange);
        }

        // 上に１ページ分スクロール
        public bool ScrollUp()
        {
            return ScrollY(-scrollBar2.LargeChange);
        }

        #endregion Scroll


        // draw::
        #region Draw

        /// <summary>
        /// 描画処理
        /// </summary>
        /// <param name="g"></param>
        public void Draw(Graphics g)
        {
            DrawBG(g);
            DrawLog(g);
        }

        /// <summary>
        /// 背景の描画処理
        /// </summary>
        /// <param name="g"></param>
        private void DrawBG(Graphics g)
        {
            using (Graphics g2 = Graphics.FromImage(image))
            {
                // clear background
                g2.FillRectangle(Brushes.Black, 0, 0, image.Width, image.Height);

                var font1 = new Font("Arial", 10);

                // テキスト表示
                int x0 = 10;
                int y0 = 10;

                g2.DrawString(String.Format("dispTopTime:{0} dispEndTime:{1}", dispTopTime, dispEndTime), font1, Brushes.White, x0, y0);
                y0 += 20;
                g2.DrawString(String.Format("[sbH] value:{0},large:{1} max:{2}", scrollBarH.Value, scrollBarH.LargeChange, scrollBarH.Maximum),
                    font1, Brushes.White, x0, y0);
                y0 += 20;
                g2.DrawString(String.Format("[sbV] value:{0},large:{1} max:{2}", scrollBarV.Value, scrollBarV.LargeChange, scrollBarV.Maximum),
                    font1, Brushes.White, x0, y0);
                y0 += 20;
                g2.DrawString(String.Format("zoomRate:{0} pixTime.zoom:{1}", zoomRate, pixTime.Val), font1, Brushes.White, x0, y0);
                y0 += 20;

                if (direction == ELogViewDir.Vertical)
                {
                    DrawV(g2);
                }
                else
                {
                    DrawH(g2);
                }
            }
            g.DrawImage(image, 0, 0);
        }

        // 横にログが進んでいく描画モード
        private void DrawH(Graphics g)
        {
            UpdateScrollY();

            // クリッピング設定
            Rectangle rect1 = new Rectangle(topMarginX, topMarginY, scrollBarH.LargeChange, scrollBarV.LargeChange);
            g.SetClip(rect1);

            g.FillRectangle(Brushes.DarkRed, rect1);

            int x = -(scrollBarH.Value % intervalX);
            int y = -(scrollBarV.Value % intervalY);
            int intervalX2 = (int)(intervalX * zoomRate.Value);
            int intervalY2 = (int)(intervalY * zoomRate.Value);

            //--------------------------------
            // ライン
            //--------------------------------
            var font2 = new Font("Arial", getZoomValue(10));
            // 横のライン
            while (y < scrollBarV.LargeChange)
            {
                g.DrawLine(Pens.White, topMarginX, topMarginY + y,
                    image.Width - bottomMarginX, topMarginY + y);
                y += intervalY2;
            }
            // 縦のライン
            int offsetX = scrollBarV.Value;
            while (x < scrollBarH.LargeChange)
            {
                g.DrawLine(Pens.White, topMarginX + x, topMarginY,
                    topMarginX + x, image.Height - bottomMarginY);
                x += intervalX2;
            }

            //--------------------------------
            // 文字列
            //--------------------------------
            x = -(scrollBarH.Value % intervalX2);
            y = -(scrollBarV.Value % intervalY2);

            StringFormat sf1 = new StringFormat();
            sf1.Alignment = StringAlignment.Far;
            sf1.LineAlignment = StringAlignment.Center;

            // set cliping
            Rectangle rect2 = new Rectangle(0, topMarginY - 10, image.Width, image.Height - topMarginY + 10);
            g.SetClip(rect2);

            // 横のテキスト
            while (y < scrollBarV.LargeChange)
            {
                g.DrawString(String.Format("{0}", (y + offsetX) / zoomRate.Value),
                    font2, Brushes.Yellow, topMarginX - 5, topMarginY + y, sf1);
                y += intervalY2;
            }

            // 縦のテキスト
            Rectangle rect3 = new Rectangle(topMarginX - 10, 0, scrollBarH.LargeChange + 10, image.Height - topMarginY);
            g.SetClip(rect3);
            sf1.Alignment = StringAlignment.Center;
            sf1.LineAlignment = StringAlignment.Far;

            while (x < scrollBarH.LargeChange + 20)
            {
                double time = dispTopTime + pixTime.pixToTime(x) / zoomRate.Value;
                g.DrawString(String.Format("{0:0.######}s", time),
                    font2, Brushes.Yellow, topMarginX + x, topMarginY - 5, sf1);
                x += intervalX2;
            }
        }

        // 縦(下)にログが進んでいくモード
        private void DrawV(Graphics g)
        {
            UpdateScrollY();

            // クリッピング設定
            Rectangle rect1 = new Rectangle(topMarginX, topMarginY, image.Width - (topMarginX + bottomMarginX), image.Height - (topMarginY + bottomMarginY));
            g.SetClip(rect1);

            g.FillRectangle(Brushes.DarkRed, rect1);

            int x = -(scrollBarH.Value % intervalX);
            int y = -(scrollBarV.Value % intervalY);
            int intervalX2 = (int)(intervalX * zoomRate.Value);
            int intervalY2 = (int)(intervalY * zoomRate.Value);

            //--------------------------------
            // ライン
            //--------------------------------
            var font2 = new Font("Arial", getZoomValue(10));
            // 横のライン
            while (y < scrollBarV.LargeChange)
            {
                g.DrawLine(Pens.White, topMarginX, topMarginY + y,
                    image.Width - bottomMarginX, topMarginY + y);
                y += intervalY2;
            }
            // 縦のライン
            int offsetX = scrollBarH.Value;
            while (x < scrollBarH.LargeChange)
            {
                g.DrawLine(Pens.White, topMarginX + x, topMarginY,
                    topMarginX + x, image.Height - bottomMarginY);
                x += intervalX2;
            }

            //--------------------------------
            // 文字列
            //--------------------------------
            x = -(scrollBarH.Value % intervalX2);
            y = -(scrollBarV.Value % intervalY2);

            StringFormat sf1 = new StringFormat();
            sf1.Alignment = StringAlignment.Far;
            sf1.LineAlignment = StringAlignment.Center;

            // set cliping
            Rectangle rect2 = new Rectangle(0, topMarginY - 10, image.Width, image.Height - topMarginY + 10);
            g.SetClip(rect2);

            // 横のテキスト
            while (y < scrollBarV.LargeChange)
            {
                double time = dispTopTime + pixTime.pixToTime(y) / zoomRate.Value;
                g.DrawString(String.Format("{0:0.######}s", time),
                    font2, Brushes.Yellow, topMarginX - 5, topMarginY + y, sf1);
                y += intervalY2;
            }

            // 縦のテキスト
            Rectangle rect3 = new Rectangle(topMarginX - 10, 0, scrollBarH.LargeChange + 10, image.Height - topMarginY);
            g.SetClip(rect3);
            sf1.Alignment = StringAlignment.Center;
            sf1.LineAlignment = StringAlignment.Far;
            while (x < scrollBarH.LargeChange + 20)
            {
                g.DrawString(String.Format("{0}", (x + offsetX) / zoomRate.Value),
                    font2, Brushes.Yellow, topMarginX + x, topMarginY - 5, sf1);
                x += intervalX2;
            }

        }

        /**
         * 表示用の情報を描画 for Debug
         */
        private void DrawParams(Graphics g, int _x, int _y)
        {
            int x = _x;
            int y = _y;
            const int fontSize = 10;

            g.DrawString(String.Format("TopTime:{0}", topTime), fontDebug, brushDebugText, x, y);
            y += fontSize + 4;
            g.DrawString(String.Format("EndTime:{0}", endTime), fontDebug, brushDebugText, x, y);
            y += fontSize + 4;
            g.DrawString(String.Format("DispTopTime:{0}", dispTopTime), fontDebug, brushDebugText, x, y);
            y += fontSize + 4;
            g.DrawString(String.Format("DispEndTime:{0}", dispEndTime), fontDebug, brushDebugText, x, y);
        }

        public void DrawLog(Graphics g)
        {

        }


        // zoom::
        #region Zoom
        /**
         * ズームバーを描画
         */
        public void DrawZoomBar(Graphics g)
        {

        }
        #endregion Draw


        // zoom::

        // １秒当たりのピクセル数のズーム
        // 拡大
        public bool PixTimeZoomUp()
        {
            pixTime.ZoomIn();
            ChangeZoomRate();
            return true;
        }

        // 縮小
        public bool PixTimeZoomDown()
        {
            pixTime.ZoomOut();
            ChangeZoomRate();
            return true;
        }

        // 全体のズーム率
        // 拡大
        public bool ZoomUp()
        {
            zoomRate.ZoomIn();
            ChangeZoomRate();
            return true;
        }

        // 縮小
        public bool ZoomDown()
        {
            zoomRate.ZoomOut();
            ChangeZoomRate();
            return true;
        }

        // 拡大率が変化したときの処理
        private void ChangeZoomRate()
        {
            // 拡大したときの動作としてスクロールバーのmaxが変化するパターンと
            // LargeChangeが変化するパターンがあるが、ここではmaxが変換するパターンを採用
            scrollBar1.Maximum = (int)(1000 * zoomRate.Value);
            scrollBar2.Maximum = (int)pixTime.timeToPix(dispEndTime - dispTopTime);

            // 0になると完全に表示されなくなるため下限は1
            if (scrollBar1.Maximum < 1)
            {
                scrollBar1.Maximum = 1;
            }
            if (scrollBar2.Maximum < 1)
            {
                scrollBar2.Maximum = 1;
            }

            scrollBarH.LargeChange = image.Width;
            scrollBarV.LargeChange = image.Height;
            

            if (scrollBar1.LargeChange > scrollBar1.Maximum)
            {
                scrollBar1.Enabled = false;
                scrollBar1.LargeChange = scrollBar1.Maximum;
            }
            else
            {
                scrollBar1.Enabled = true;
            }

            if (scrollBar2.LargeChange > scrollBar2.Maximum)
            {
                scrollBar2.Enabled = false;
                scrollBar2.LargeChange = scrollBar2.Maximum;
            }
            else
            {
                scrollBar2.Enabled = true;
            }

            delegateInvalidate();
        }


        #endregion Zoom

        #region UI
        // UI::

        /// <summary>
        /// エリア表示用のTreeViewを更新する
        /// </summary>
        /// <param name="areaTree">更新対象のTreeView</param>
        /// <returns></returns>
        public bool UpdateAreaTree(TreeView areaTree)
        {
            if (rootArea == null)
            {
                return false;
            }

            // 全エリアをTreeに追加
            areaTree.Nodes.Clear();
            areaTree.Nodes.Add("root");
            GetAreaNode(areaTree.Nodes[0], rootArea);

            return true;
        }

        /// <summary>
        /// 指定したTreeViewに指定のLogArea以下のノードをすべて追加する
        /// </summary>
        /// <param name="node">追加先のTreeViewのノード(ルート)</param>
        /// <param name="area">追加元のエリア</param>
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

        /// <summary>
        /// ログID用のCheckedListBoxを更新する
        /// </summary>
        /// <param name="listBox">更新対象のListBox</param>
        /// <returns></returns>
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

    public class DoubleBufferingPanel : System.Windows.Forms.Panel
    {
        public DoubleBufferingPanel()
        {
            this.DoubleBuffered = true;
        }
    }
}
