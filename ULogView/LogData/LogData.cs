using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * ULogViewでメモリ上に展開されたLogArea
 */
namespace ULogView
{
    enum MemLogType : byte
    {
        Point,          // 1: 点ログ
        Range,          // 2: 範囲ログ
        Value,          // 3: 値ログ (値の遷移をグラフ表示できる)
        Bind            // 4: 結合ログ (近いログをまとめて表示した状態)
    }

    class LogData
    {
        //
        // Properties
        //

        //ログID(マスタ参照)
        private UInt32 id;

        public UInt32 ID
        {
            get { return id; }
            set { id = value; }
        }

        //ログ名(マスタの参照値を保持する)
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        //ログ種類
        private MemLogType type;

        public MemLogType Type
        {
            get { return type; }
            set { type = value; }
        }

        //ログ色
        private UInt32 color;

        public UInt32 Color
        {
            get { return color; }
            set { color = value; }
        }

        //表示レーン
        private UInt32 laneId;

        public UInt32 LaneId
        {
            get { return laneId; }
            set { laneId = value; }
        }

        private string text;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }


        //時間1
        private double time1;

        public double Time1
        {
            get { return time1; }
            set { time1 = value; }
        }

        //時間2(範囲タイプの場合に使用する)
        private double time2;

        public double Time2
        {
            get { return time2; }
            set { time2 = value; }
        }

        //表示時間の末端の座標（横画面表示時に次のログと重ならないようにするために、表示テキストも含めた末尾の座標を保持する)
        private UInt32 endPos;

        public UInt32 EndPos
        {
            get { return endPos; }
            set { endPos = value; }
        }

        //ログの詳細(Todo 将来的に種別ごとの構造(配列やツリー等)にする）
        private object detail;

        public object Detail
        {
            get { return detail; }
            set { detail = value; }
        }

        //ログのデータ(todo)
        //表示ログ情報（高速化用)(todo)
        //表示スキップフラグ (前のログに結合されて表示が不要になったログ、拡大率が変わった場合もとに戻る)
        private bool skipFlag;

        public bool SkipFlag
        {
            get { return skipFlag; }
            set { skipFlag = value; }
        }

        //表示幅
        //１レーンにログが重なった場合に、１ログの幅がレーン幅を分割したサイズになる。このときの幅。
        private int width;

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        //表示位置
        // １レーンを複数のログを並べた場合の、表示座標。
        private int pos;

        public int Pos
        {
            get { return pos; }
            set { pos = value; }
        }


        //
        // Constructor
        //
        public LogData()
        {
            
        }

        public LogData(UInt32 id, LogType type, byte laneId, double time1, double time2, string text, string detailText)
        {
            this.id = id;
            this.type = (MemLogType)type;
            this.laneId = laneId;
            this.time1 = time1;
            this.time2 = time2;
            this.text = text;

            this.detail = MemDetailData.Deserialize(detailText);
        }

        /**
         * 文字列に変換
         */
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("data id:{0}", id);
            sb.AppendFormat(" type:{0}", type.ToString());
            sb.AppendFormat(" laneId:{0}", laneId);
            sb.AppendFormat(" time1:{0}", time1);
            if (time2 != 0)
            {
                sb.AppendFormat(" time2:{0}", time2);
            }
            sb.AppendFormat(" text:{0}", text);
            if (detail != null)
            {
                sb.AppendFormat(" detailText:{0}", detail);
            }

            return sb.ToString();
        }
        /*
         * コンソールログ出力
         */
        public void Print()
        {
            Console.WriteLine(this);
        }
    }
}
