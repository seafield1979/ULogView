using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ULogView.Utility
{
    /**
     * FileStreamを自前拡張したクラス
     *
     * FileStreamはバイナリファイルを読み込むことができるが、Byte単位でしか値を読みだせないので不便
     * Int型やDouble型のデータを読み込んだり、オフセット位置を覚えておいてくれるように拡張する。
     */
    class UFileStream : IDisposable
    {
        //
        // Properties
        //
        FileStream fs;

        private Encoding encoding;

        public Encoding EncodingType
        {
            get { return encoding; }
            set { encoding = value; }
        }

        // Byteを読み込む先のバッファ。一番大きい Int64, Doubleが取得できる容量
        private byte[] buf = new byte[8]; 

        //
        // Constructor
        // 
        // using (var fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
        public UFileStream(string filePath, FileMode mode, FileAccess access)
        {
            encoding = Encoding.UTF8;
            fs = new FileStream(filePath, mode, access);
        }

        // 
        // Methods
        //
        #region IDisposable

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                fs.Dispose();
            }

            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        /**
         * ファイルポインタが末尾かどうかを返す
         * 
         * @output  true:末尾 / false:末尾でない
         */
        public bool EndOfStream()
        {
            if (fs.Length <= fs.Position)
            {
                return true;
            }
            return false;
        }

        #region Write

        public void WriteBool(bool value)
        {
            fs.WriteByte((byte)(value ? 1 : 0));
        }

        public void WriteChar(char value)
        {
            fs.WriteByte((byte)value);
        }

        public void WriteByte(byte value)
        {
            fs.WriteByte(value);
        }

        public void WriteInt16(Int16 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            fs.Write(bytes, 0, sizeof(Int16));
        }

        public void WriteUInt16(UInt16 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            fs.Write(bytes, 0, sizeof(UInt16));
        }

        public void WriteInt32(Int32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            fs.Write(bytes, 0, sizeof(Int32));
        }

        public void WriteUInt32(UInt32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            fs.Write(bytes, 0, sizeof(UInt32));
        }

        public void WriteInt64(Int64 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            fs.Write(bytes, 0, sizeof(Int64));
        }

        public void WriteUInt64(UInt64 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            fs.Write(bytes, 0, sizeof(UInt64));
        }

        public void WriteFloat(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            fs.Write(bytes, 0, sizeof(float));
        }

        public void WriteDouble(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            fs.Write(bytes, 0, sizeof(double));
        }

        public void WriteBytes(byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
            {
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        /**
         * バイト配列のサイズと本体を書き込む
         */
        public void WriteSizeBytes(byte[] bytes)
        {
            WriteUInt32((UInt32)bytes.Length);

            WriteBytes(bytes);
        }

        public void WriteString(string value)
        {
            byte[] bytes = encoding.GetBytes(value);
            fs.Write(bytes, 0, bytes.Length);
        }

        public void WriteSizeString(string value, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(value);

            WriteUInt32((UInt32)bytes.Length);

            fs.Write(bytes, 0, bytes.Length);
        }

        #endregion

        #region Read

        public bool GetBool()
        {
            fs.Read(buf, 0, 1);
            return buf[0] == 0 ? false : true;
        }
        public byte GetByte()
        {
            fs.Read(buf, 0, 1);
            return buf[0];
        }

        public char GetChar()
        {
            fs.Read(buf, 0, 1);
            return (char)buf[0];
        }

        public Int16 GetInt16()
        {
            fs.Read(buf, 0, sizeof(Int16));
            return BitConverter.ToInt16(buf, 0);
        }

        public UInt16 GetUInt16()
        {
            fs.Read(buf, 0, sizeof(UInt16));
            return BitConverter.ToUInt16(buf, 0);
        }

        public Int32 GetInt32()
        {
            fs.Read(buf, 0, sizeof(Int32));
            return BitConverter.ToInt32(buf, 0);
        }

        public UInt32 GetUInt32()
        {
            fs.Read(buf, 0, sizeof(UInt32));
            return BitConverter.ToUInt32(buf, 0);
        }

        public Int64 GetInt64()
        {
            fs.Read(buf, 0, sizeof(Int64));
            return BitConverter.ToInt64(buf, 0);
        }

        public UInt64 GetUInt64()
        {
            fs.Read(buf, 0, sizeof(UInt64));
            return BitConverter.ToUInt64(buf, 0);
        }

        public float GetSingle()
        {
            fs.Read(buf, 0, sizeof(float));
            return BitConverter.ToSingle(buf, 0);
        }

        public double GetDouble()
        {
            fs.Read(buf, 0, sizeof(double));
            return BitConverter.ToDouble(buf, 0);
        }

        public byte[] GetBytes(int size)
        {
            byte[] bytes = new byte[size];

            fs.Read(bytes, 0, size);
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
            byte[] _buf = new byte[size];
            fs.Read(_buf, 0, size);

            return encoding.GetString(_buf);
        }

        public string GetString(int size)
        {
            // string
            byte[] _buf = new byte[size];
            fs.Read(_buf, 0, size);
            return encoding.GetString(_buf);
        }

        #endregion

    }
}
