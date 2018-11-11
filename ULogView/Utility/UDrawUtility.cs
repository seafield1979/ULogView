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
