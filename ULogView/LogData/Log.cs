using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULogView.Utility;

namespace ULogView
{
    // 色の定義
    // ログやエリアの色指定に使用する
    class UColor
    {
        public const UInt32 Black = 0xFF000000;
        public const UInt32 Red = 0xFFFF0000;
        public const UInt32 Green = 0xFF00FF00;
        public const UInt32 Blue = 0xFF0000FF;
        public const UInt32 Yellow = 0xFFFFFF00;
        public const UInt32 Gray = 0xFF808080;
    }

    // 本体以下のログの種類
    public enum LogFileType : byte
    {
        Data = 1,       // データ(ログ)
        Area = 2        // エリア(ログを入れる箱)
    }

    // Type of Log
    // ログの種別
    public enum LogDataType : byte
    {
        Single = 0,    // 単体ログ
        RangeStart,    // 範囲開始
        RangeEnd,      // 範囲終了
        Value          // 値
    }

    /**
     * 抽象クラス
     * ログバッファに追加するオブジェクトは、このクラスを派生すること
     */
    abstract class Log
    {
        // Constructor
        // 抽象クラスなのでインスタンスは生成できない
        public Log()
        {
        }

        // Convert log to string
        // public abstract string ToString();

        // Convert log to byte array
        public abstract byte[] ToBinary();

        // Write log to binary file
        public abstract void WriteToBinFile(UFileStream fs, Encoding encoding);
    }

}
