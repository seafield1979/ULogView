using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ULogView
{
    /*
     * メモリ展開されたアイコン画像
     */
    class MemIconImage
    {
        //
        // Properties
        //

        // 画像名
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        // 画像
        private Image image;

        public Image Image
        {
            get { return image; }
            set { image = value; }
        }

        //
        // Constructor
        //
        public MemIconImage()
        {
            this.name = null;
            this.image = null;
        }
        public MemIconImage(string name, Image image)
        {
            this.name = name;
            this.image = image;
        }

        //
        // Methods
        //

        /**
         * バイト配列から画像を取得、設定
         */
        public void SetByteImage(byte[] byteImage)
        {
            this.image = MemIconImage.ByteArrayToImage(byteImage);
        }

        // バイト配列をImageオブジェクトに変換
        public static Image ByteArrayToImage(byte[] byteImage)
        {
            try
            {
                ImageConverter imgconv = new ImageConverter();
                Image img = (Image)imgconv.ConvertFrom(byteImage);
                return img;
            }
            catch
            {
                Console.WriteLine("{0} Imageの作成に失敗しました。");
            }
            return null;
        }

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"name:""{0}""", name);
            if (image != null)
            {
                sb.AppendFormat(@",imageSize:{0}", image.Size);
            }

            return sb.ToString();
        }
    }

    class MemIconImages
    {
        //
        // Properties
        // 
        Dictionary<string, MemIconImage> images;

        //
        // Constructor
        // 
        public MemIconImages()
        {
            images = new Dictionary<string, MemIconImage>();
        }

        // 
        // Methods
        //
        public void Add(MemIconImage image)
        {
            if (image != null && image.Name != null)
            {
                images[image.Name] = image;
            }
        }

        public Image GetImage(string name)
        {
            if (images.ContainsKey(name))
            {
                return images[name].Image;
            }
            return null;
        }

        /**
         * 文字列に変換 for Debug
         */
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<images>");
            foreach( MemIconImage image in images.Values)
            {
                sb.AppendFormat("\t{0}\r\n", image.ToString());
            }
            sb.AppendLine("</images>");

            return sb.ToString();
        }
    }
}
