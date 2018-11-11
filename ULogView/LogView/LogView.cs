﻿using System;   
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using ULogView.Utility;
using System.Drawing.Drawing2D;

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
        
        private int topMarginX = 140;
        private int topMarginY = 200;
        private int bottomMarginX = 50;
        private int bottomMarginY = 50;

        private int intervalX = 100;
        private int intervalY = 100;

        private int laneLen = 100;

        private int logAreaW, logAreaH;

        //
        // Properties pp::
        //
        private InvalidateForm delegateInvalidate;
        
        private LogViewOption lvOption;

        private LogArea rootArea;           // ルートエリア
        private LogArea currentArea;        // 表示中のエリア(配下のエリアも表示される)
        private List<LogData> currentLogs;  // カレントエリア配下のログリスト
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

        private ScrollBar scrollBarLane;       // 縦モードのときは水平スクロールバー。横モードのときは垂直スクロールバー
        private ScrollBar scrollBarTime;       // 縦モードのときは垂直スクロールバー。横モードのときは水平スクロールバー

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
                scrollBarLane = scrollBarH;
                scrollBarTime = scrollBarV;
            }
            else
            {
                scrollBarLane = scrollBarV;
                scrollBarTime = scrollBarH;
            }

            topTime = 0.0;
            endTime = 10.0;
            dispTopTime = 0.0;
            dispEndTime = GetDispEndTime();
                        
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

                // LogViewの描画エリアのサイズを変更
                logAreaW = width - (topMarginX + bottomMarginX);
                logAreaH = height - (topMarginY + bottomMarginY);

                scrollBarLane.Maximum = GetLaneLength();
                scrollBarTime.Maximum = (int)pixTime.timeToPix(endTime - topTime, zoomRate.Value);

                if (logAreaW > scrollBarH.Maximum)
                {
                    scrollBarH.LargeChange = scrollBarH.Maximum;
                }
                else
                {
                    scrollBarH.LargeChange = logAreaW;
                }
                
                if (logAreaH > scrollBarV.Maximum)
                {
                    scrollBarV.LargeChange = scrollBarV.Maximum;
                }
                else
                {
                    scrollBarV.LargeChange = logAreaH;
                }

                dispEndTime = GetDispEndTime();
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

                dispAreaLen = pixTime.timeToPix(endTime, zoomRate.Value);

                ChangeZoomRate();

                // 配下のログを currentLogs にまとめる
                currentLogs = new List<LogData>();
                AddCurrentLogs(currentArea, currentLogs);

                // ログの表示状態を初期状態に戻す
                LogAreaManager.ResetLogData(area);
            }
            catch(Exception e)
            {
                Debug.WriteLine("error " + e.Message);
            }
        }

        /// <summary>
        /// カレントエリア配下のログリストを作成する
        /// </summary>
        /// <param name="logs"></param>
        private void AddCurrentLogs(LogArea logArea, List<LogData> logs)
        {
            // ログを追加
            if (logArea.Logs != null)
            {
                foreach(LogData log in logArea.Logs)
                {
                    logs.Add(log);
                }
            }

            if (logArea.ChildArea != null)
            {
                foreach (LogArea child in logArea.ChildArea)
                {
                    AddCurrentLogs(child, logs);
                }
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
                scrollBarLane = scrollBarH;
                scrollBarTime = scrollBarV;
            }
            else
            {
                scrollBarLane = scrollBarV;
                scrollBarTime = scrollBarH;
            }

            // dispEndTimeは画面のサイズに依存するため更新する
            dispEndTime = GetDispEndTime();

            // ほとんど同じ処理なのでズーム時の更新処理を代用
            ChangeZoomRate();
        }

        /// <summary>
        /// ログのタイムライン進行方向を縦横切り替える
        /// </summary>
        public void ToggleDirection()
        {
            this.direction = (direction == ELogViewDir.Horizontal) ? ELogViewDir.Vertical : ELogViewDir.Horizontal;

            SetDirection(this.direction);
        }

        /**
         * 表示領域の端の時間
         * 画面の一番端のピクセルの時間
         */
        private double GetDispEndTime()
        {
            int logAreaLen = (direction == ELogViewDir.Vertical) ? logAreaH : logAreaW;
            
            double time = dispTopTime + (pixTime.pixToTime(logAreaLen)) / zoomRate.Value;

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
        public void UpdateTimeScroll()
        {
            if (direction == ELogViewDir.Horizontal)
            {
                dispTopTime = topTime + pixTime.pixToTime(scrollBarTime.Value) / zoomRate.Value;
            }
            else
            {
                dispTopTime = topTime + pixTime.pixToTime(scrollBarTime.Value) / zoomRate.Value;
                dispEndTime = GetDispEndTime();
            }
        }

        // X方向にスクロールする
        public bool ScrollX(int delta)
        {
            bool ret = ScrollXY(scrollBarH, delta);
            if (direction == ELogViewDir.Horizontal)
            {
                UpdateTimeScroll();
            }
            return ret;
        }

        // Y方向にスクロールする
        public bool ScrollY(int delta)
        {
            bool ret = ScrollXY(scrollBarV, delta);
            if (direction == ELogViewDir.Vertical)
            {
                UpdateTimeScroll();
                ChangeZoomRate();
            }
            return ret;
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
            return ScrollY(scrollBarTime.LargeChange);
        }

        // 上に１ページ分スクロール
        public bool ScrollUp()
        {
            return ScrollY(-scrollBarTime.LargeChange);
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
            // アンチエリアシング
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawBG(g);
            DrawLog(g);

            g.SmoothingMode = SmoothingMode.Default;
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

                // debug log::
                // テキスト表示
                int x0 = 50;
                int y0 = 60;

                g2.DrawString(String.Format("dispTopTime:{0} dispEndTime:{1}", dispTopTime, dispEndTime), font1, Brushes.White, x0, y0);
                y0 += 20;
                g2.DrawString(String.Format("TopTime:{0} EndTime:{1}", topTime, endTime), font1, Brushes.White, x0, y0);
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
                    DrawBG_V(g2);
                }
                else
                {
                    DrawBG_H(g2);
                }
            }
            g.DrawImage(image, 0, 0);
        }

        // 横にログが進んでいく描画モード
        private void DrawBG_H(Graphics g)
        {
            // クリッピング設定
            Rectangle rect1 = new Rectangle(topMarginX, topMarginY,
                image.Width - (topMarginX + bottomMarginX),
                image.Height - (topMarginY + bottomMarginY));
            g.SetClip(rect1);

            g.FillRectangle(Brushes.DarkSlateBlue, rect1);

            int x = -(scrollBarH.Value % intervalX);
            int y = -(scrollBarV.Value % intervalY);
            int intervalX2 = (int)(intervalX * zoomRate.Value);
            int intervalY2 = (int)(intervalY * zoomRate.Value);
            int height = (int)GetLaneLength();
            int width = image.Width - bottomMarginX;

            if (height > image.Height - (bottomMarginY + topMarginY))
            {
                height = image.Height - (bottomMarginY + bottomMarginY);
            }

            if (pixTime.timeToPix(endTime - topTime, zoomRate.Value) < (ulong)width)
            {
                width = (int)pixTime.timeToPix(endTime - topTime, zoomRate.Value);
            }

            //--------------------------------
            // ライン
            //--------------------------------
            var font2 = new Font("Arial", getZoomValue(10));
            // 横のライン
            while (y < scrollBarLane.LargeChange)
            {
                g.DrawLine(Pens.White, topMarginX, topMarginY + y,
                    topMarginX + width, topMarginY + y);
                y += intervalY2;
            }
            
            // 縦のライン
            int offsetY = scrollBarLane.Value;
            while (x < scrollBarTime.LargeChange)
            {
                g.DrawLine(Pens.White, topMarginX + x, topMarginY,
                    topMarginX + x, topMarginY + height);
                x += intervalX2;
                if (pixTime.pixToTime(x) >= endTime)
                {
                    // 表示の終端を描画する
                    g.DrawLine(Pens.White, topMarginX + x, topMarginY,
                    topMarginX + x, topMarginY + height);
                    break;
                }
            }
            g.DrawLine(Pens.White, topMarginX + x, topMarginY,
                topMarginX + x, topMarginY + height);

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

            // 横のテキスト(lane)
            while (y < scrollBarV.LargeChange)
            {
                g.DrawString(String.Format("{0}", (y + offsetY) / zoomRate.Value),
                    font2, Brushes.Yellow, topMarginX - 5, topMarginY + y, sf1);
                y += intervalY2;
            }

            // 縦のテキスト(time)
            Rectangle rect3 = new Rectangle(topMarginX - 10, 0, scrollBarH.LargeChange + 10, image.Height - topMarginY);
            g.SetClip(rect3);
            sf1.Alignment = StringAlignment.Center;
            sf1.LineAlignment = StringAlignment.Far;
            while (x < scrollBarH.LargeChange + 20)
            {
                double time = dispTopTime + pixTime.pixToTime(x) / zoomRate.Value;
                g.DrawString(String.Format("{0:0.######}s", time),
                    font2, Brushes.Yellow, topMarginX + x, topMarginY - 5, sf1);
                x += intervalY2;
            }
        }

        // 縦(下)にログが進んでいくモードの背景を描画
        private void DrawBG_V(Graphics g)
        {
            // クリッピング設定
            Rectangle rect1 = new Rectangle(topMarginX, topMarginY, image.Width - (topMarginX + bottomMarginX), image.Height - (topMarginY + bottomMarginY));
            g.SetClip(rect1);

            g.FillRectangle(Brushes.DarkSlateBlue, rect1);

            int intervalX2 = (int)(intervalX * zoomRate.Value);
            int intervalY2 = (int)(intervalY * zoomRate.Value);
            int x = -(scrollBarH.Value % intervalX2);
            int y = -(scrollBarV.Value % intervalY2);
            int width = (int)GetLaneLength();
            int height = image.Height - bottomMarginY;

            if (width > image.Width - (bottomMarginX + topMarginX))
            {
                width = image.Width - (bottomMarginX + bottomMarginX);
            }

            if (pixTime.timeToPix(endTime - topTime, zoomRate.Value) < (ulong)height)
            {
                height = (int)pixTime.timeToPix(endTime - topTime, zoomRate.Value);
            }

            //--------------------------------
            // ライン
            //--------------------------------
            var font2 = new Font("Arial", getZoomValue(10));
            // 横のライン
            while (y < scrollBarTime.LargeChange)
            {
                g.DrawLine(Pens.White, topMarginX, topMarginY + y,
                    topMarginX + width, topMarginY + y);
                y += intervalY2;
                if (pixTime.pixToTime(y) >= endTime)
                {
                    // 表示の終端を描画する
                    g.DrawLine(Pens.White, topMarginX, topMarginY + y,
                    topMarginX + width, topMarginY + y);
                    break;
                }
            }
            
            // 縦のライン
            int offsetX = scrollBarLane.Value;
            while (x < scrollBarLane.LargeChange)
            {
                g.DrawLine(Pens.White, topMarginX + x, topMarginY,
                    topMarginX + x, topMarginY + height);
                x += intervalX2;
            }
            g.DrawLine(Pens.White, topMarginX + x, topMarginY,
                topMarginX + x, topMarginY + height);

            //--------------------------------
            // 文字列
            //--------------------------------
            x = -(scrollBarH.Value % intervalX2);
            y = -(scrollBarV.Value % intervalY2);

            StringFormat sf1 = new StringFormat();
            sf1.Alignment = StringAlignment.Far;
            sf1.LineAlignment = StringAlignment.Center;

            // 横のテキスト用のクリッピングエリア設定
            Rectangle rect2 = new Rectangle(0, topMarginY - 10, topMarginX + image.Width + 100, image.Height - topMarginY + 10);
            g.SetClip(rect2);

            // 横のテキスト
            sf1.Alignment = StringAlignment.Far;
            sf1.LineAlignment = StringAlignment.Center;
            while (y < scrollBarTime.LargeChange)
            {
                double time = dispTopTime + pixTime.pixToTime(y) / zoomRate.Value;
                g.DrawString(String.Format("{0:0.######}s", time),
                    font2, Brushes.Yellow, topMarginX - 5, topMarginY + y, sf1);
                y += intervalY2;
            }

            // 縦のテキスト
            Rectangle rect3 = new Rectangle(topMarginX - 10, 0, scrollBarH.LargeChange + 100, image.Height - topMarginY);
            g.SetClip(rect3);
            sf1.Alignment = StringAlignment.Center;
            sf1.LineAlignment = StringAlignment.Far;
            while (x < scrollBarLane.LargeChange + 20)
            {
                g.DrawString(String.Format("{0}", (x + offsetX) / zoomRate.Value),
                    font2, Brushes.Yellow, topMarginX + x, topMarginY - 5, sf1);
                x += intervalX2;
            }
        }

        #region DrawLogs
        
        /// <summary>
        /// 水平にタイムが進行するモードでのログ描画
        /// </summary>
        /// <param name="g"></param>
        private void DrawLog_H(Graphics g)
        {

        }

        /// <summary>
        /// 垂直にタイムが進行するモードでのログ描画
        /// </summary>
        /// <param name="g"></param>
        private void DrawLog_V(Graphics g)
        {
            // 先頭のログを取得
            int topIndex = LogArea.GetTopLogIndex(dispTopTime, currentLogs);
            if (topIndex == -1)
            {
                // 表示するログが見つからない
                return;
            }

            // クリッピング設定
            Rectangle rect1 = new Rectangle(topMarginX, topMarginY,
                        image.Width - (topMarginX + bottomMarginX),
                        image.Height - (topMarginY + bottomMarginY));
            g.SetClip(rect1);

            int index = topIndex;
            
            for (int i = topIndex; i < currentLogs.Count; i++)
            {
                LogData log = currentLogs[i];
                Brush brush1 = new SolidBrush(Color.FromArgb((int)log.Color));

                // 描画座標を計算する
                double time = log.Time1 - dispTopTime;
                int posY = topMarginY + (int)pixTime.timeToPix(time, zoomRate.Value);
                int posX = topMarginX + (int)(((float)log.LaneId - 1.0 + 0.5) * (laneLen) * zoomRate.Value);

                UDrawUtility.FillCircle(g, brush1, posX, posY, 10.0f);

                if (log.Time1 > dispEndTime)
                {
                    break;
                }
            }
        }

        #endregion

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
            if (direction == ELogViewDir.Horizontal)
            {
                DrawLog_H(g);
            }
            else
            {
                DrawLog_V(g);
            }
        }

        public int GetLaneLength()
        {
            if (lanes == null)
            {
                return 0;
            }
            return (int)(lanes.Count * laneLen * zoomRate.Value);
        }

        /// <summary>
        /// 描画領域のレーン方向の長さを取得する
        /// </summary>
        /// <returns></returns>
        public int GetLaneAreaLength()
        {
            if (direction == ELogViewDir.Horizontal)
            {
                return GetLaneLength() - (bottomMarginY - topMarginY);
            }
            else
            {
                return GetLaneLength() - (bottomMarginX - topMarginX);
            }
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
            dispTopTime = topTime + pixTime.pixToTime(scrollBarTime.Value) / zoomRate.Value;

            // 拡大したときの動作としてスクロールバーのmaxが変化するパターンと
            // LargeChangeが変化するパターンがあるが、ここではmaxが変換するパターンを採用
            scrollBarLane.Maximum = (int)GetLaneLength();
            scrollBarTime.Maximum = (int)(pixTime.timeToPix(endTime - topTime, zoomRate.Value) * zoomRate.Value);

            // 0になると完全に表示されなくなるため下限は1
            if (scrollBarLane.Maximum < 1)
            {
                scrollBarLane.Maximum = 1;
            }
            if (scrollBarTime.Maximum < 1)
            {
                scrollBarTime.Maximum = 1;
            }

            dispEndTime = GetDispEndTime();
            
            scrollBarTime.LargeChange = (int)pixTime.timeToPix(dispEndTime - dispTopTime, zoomRate.Value);
            scrollBarLane.LargeChange = (int)GetLaneAreaLength();
            
            // スクロールバーのLargeChangeよりもMaximum が小さくなったらバーを非表示にする
            if (scrollBarLane.LargeChange > scrollBarLane.Maximum)
            {
                scrollBarLane.Enabled = false;
                scrollBarLane.LargeChange = scrollBarLane.Maximum;
            }
            else
            {
                scrollBarLane.Enabled = true;
            }

            if (scrollBarTime.LargeChange > scrollBarTime.Maximum)
            {
                scrollBarTime.Enabled = false;
                scrollBarTime.LargeChange = scrollBarTime.Maximum;
            }
            else
            {
                scrollBarTime.Enabled = true;
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
