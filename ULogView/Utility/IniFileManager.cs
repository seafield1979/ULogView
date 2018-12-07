using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ULogView.Utility
{
    /// <summary>
    /// iniファイルを管理するクラス
    /// </summary>
    class IniFileManager
    {
        //
        // Const
        //
        private const string IniFilePath = @".\LogView.ini";


        //
        // Property
        //
        private Dictionary<string, Dictionary<string, string>> sections = new Dictionary<string, Dictionary<string, string>>();

        // 
        // Constructor
        //
        private static IniFileManager singleton;
        public static IniFileManager Singleton
        {
            get {
                if (singleton == null)
                {
                    singleton = new IniFileManager();
                }
                return singleton;
            }
        }

        //
        // Method
        //

        public void SetString(string section, string key, string value)
        {
            SetData(section, key, value);
        }

        public void SetData(string section, string key, string value)
        {
            // sectionがないなら追加
            if (sections.ContainsKey(section) == false)
            {
                sections[section] = new Dictionary<string, string>();
            }
            sections[section][key] = value;
        }

        private bool GetData(string section, string key, out string getData )
        {
            if (sections.ContainsKey(section))
            {
                if (sections[section].ContainsKey(key))
                {
                    getData = sections[section][key];
                    return true;
                }
            }
            getData = null;
            return false;
        }

        public bool GetString(string section, string key, out string getData)
        {
            return GetData(section, key, out getData);
        }

        public bool GetInt(string section, string key, out int getData)
        {
            string str;
            if (GetData(section, key, out str))
            {
                if (int.TryParse(str, out getData))
                {
                    return true;
                }
            }
            getData = 0;
            return false;
        }

        public bool GetBool(string section, string key, out bool getData)
        {
            string str;
            if (GetData(section, key, out str))
            {
                if (bool.TryParse(str, out getData))
                {
                    return true;
                }
            }
            getData = false;
            return false;
        }

        public bool GetFloat(string section, string key, out float getData)
        {
            string str;
            if (GetData(section, key, out str))
            {
                if (float.TryParse(str, out getData))
                {
                    return true;
                }
            }
            getData = 0;
            return false;
        }

        //
        // Array
        //
        public void SetArray(string section, string key, string[] array)
        {
            int count = 1;
            foreach(string str in array)
            {
                string _key = string.Format("{0}_{1:D2}", key, count);
                SetString(section, _key, str);
                count++;
            }
        }

        public bool GetArray(string section, string key, out string[] getData)
        {
            int count = 1;
            List<string> list = new List<string>();
            while (true)
            {
                string _key = string.Format("{0}_{1:D2}", key, count);
                string str;
                if (GetString(section, _key, out str))
                {
                    list.Add(str);
                }
                else
                {
                    break;
                }
                count++;
            }

            if (list.Count == 0)
            {
                getData = null;
                return false; 
            }
            getData = list.ToArray();
            return true;
        }


        //
        // File Read/Write
        //
        public bool ReadFromFile()
        {
            try
            {
                using (StreamReader sr = new StreamReader(IniFilePath, Encoding.GetEncoding("shift_jis")))
                {
                    sections.Clear();

                    string section = null;

                    while (sr.EndOfStream == false)
                    {
                        string line = sr.ReadLine();
                        if (line.Contains("[") && line.Contains("]"))
                        {
                            section = line.Replace("[", "").Replace("]", "");
                            if (string.IsNullOrEmpty(section) == false)
                            {
                                sections[section] = new Dictionary<string, string>();
                            }
                        }
                        else if (section != null)
                        {
                            string[] splitted = line.Split(new char[] { '=' });
                            if (splitted != null && splitted.Length >= 2)
                            {
                                SetString(section, splitted[0], splitted[1]);
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("iniファイルが読み込めませんでした。");
                return false;
            }
            return true;
        }

        public bool WriteToFile()
        {
            try
            {
                using(StreamWriter sw = new StreamWriter(IniFilePath, false, Encoding.GetEncoding("shift_jis")))
                {
                    // セクションごとにデータを書き込む
                    foreach( string sectionName in sections.Keys)
                    {
                        sw.WriteLine("[" + sectionName + "]");
                        
                        foreach( KeyValuePair<string, string>kvp in sections[sectionName])
                        {
                            sw.WriteLine("{0}={1}", kvp.Key, kvp.Value);
                        }
                    }                    
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("iniファイルの書き込みに失敗しました。");
                return true;
            }
            return true;
        }
    }
}
