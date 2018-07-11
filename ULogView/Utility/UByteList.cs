using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULogView.Utility
{
    /**
     * 様々なデータをByte配列に変換するためのクラス
     */
    class UByteList
    {
        //
        // Properties
        //
        private List<byte> list;

        private int offset;

        private Encoding encoding;

        public Encoding EncodingType
        {
            get { return encoding; }
            set { encoding = value; }
        }

        //
        // Constructor
        //
        #region Constructor
        public UByteList()
        {
            list = new List<byte>(1000);
            offset = 0;
            encoding = Encoding.UTF8;
        }
        
        #endregion

        //
        // Methods
        //
        public byte[] ToArray()
        {
            return list.ToArray();
        }

        #region Add

        public void AddBool(bool value)
        {
            list.Add((byte)(value ? 1 : 0));
        }

        public void AddChar(char value)
        {
            list.Add((byte)value);
        }

        public void AddByte(byte value)
        {
            list.Add(value);
        }

        public void AddInt16(Int16 value)
        {
            list.AddRange(BitConverter.GetBytes(value));
        }

        public void AddUInt16(UInt16 value)
        {
            list.AddRange(BitConverter.GetBytes(value));
        }

        public void AddInt32(Int32 value)
        {
            list.AddRange(BitConverter.GetBytes(value));
        }

        public void AddUInt32(UInt32 value)
        {
            list.AddRange(BitConverter.GetBytes(value));
        }

        public void AddInt64(Int64 value)
        {
            list.AddRange(BitConverter.GetBytes(value));
        }

        public void AddUInt64(UInt64 value)
        {
            list.AddRange(BitConverter.GetBytes(value));
        }

        public void AddSingle(Single value)
        {
            list.AddRange(BitConverter.GetBytes(value));
        }

        public void AddDouble(Double value)
        {
            list.AddRange(BitConverter.GetBytes(value));
        }

        public void AddBytes(byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
            {
                list.AddRange(bytes);
            }
        }

        public void AddObject(object value)
        {
            if (value is bool)
            {
                AddBool((bool)value);
            }
            else if (value is byte)
            {
                AddByte((byte)value);
            }
            else if (value is char)
            {
                AddChar((char)value);
            }
            else if (value is Int16)
            {
                AddInt16((Int16)value);
            }
            else if (value is UInt16)
            {
                AddUInt16((UInt16)value);
            }
            else if (value is Int32)
            {
                AddInt32((Int32)value);
            }
            else if (value is UInt32)
            {
                AddUInt32((UInt32)value);
            }
            else if (value is Int32)
            {
                AddInt64((Int64)value);
            }
            else if (value is UInt64)
            {
                AddUInt64((UInt64)value);
            }
            else if (value is Single)
            {
                AddSingle((Single)value);
            }
            else if (value is Double)
            {
                AddDouble((Double)value);
            }
            else if (value is string)
            {
                AddSizeString((string)value);
            }
        }

        /**
         * バイト配列のサイズと本体を書き込む
         */
        public void AddSizeBytes(byte[] bytes)
        {
            AddUInt32((UInt32)bytes.Length);

            AddBytes(bytes);
        }

        public void AddString(string value)
        {
            list.AddRange(encoding.GetBytes(value));
        }

        public void AddSizeString(string value)
        {
            byte[] bytes = encoding.GetBytes(value);

            AddUInt32((UInt32)bytes.Length);
            AddBytes(bytes);
        }
        #endregion

        #region Get
        public bool GetBool()
        {
            byte value = list[offset];
            offset++;
            return value == 0 ? false : true;
        }
        public byte GetByte()
        {
            byte value = list[offset];
            offset++;
            return value;
        }

        public char GetChar()
        {
            byte value = list[offset];
            offset++;
            return (char)value;
        }

        public Int16 GetInt16()
        {
            byte[] bytes = list.GetRange(offset, 2).ToArray();
            offset += sizeof(Int16);
            return BitConverter.ToInt16(bytes, 0);
        }

        public UInt16 GetUInt16()
        {
            byte[] bytes = list.GetRange(offset, 2).ToArray();
            offset += sizeof(UInt16);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public Int32 GetInt32()
        {
            byte[] bytes = list.GetRange(offset, 4).ToArray();
            offset += sizeof(Int32);
            return BitConverter.ToInt32(bytes, 0);
        }

        public UInt32 GetUInt32()
        {
            byte[] bytes = list.GetRange(offset, 4).ToArray();
            offset += sizeof(UInt32);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public Int64 GetInt64()
        {
            byte[] bytes = list.GetRange(offset, 8).ToArray();
            offset += sizeof(Int64);
            return BitConverter.ToInt64(bytes, 0);
        }

        public UInt64 GetUInt64()
        {
            byte[] bytes = list.GetRange(offset, 8).ToArray();
            offset += sizeof(UInt64);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public Single GetSingle()
        {
            byte[] bytes = list.GetRange(offset, 4).ToArray();
            offset += sizeof(Single);
            return BitConverter.ToSingle(bytes, 0);
        }

        public Double GetDouble()
        {
            byte[] bytes = list.GetRange(offset, 8).ToArray();
            offset += sizeof(Double);
            return BitConverter.ToDouble(bytes, 0);
        }

        public byte[] GetBytes(int size)
        {
            byte[] bytes = list.GetRange(offset, size).ToArray();
            offset += size;
            return bytes;
        }

        /**
         * 文字列のサイズと文字列本体を読み込む
         */
        public string GetSizeString()
        {
            // size
            Int32 size = GetInt32();

            // string
            if (size > 0)
            {
                return null;
            }
            return GetString(size);
        }

        public string GetString(int size)
        {
            // string
            byte[] bytes = GetBytes(size);
            
            return encoding.GetString(bytes);
        }


        #endregion


    }
}
