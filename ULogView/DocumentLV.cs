using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ULogView
{
    /**
     * LogViewオブジェクトをフォームに表示するためのクラス
     * 
     * Formのイベントを受け取りLogViewに渡す。
     * LogViewの情報を参照して画面に描画を行う。
     */
    public class DocumentLV
    {
        //
        // Consts
        // 

        // 
        // Properties
        //
        public LogView logview;
        private TreeView areaTree;

        public TreeView AreaTree
        {
            get { return areaTree; }
            set { areaTree = value; }
        }

        private CheckedListBox idListBox;
            
        public CheckedListBox IdListBox
        {
            get { return idListBox; }
            set { idListBox = value; }
        }



        // 
        // Constructor
        //
        public DocumentLV(TreeView areaTree, CheckedListBox idListBox, int width, int height, InvalidateForm invalidate1)
        {
            this.areaTree = areaTree;
            this.idListBox = idListBox;

            logview = new LogView(width, height, invalidate1);
        }

        //
        // Methods
        // 
        public bool ReadLogFile(string filePath)
        {
            return logview.ReadLogFile(filePath, areaTree, idListBox);
        }

        public void Resize(int width, int height, HScrollBar hSB, VScrollBar vSB)
        {
            logview.Resize(width, height, hSB, vSB);
        }

        #region Draw
        public void Draw(Graphics g)
        {
            logview.Draw(g);
        }

        #endregion

        #region UI

        public bool Click()
        {
            return true;
        }

        public bool Wheel(int direction)
        {
            return true;
        }

        public bool CursorMove(Point p)
        {
            return true;
        }

        //
        // AreaTreeView
        //
        public bool SelectAreaTreeNode(LogArea logArea)
        {
            return logview.SelectArea(logArea);
        }
        #endregion


    }
}
