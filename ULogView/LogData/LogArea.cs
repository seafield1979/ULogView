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
    class LogArea
    {
        //
        // Properties
        //
        private string name;            // エリア名
        private UInt32 color;           // エリアの色
        private double timeStart;       // 開始時間(最初のログ時間)
        private double timeEnd;         // 終了時間(最後のログ時間)
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
        public double TimeStart
        {
            get { return timeStart; }
            set { timeStart = value; }
        }
        public double TimeEnd
        {
            get { return timeEnd; }
            set { timeEnd = value; }
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

            timeStart = 10000000;
            timeEnd = 0;
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
            if (timeStart > logData.Time1)
            {
                timeStart = logData.Time1;
            }

            // End
            if (timeEnd < logData.Time2)
            {
                timeEnd = logData.Time2;
            }
            else if (timeEnd < logData.Time1)
            {
                timeEnd = logData.Time1;
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
            sb.AppendFormat(" timeStart:{0}", timeStart);
            if (timeEnd != 0)
            {
                sb.Append(String.Format(" timeEnd:{0}", timeEnd));
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
            sw.Write("area name:{0},color:{1:X8} timeStart:{2}", name, color, timeStart);
            if (timeEnd != 0)
            {
                sw.Write(",timeEnd:{0}", timeEnd);
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
    class MemLogAreaManager
    {
        //
        // Properties 
        //
        private LogArea rootArea;
        private LogArea lastAddArea;

        public LogArea Rootarea
        {
            get { return rootArea; }
            set { rootArea = value; }
        }

        /**
         * Constructor
         */
        public MemLogAreaManager()
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
