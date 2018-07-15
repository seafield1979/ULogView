using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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

        //
        // Properties
        //
        private LogArea rootArea;       // ルートエリア
        private LogArea currentArea;   // 表示中のエリア(配下のエリアも表示される)
        private Image image;            // LogView描画先のImage
        private LogIDs logIDs;          // LogのID情報
        private Lanes lanes;            // レーン情報
        private IconImages iconImages;  // アイコン画像


        private double topTime;         // 表示領域の先頭の時間
        private double pixPerTime;      // 100pixelあたりの時間 (例:100pix = 1s なら 1)
        private int topPos;             // 表示先頭座標(縦表示ならx、横表示ならy)

        private DrawDir drawDir;            // 表示方向(0: 縦 / 1:横)

        // 
        // Constructor
        //
        public LogView()
        {
            image = new Bitmap(1000, 1000);
        }
      

        //
        // Methods
        //

        /**
         * LogViewファイルを読み込む
         */
        public bool ReadLogFile(string filePath)
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
            }
            return true;
        }
        #region Draw

        public void Draw(Graphics g)
        {
            DrawBG(g);
            DrawLog(g);
        }

        public void DrawBG(Graphics g)
        {
            //Graphics g = Graphics.FromImage(image);

            g.FillRectangle(Brushes.Red, 0, 0, 300, 300);

            //g.Dispose();
        }

        public void DrawLog(Graphics g)
        {

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
