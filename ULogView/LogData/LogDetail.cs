using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULogView
{
    //
    // データタイプの種類
    //
    enum DetailDataType : byte
    {
        None = 0,       // 詳細情報なし
        Text,           // テキスト  "hello"
        Array,          // 配列 {0,1,2,3...}
        Dictionary,     // 辞書 {"key1"="value1", "key2="value2", ...}
        JSON            // JSON {"key1"="value1", "dic1"={"key2"="value2",...}, "array1"=[0,1,2,3]}
    }

    //
    // LogDetailはログの詳細を出力するクラスの親クラス。
    // ユーザーは自分のプログラムでログの詳細を出力したいクラスの親クラスにこれを指定します。
    //
    abstract class LogDetail
    {
        // Constructor
        public LogDetail() {
        }

        // Methods

        // オブジェクトのコピーを作成する
        // 派生クラスで実装する。
        // ※ログを追加してから実際にファイルに書き込むまでの間に詳細情報が変化、消滅してしまう場合がある。
        public abstract LogDetail CreateCopy();

        
        // 詳細の種別をバイトで返す
        public abstract byte dataTypeByte();

        // 詳細の文字列を返す
        //public abstract string ToString();
        
        // 詳細の文字列をバイト配列で返す
        public abstract byte[] ToBinary();
    }
}
