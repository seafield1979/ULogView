using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULogView
{
    /**
     * LogViewの1pixelあたりの表示時間
     */
    class LogViewPixTime
    {
        //
        // Properties
        //

        // 1pixelあたりの時間
        private double val;

        public double Val
        {
            get { return val; }
            set { val = value; }
        }

        //
        // Constructor
        //
        public LogViewPixTime()
        {
            // 100pix = 1sec -> 1pix = 0.01sec
            val = 0.01;
        }
        
        //
        // Medhots
        //
        /**
         * ズームイン
         * 表示される領域が狭くなる(1pixあたりの時間が小さくなる)
         * 1pix = 0.01秒 -> 1pix = 0.005秒
         */
        public void ZoomIn()
        {
            val *= 0.8f;
        }

        /**
         * ズームアウト
         * 表示される領域が広くなる(1pixあたりの時間が大きくなる)
         * 1pix = 0.01秒 -> 1pix = 0.02秒
         */
        public void ZoomOut()
        {
            val *= 1.2f;
        }

        public override string ToString()
        {
            //return String.Format("1pix={0}sec", val);
            return String.Format("1sec={0}pix", val);
        }

        /**
         * 時間を現在のズーム率でpixelに変換する
         * @input time : 変換元の時間(sec)
         * @output 変換後のpixel数
         */
        public UInt64 timeToPix(double time)
        {
            // 1秒あたりのpixel数を取得してから時間(sec)を書ける
            return (UInt64)((1.0 / val) * time);
        }

        /**
         * pixelを現在のズーム率で時間に変換する
         * @input pix : 変換元のpixel数
         * @output 変換後の時間(sec)
         */
        public double pixToTime(int pix)
        {
            return pix * val;
        }
    }
}
