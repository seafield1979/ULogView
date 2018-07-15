using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULogView
{
    /**
     * LogViewオブジェクトをフォームに表示するためのクラス
     * 
     * Formのイベントを受け取りLogViewに渡す。
     * LogViewの情報を参照して画面に描画を行う。
     */
    public class DocumentLV
    {
        //
        // Consts
        // 

        // 
        // Properties
        //
        public LogView logview;

        private Image image;

        public Image Image
        {
            get { return image; }
            set { image = value; }
        }


        // 
        // Constructor
        //
        public DocumentLV()
        {
            logview = new LogView();

            image = new Bitmap(500, 500);
        }

        //
        // Methods
        // 
        public bool ReadLogFile(string filePath)
        {
            return logview.ReadLogFile(filePath);
        }

        #region Draw
        public void Draw(Graphics g)
        {
            logview.Draw(g);
        }

        #endregion

        #region UI

        public bool Click()
        {
            return true;
        }

        public bool Wheel(int direction)
        {
            return true;
        }

        public bool CursorMove(Point p)
        {
            return true;
        }

        #endregion


    }
}
