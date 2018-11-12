using System;
using System.Collections.Generic;
using System.Drawing;

//
// Graphicsの汎用描画処理
//

namespace ULogView.Utility
{
    class UDrawUtility
    {
        //
        // Properties
        //
        #region Properties

        // 再利用ブラシリスト
        private static Dictionary<int, Brush> brushList = new Dictionary<int, Brush>();

        private static Dictionary<int, Pen> penList = new Dictionary<int, Pen>();

        #endregion Properties

        // 
        // Methods
        //

        #region Methods

        /// <summary>
        /// ブラシを取得する。過去に作成済みのものはリストから取得。
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Brush GetBrush(int color)
        {
            if (brushList.ContainsKey(color) == false)
            {
                brushList.Add(color, new SolidBrush(Color.FromArgb(color)));
            }
            return brushList[color];
        }

        public static Pen GetPen(int color)
        {
            if (penList.ContainsKey(color) == false)
            {
                penList.Add(color, new Pen(GetBrush(color)));
            }
            return penList[color];
        }

        #endregion Methods


        /// <summary>
        /// 中心座標と半径を指定して円を描画(内部塗りつぶし)
        /// </summary>
        /// <param name="g"></param>
        /// <param name="color"></param>
        /// <param name="pos"></param>
        /// <param name="length"></param>
        public static void FillCircle(Graphics g, Brush brush, int x, int y, float radius)
        {
            g.FillEllipse(brush, x - radius, y - radius, radius * 2, radius * 2);
            
        }

        /// <summary>
        /// 中心座標と半径を指定して円を描画（線）
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pen"></param>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        public static void DrawCircle(Graphics g, Pen pen, int x, int y, float radius)
        {
            g.DrawEllipse(pen, x - radius, y - radius, radius * 2, radius * 2);
        }
    }
}
