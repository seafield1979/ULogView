using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULogView
{
    /**
     * LogView全体のオプション
     */
    public class LogViewOption
    {
        //
        // Properties
        //
        #region Properties
        // 画面の表示サイズ
        // 1.0 = 100% 1.5 = 150%
        private float zoomRate;

        public float ZoomRate
        {
            get { return zoomRate; }
            set { zoomRate = value; }
        }

        //public DrawDir drawDir;        // 表示方向(0: 縦 / 1:横)
        private DrawDirection drawDir;

        public DrawDirection DrawDir
        {
            get { return drawDir; }
            set { drawDir = value; }
        }



        #endregion Properties

        //
        // Static variable
        //
        private static LogViewOption singletonObject = new LogViewOption();


        //
        // Constructor
        //
        private LogViewOption()
        {
            zoomRate = 1.0f;
        }
        
        // Singleton object
        public static LogViewOption GetObject()
        {
            // オーバーヘッドが発生するので生成チェックはしない
            //if (singletonObject == null)
            //{
            //    singletonObject = new LogViewOption();
            //}
            return singletonObject;
        }

        //
        // Methods
        //
        #region Methods

        /*
         * 拡大
         */
        public void ZoomIn()
        {
            zoomRate *= 1.2f;
        }
        /**
         * 縮小
         */
        public void ZoomOut()
        {
            zoomRate *= 0.8f;
        }

        /**
         * デフォルトの拡大率にする
         */
        public void SetDefaultZoom()
        {
            zoomRate = 1.0f;
        }

        #endregion
    }
}
