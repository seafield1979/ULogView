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
    public class LogViewPixTime
    {
        struct SPixTime
        {
            //public string unitName;    // 時間の単位
            public EUnitType unitType;  // 時間の単位
            public double pixTime;     // 1ピクセル当たりの時間
            public double calcTime;    // unitTime計算用の掛け算の値
        }

        // 時間の単位
        enum EUnitType : byte
        {
            Nano,       // ナノ秒  1/1000000000
            Micro,      // マイクロ秒 1/1000000
            Milli,      // ミリ秒 1/1000
            Second      // 秒
        }

        enum EPixTime : byte
        {
            E1_0N,     // 1pix = 1nano s
            E1_5N,
            E2_0N,
            E3_0N,
            E5_0N,
            E7_5N,
            E10N,
            E15N,
            E20N,
            E30N,
            E50N,
            E75N,
            E100N,
            E150N,
            E200N,
            E300N,
            E500N,
            E750N,
            E1_0U,       // 1pix = 1 micro s
            E1_5U,
            E2_0U,
            E3_0U,
            E5_0U,
            E7_5U,
            E10U,
            E15U,
            E20U,
            E30U,
            E50U,
            E75U,
            E100U,
            E150U,
            E200U,
            E300U,
            E500U,
            E750U,
            E1_0M,      // 1pix = 1 milli second
            E1_5M,
            E2_0M,
            E3_0M,
            E5_0M,
            E7_5M,
            E10M,
            E15M,
            E20M,
            E30M,
            E50M,
            E75M,
            E100M,
            E150M,
            E200M,
            E300M,
            E500M,
            E750M,
            E1_0S,      // 1pix = 1 second
            E1_5S,
            E2_0S,
            E3_0S,
            E5_0S,
            E7_5S,
            E10S,
            E15S,
            E20S,
            E30S,
            E50S,
            E75S,
            E100S,
            E150S,
            E200S,
            E300S,
            E500S,
            E750S,
            E1000S
        }

        static SPixTime[] pixTimeTbl = new SPixTime[]
        {
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.000000001, calcTime=0.1 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.0000000015 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.000000002 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.000000003 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.000000005 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.0000000075 },

            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.00000001 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.000000015 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.00000002 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.00000003 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.00000005 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.000000075 },

            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.0000001 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.00000015 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.0000002 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.0000003 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.0000005 },
            new SPixTime(){ unitType=EUnitType.Nano, pixTime = 0.00000075 },

            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.000001 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.0000015 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.000002 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.000003 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.000005 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.0000075 },

            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.00001 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.000015 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.00002 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.00003 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.00005 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.000075 },

            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.0001 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.00015 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.0002 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.0003 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.0005 },
            new SPixTime(){ unitType=EUnitType.Micro, pixTime = 0.00075 },

            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.001 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.0015 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.002 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.003 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.005 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.0075 },

            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.01 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.015 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.02 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.03 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.05 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.075 },

            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.1 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.15 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.2 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.3 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.5 },
            new SPixTime(){ unitType=EUnitType.Milli, pixTime = 0.75 },

            new SPixTime(){ unitType=EUnitType.Second, pixTime = 1 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 1.5 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 2.0 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 3.0 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 5.0 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 7.5 },

            new SPixTime(){ unitType=EUnitType.Second, pixTime = 10 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 15 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 20 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 30 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 50 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 75 },

            new SPixTime(){ unitType=EUnitType.Second, pixTime = 100 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 150 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 200 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 300 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 500 },
            new SPixTime(){ unitType=EUnitType.Second, pixTime = 750 },

            new SPixTime(){ unitType=EUnitType.Second, pixTime = 1000 },
        };

        //
        // Properties
        //

        private EPixTime pixTime;

        // 1pixelあたりの時間
        public double Val
        {
            get
            {
                return pixTimeTbl[(byte)pixTime].pixTime;
            }
        }

        //
        // Constructor
        //
        public LogViewPixTime()
        {
            // 1000pix = 1sec -> 1pix = 0.001sec = 1ms
            //pixTime = EPixTime.E1_0M;
            pixTime = EPixTime.E1_0M; 
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
            if (pixTime > EPixTime.E1_0N)
            {
                pixTime--;
            }
        }

        /**
         * ズームアウト
         * 表示される領域が広くなる(1pixあたりの時間が大きくなる)
         * 1pix = 0.01秒 -> 1pix = 0.02秒
         */
        public void ZoomOut()
        {
            if (pixTime < EPixTime.E1000S)
            {
                pixTime++;
            }
        }

        public override string ToString()
        {
            //return String.Format("1pix={0}sec", val);
            return String.Format("1sec={0}pix", Val);
        }

        /**
         * 時間を現在のズーム率でpixelに変換する
         * @input time : 変換元の時間(sec)
         * @output 変換後のpixel数
         */
        public int timeToPix(double time, float zoom)
        {
            // 1秒あたりのpixel数を取得してから時間(sec)を書ける
            return (int)((1.0 / Val) * time * zoom);
        }

        /**
         * pixelを現在のズーム率で時間に変換する
         * @input pix : 変換元のpixel数
         * @output 変換後の時間(sec)
         */
        public double pixToTime(int pix)
        {
            return pix * Val;
        }
        /**
         * pixelを現在のズーム率で時間に変換し、適切な時間単位で表現した文字列を返す
         * @input pix : 変換元のpixel数
         * @output 変換後の単位付きの時間 (ns,us,ms,sのどれか)
         */
        public string timeToStr(double time)
        {
            switch (pixTimeTbl[(byte)pixTime].unitType)
            {
                case EUnitType.Nano:
                    // 0.000000001 -> 1 
                    return String.Format("{0:0.0}ns", time * 1000000000);
                case EUnitType.Micro:
                    // 0.000001 -> 1
                    return String.Format("{0:0.0}us", time * 1000000);
                case EUnitType.Milli:
                    // 0.001 -> 1
                    return String.Format("{0:0.0}ms", time * 1000);
                default:
                    return String.Format("{0:0.0}s", time);
            }
        }
    }

    /*
     * 拡大率を管理するクラス
     */
    public class ZoomRate
    {
        enum EZoomRate : byte
        {
            E50P = 0,
            E67P,
            E75P,
            E80P,
            E90P,
            E100P,  // 100%
            E110P,
            E125P,
            E150P,
            E175P,
            E200P,
            E250P,
            E300P,
            E400P
        }
        private EZoomRate zoomRate;

        private float value;

        public float Value
        {
            get { return value; }
            set { value = value; }
        }

        //
        // Consts
        //
        private float[] eToV = new float[] { 0.5f, 0.67f, 0.75f, 0.8f, 0.9f, 1.0f, 1.1f, 1.25f, 1.5f, 1.75f, 2.0f, 2.5f, 3.0f, 4.0f };

        //
        // Constructor
        // 
        public ZoomRate()
        {
            zoomRate = EZoomRate.E100P;
            SetZoomValue();
        }

        private void SetZoomValue()
        {
            value = eToV[(byte)zoomRate];
        }

        public float ZoomIn()
        {
            if (zoomRate < EZoomRate.E400P)
            {
                zoomRate++;
            }
            SetZoomValue();
            return value;
        }

        public float ZoomOut()
        {
            if (zoomRate > EZoomRate.E50P)
            {
                zoomRate--;
            }
            SetZoomValue();
            return value;
        }
    }
}
