using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULogView
{
    /**
  * Json形式のデータ
  * 
  * key = string, value = string
  * { "key" : "value" }
  * 
  * key = string, value = numeric value
  * { "key" : value }
  * 
  * dictionary
  * { "key1" : "value1", "key2" : "value2" ...}
  * 
  * array
  * [ value1, value2, value3, ... ]
  * 
  * tree
  * { "key1" : {"key1-1" : "value", "key1-2" : "value" }, "key2" : {"key1-1" : "value", "key1-2" : "value" }}
  */

    enum EJsonReadMode : int
    {
        None = 0,       
        DicKey,         // 辞書型のキーを読み込み中
        DicValue,       // 辞書型の値を読み込み中
        Array           // 配列の値を読み込み中
    }

    class MemDetailData
    {
        // 
        // Properties
        //
        // 文字列、配列、辞書型のどれからのオブジェクトが入る
        private Object obj;

        public Object Obj
        {
            get { return obj; }
            set { obj = value; }
        }

        //
        // Constructor
        //

        //
        // Methods
        //
        /**
         * JSON形式の文字列をメモリ上のツリー構造に展開する
         * 
         * @input jsonStr : JSON形式の文字列
         * @output : メモリ上のツリー構造のオブジェクト
         */
        public static MemDetailData Deserialize(string jsonStr)
        {
            if (jsonStr == null || jsonStr.Length == 0)
            {
                return null;
            }

            char[] chars = jsonStr.ToCharArray();
            int offset = 0;

            MemDetailData jsonData = new MemDetailData();

            jsonData.Obj = GetJson(chars, ref offset);
            return jsonData;
        }

        /**
         * １階層分のJSONデシリアライズを行う
         * JSON文字列中の１個分の配列、辞書型の末尾までを取得してList, Dictionary形式に取得
         * 
         * @input chars : シリアライズ対象のJSON形式のキャラ配列
         * @input offset : chars の参照位置
         */
        private static object GetJson(char[] chars, ref int offset)
        {
            string keyStr = null;
            EJsonReadMode readMode = EJsonReadMode.None;
            var cbuf = new List<char>();
            object value = null;
            object newObj = null;

            for (; offset < chars.Length; offset++)
            {
                char c = chars[offset];

                if (c == ' ' || c == '\n')
                {
                    // スペースと改行はスキップ
                    continue;
                }

                switch (readMode)
                {
                    case EJsonReadMode.None:
                        if (c == '[')
                        {
                            newObj = new List<object>();
                            readMode = EJsonReadMode.Array;
                        }
                        else if (c == '{')
                        {
                            newObj = new Dictionary<string, object>();
                            readMode = EJsonReadMode.DicKey;
                        }
                        else if (c == '"')
                        {
                            newObj = GetString(chars, ref offset, true);
                        }
                        else
                        {
                            // ダブルコートで囲まれていない文字列
                            for (; offset < chars.Length; offset++)
                            {
                                c = chars[offset];
                                cbuf.Add(c);
                            }
                            newObj = new String(cbuf.ToArray());
                        }
                        break;
                    case EJsonReadMode.DicKey:
                        switch (c)
                        {
                            case '"':
                                keyStr = GetString(chars, ref offset, false);
                                break;
                            case ':':
                                readMode = EJsonReadMode.DicValue;
                                break;
                            case ',':
                                // 値を取得前なので、キーのみ
                                if (keyStr != null)
                                {
                                    ((Dictionary<string, object>)newObj)[keyStr] = "";
                                }
                                break;
                            case ']':
                                return newObj;
                        }
                        break;
                    case EJsonReadMode.DicValue:
                        if (keyStr == null)
                        {
                            // キーがない場合は値を取得しても意味がないのでスキップ
                            for (; offset < chars.Length; offset++)
                            {
                                c = chars[offset];
                                if (c == ',')
                                {
                                    readMode = EJsonReadMode.DicKey;
                                }
                                else if (c == '}')
                                {
                                    return newObj;
                                }
                            }
                        }
                        switch (c)
                        {
                            case '"':
                                value = GetString(chars, ref offset, true);
                                break;
                            case '{':
                            case '[':
                                // 別のブロックを見つけたので処理をネスト
                                value = GetJson(chars, ref offset);
                                break;
                            case ',':
                            case '}':
                                if (cbuf.Count > 0 && value == null)
                                {
                                    value = new string(cbuf.ToArray());
                                }
                                ((Dictionary<string, object>)newObj)[keyStr] = value;
                                value = null;
                                readMode = EJsonReadMode.DicKey;

                                if (c == '}')
                                {
                                    return newObj;
                                }
                                break;
                            default:
                                cbuf.Add(c);
                                break;
                        }
                        break;
                    case EJsonReadMode.Array:
                        switch (c)
                        {
                            case '"':
                                value = GetString(chars, ref offset, true);
                                break;
                            case '{':
                            case '[':
                                // 別のブロックを見つけたので処理をネスト
                                value = GetJson(chars, ref offset);
                                break;
                            case ']':
                            case ',':
                                if (cbuf.Count > 0 && value == null)
                                {
                                    value = new string(cbuf.ToArray());
                                    cbuf.Clear();
                                }
                                ((List<object>)newObj).Add(value);
                                value = null;
                                if (c == ']')
                                {
                                    return newObj;
                                }
                                break;
                            default:
                                cbuf.Add(c);
                                break;
                        }
                        break;
                }
            }
            return newObj;
        }


        /**
         * ダブルクオートで囲まれた文字列を取得する
         * 
         * @input chars : 取得先のキャラ配列
         * @input offset : chars の参照オフセット
         * @input dqOn : ダブルクオートを残すかどうか(true:残す / false:残さない)
         * @output : 取得した文字列
         */
        private static string GetString(char[] chars, ref int offset, bool dqOn)
        {
            var cbuf = new List<char>();
            bool isEscape = false;

            // 先頭のダブルクオートはスキップ
            if (chars[offset] == '"')
            {
                offset++;
            }

            for (; offset < chars.Length; offset++)
            {
                char c = chars[offset];
                if (isEscape)
                {
                    switch (c)
                    {
                        case '\\':
                            cbuf.Add(c);
                            break;
                        case 'n':
                            cbuf.Add('\n');
                            break;
                        case 't':
                            cbuf.Add('\t');
                            break;
                        case '"':
                            cbuf.Add('"');
                            break;
                    }
                    isEscape = false;
                }
                if (c == '"')
                {
                    if (dqOn)
                    {
                        // ダブルクオートを残す
                        return "\"" + (new string(cbuf.ToArray())) + "\"";
                    }
                    else
                    {
                        return new string(cbuf.ToArray());
                    }
                }
                else if (c == '\\')
                {
                    isEscape = true;
                }
                else
                {
                    cbuf.Add(c);
                }
            }

            return null;
        }

        /**
         * 文字列に変換
         */
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            ObjToString(sb, this.obj, "");

            return sb.ToString();
        }

        /**
         * １階層分のリストを文字列に変換する
         * 
         * @input sb:    変換した結果を格納する先
         * @input nodeObj: 変換元のデータ(string、array、dictionaryのどれか)
         * @input nest:  階層をネストするたびに文字列の先頭に付加するスペース
         */
        private static void ObjToString(StringBuilder sb, object node, string nest)
        {
            int cnt = 0;
            string nest2;

            if (node is string)
            {
                sb.AppendFormat((string)node);
            }
            else if (node is List<object>) {
                sb.Append("[\r\n");
                nest2 = nest + "  ";
                foreach (object value in (node as List<object>))
                {
                    if (value is List<object> || value is Dictionary<string, object>)
                    {
                        sb.AppendFormat("{0}[{1}]=", nest2, cnt);
                        ObjToString(sb, value, nest2);
                    }
                    else if (value is string)
                    {
                        sb.AppendFormat("{0}[{1}]={2}\r\n", nest2, cnt, value);
                    }
                    cnt++;
                }
                sb.AppendFormat("{0}]\r\n", nest);
            }
            else if (node is Dictionary<string, object>)
            {
                sb.Append("{\r\n");
                nest2 = nest + "  ";
                foreach (KeyValuePair<string, object> kvp in (node as Dictionary<string, object>))
                {
                    if (kvp.Value is List<object> || kvp.Value is Dictionary<string, object>)
                    { 
                        sb.AppendFormat("{0}\"{1}\"=", nest2, kvp.Key);

                        ObjToString(sb, kvp.Value, nest2);
                    }
                    else if (kvp.Value is string)
                    {
                        sb.AppendFormat("{0}\"{1}\"={2}\r\n", nest2, kvp.Key, kvp.Value);
                    }
                }
                sb.AppendFormat("{0}}}\r\n", nest);
            }
        }
    }
}
