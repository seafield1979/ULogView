using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

/**
 * ULogViewでメモリ情報に展開されたログデータ
 */
namespace ULogView
{
    public class LogArea
    {
        //
        // Properties
        //
        private string name;            // エリア名
        private UInt32 color;           // エリアの色
        private double topTime;       // 開始時間(最初のログ時間)
        private double endTime;         // 終了時間(最後のログ時間)
        private Image image;            // 画像

        private List<LogArea> childArea;  // 配下のエリア(areaTypeがDirの場合のみ使用)
        private List<LogData> logs;      // 配下のログ(areaTypeがDataの場合のみ使用)
        private LogArea parentArea;      // 親のエリア
        
        public LogArea ParentArea
        {
            get { return parentArea; }
            set { parentArea = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        
        public UInt32 Color
        {
            get { return color; }
            set { color = value; }
        }

        public Image Image
        {
            get { return image; }
            set { image = value; }
        }

        // エリア内の先頭ログの時間
        public double TopTime
        {
            get { return topTime; }
            set { topTime = value; }
        }

        // エリア内の末尾のログの時間
        public double EndTime
        {
            get { return endTime; }
            set { endTime = value; }
        }
        public List<LogArea> ChildArea
        {
            get { return childArea; }
            set { childArea = value; }
        }
        public List<LogData> Logs
        {
            get { return logs; }
            set { logs = value; }
        }

        //
        // Constructor
        //
        public LogArea()
        {
            this.name = "root";

            topTime = 10000000;
            endTime = 0;
            childArea = null;
            logs = null;
        }
        public LogArea(UInt32 id, string name, UInt32 color, LogArea parentArea) : this()
        {
            this.name = name;
            this.color = color;
            this.parentArea = parentArea;
        }

        //
        // Methods
        //
        /**
         * 配下にエリアを追加
         */
        public void AddChildArea(LogArea logArea)
        {
            if (childArea == null)
            {
                childArea = new List<LogArea>();
            }
            logArea.parentArea = this;
            childArea.Add(logArea);
        }

        /**
         * ログデータを追加
         */
        public void AddLogData(LogData logData)
        {
            if (logs == null)
            {
                logs = new List<LogData>();
            }
            logs.Add(logData);

            // 開始、終了の時間を更新
            // Start
            if (topTime > logData.Time1)
            {
                topTime = logData.Time1;
            }

            // End
            if (endTime < logData.Time2)
            {
                endTime = logData.Time2;
            }
            else if (endTime < logData.Time1)
            {
                endTime = logData.Time1;
            }
        }

        /**
         * コンソールログに出力する
         * 子エリアも同時に出力するため、再帰呼び出しを行う。
         */
        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("area name:{0} ", name);
            sb.AppendFormat(" color:{0:X8}", color);
            sb.AppendFormat(" timeStart:{0}", topTime);
            if (endTime != 0)
            {
                sb.Append(String.Format(" timeEnd:{0}", endTime));
            }
            if (logs != null)
            {
                sb.AppendFormat(" logCount:{0}", logs.Count);
            }
            if (image != null)
            {
                sb.Append(String.Format(" imageSize:{0}", image.Size));
            }

            sb.AppendLine("");

            // ログデータ
            if (logs != null)
            {
                foreach (var log in logs)
                {
                    sb.AppendLine(log.ToString());
                }
            }

            // 子エリア
            if (childArea != null)
            {
                foreach( LogArea area in childArea)
                {
                    sb.Append(area.ToString());
                }
            }

            return sb.ToString();
        }

        public void WriteToFile(StreamWriter sw)
        {
            sw.Write("area name:{0},color:{1:X8} timeStart:{2}", name, color, topTime);
            if (endTime != 0)
            {
                sw.Write(",timeEnd:{0}", endTime);
            }
            if (image != null)
            {
                sw.Write(",imageSize:{0}", image.Size);
            }
            if (logs != null)
            {
                sw.Write(",logCount:{0}", logs.Count);
            }
            if (parentArea != null)
            {
                sw.Write(",parentArea:{0}", parentArea.Name);
            }

            sw.WriteLine();

            // ログデータ
            if (logs != null)
            {
                foreach (var log in logs)
                {
                    sw.WriteLine(log.ToString());
                }
            }

            // 子エリア
            // 再帰的に子エリアをたどっていく
            if (childArea != null)
            {
                foreach (LogArea area in childArea)
                {
                    area.WriteToFile(sw);
                }
            }
        }

        public void Print()
        {
            Console.WriteLine(this);
        }

    }  // class LogArea

    //
    // メモリ情報エリアを管理するクラス
    // このクラスを使用してエリアを追加する
    // 
    class LogAreaManager
    {
        //
        // Properties 
        //
        private LogArea rootArea;
        private LogArea lastAddArea;

        public LogArea RootArea
        {
            get { return rootArea; }
            set { rootArea = value; }
        }

        /**
         * Constructor
         */
        public LogAreaManager()
        {
            rootArea = new LogArea();
            lastAddArea = rootArea;
        }

        //
        // Methods
        //
        /**
         * 追加先を指定してエリアを追加
         * 
         * @input logArea: 追加するログエリア
         * @input parentName: 追加先の親エリア名
         */
        public void AddArea(LogArea logArea)
        {
            if (logArea.ParentArea == null)
            {
                // 最後に追加したエリアと同じ階層（同じ親の下）に追加する
                if (lastAddArea != null)
                {
                    if (lastAddArea.ParentArea != null)
                    {
                        lastAddArea.ParentArea.AddChildArea(logArea);
                    }
                    else
                    {
                        rootArea.AddChildArea(logArea);
                    }
                }
                else
                {
                    rootArea.AddChildArea(logArea);
                }
            }
            else
            {
                // 指定した親の下に追加
                logArea.ParentArea.AddChildArea(logArea);
            }

            lastAddArea = logArea;
        }

        /**
         * ログを追加
         */
        public void AddLogData(LogData logData)
        {
            lastAddArea.AddLogData(logData);
        }

        /**
         * 指定の名前のエリアを探す
         * ※エリアを追加できるポイントは、自分の親（親の親も含む）に限られるのでその範囲で探す
         * @input name: 探すエリア名
         */
        public LogArea searchArea(string name)
        {
            // １つもエリアを追加していない場合はルート
            if (lastAddArea == null)
            {
                return rootArea;
            }

            LogArea area = lastAddArea;

            while(area != rootArea)
            {
                if (area.Name.Equals(name))
                {
                    return area;
                }
                area = area.ParentArea;
            }

            // 見つからなかった場合はルート
            return rootArea;
        }


        #region Static Methods

        /**
         * 指定エリア以下でログが存在するレーンのディクショナリを取得する
         */
        public static Dictionary<int, Lane> GetDispLaneList(LogArea area, Lanes lanes)
        {
            var dic1 = new Dictionary<int, bool>();
            var dic2 = new Dictionary<int, Lane>();

            GetDispLaneListOne(dic1, area);

            // 戻り値用のDictionaryを作成
            foreach (int laneID in dic1.Keys)
            {
                foreach (Lane lane in lanes)
                {
                    // dicのキーに表示すべきレーンのキーが入っているのでこれと一致するレーンが
                    // 見つかったら戻り値用のDictionaryに追加する
                    if (lane.ID == laneID)
                    {
                        dic2[laneID] = lane;
                        break;
                    }
                }
            }
            return dic2;
        }

        /**
         * GetDispLaneListの１エリア分の処理
         * @input dic : 情報設定先
         * @input area : 対象のエリア
         */
        private static void GetDispLaneListOne(Dictionary<int, bool> dic, LogArea area)
        {
            if (area.Logs != null && area.Logs.Count > 0)
            {
                foreach (LogData log in area.Logs)
                {
                    dic[(int)log.LaneId] = true;
                }
            }

            // 子要素を探索して処理
            if (area.ChildArea != null)
            {
                foreach(LogArea cArea in area.ChildArea)
                {
                    GetDispLaneListOne(dic, cArea);
                }
            }
        }

        /**
         * 指定したエリア以下のログの状態をクリアする
         * @input area : 指定のエリア
         */
        public static void ResetLogData(LogArea area)
        {
            if (area.Logs != null && area.Logs.Count > 0)
            {
                foreach (LogData log in area.Logs)
                {
                    log.ClearState();
                }
            }

            // 子要素を探索して処理
            if (area.ChildArea != null)
            {
                foreach (LogArea cArea in area.ChildArea)
                {
                    ResetLogData(cArea);
                }
            }
        }

        #endregion


        #region Debug
        public void Print()
        {
            rootArea.Print();
        }

        public void WriteToFile(StreamWriter sw)
        {
            rootArea.WriteToFile(sw);
        }
        #endregion
    }
}
