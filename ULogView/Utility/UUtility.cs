using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULogView.Utility
{
    /**
     * ULoggerの汎用処理
     */
    class UUtility
    {
        /**
         * テキストからエンコーディングを取得する
         * @input encStr: エンコード名
         */
        public static Encoding GetEncodingFromStr(string encStr)
        {
            Encoding encoding = Encoding.UTF8;      // デフォルトのエンコード

            switch (encStr.ToLower())
            {
                case "ascii":
                    encoding = Encoding.ASCII;
                    break;
                case "utf7":
                case "utf-7":
                    encoding = Encoding.UTF7;
                    break;
                case "utf8":
                case "utf-8":
                    encoding = Encoding.UTF8;
                    break;
                case "utf32":
                case "utf-32":
                    encoding = Encoding.UTF32;
                    break;
                case "unicode":
                    encoding = Encoding.Unicode;
                    break;
                default:
                    encoding = Encoding.GetEncoding(encStr);
                    break;
            }
            return encoding;
        }

        /**
         * エンコーディングからテキストを取得する
         * 
         * @input encoding: エンコーディングオブジェクト
         */
        public static string GetEncodingStr(Encoding encoding)
        {
            if (encoding.Equals(Encoding.ASCII))
            {
                return "ascii";
            }
            else if (encoding.Equals(Encoding.UTF7))
            {
                return "utf7";
            }
            else if (encoding.Equals(Encoding.UTF8))
            {
                return "utf8";
            }
            else if (encoding.Equals(Encoding.UTF32))
            {
                return "utf32";
            }
            else if (encoding.Equals(Encoding.Unicode))
            {
                return "unicode";
            }
            else
            {
                encoding.ToString();
            }
            return null;
        }
    }
}
