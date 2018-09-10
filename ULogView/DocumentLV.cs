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
        public DocumentLV(int width, int height,
            TreeView areaTree, CheckedListBox idListBox, HScrollBar hScrollBar, VScrollBar vScrollBar,
            InvalidateForm invalidate1)
        {
            this.areaTree = areaTree;
            this.idListBox = idListBox;

            logview = new LogView(width, height, 0, invalidate1, hScrollBar, vScrollBar);
        }

        //
        // Methods
        // 
        public bool ReadLogFile(string filePath)
        {
            return logview.ReadLogFile(filePath, areaTree, idListBox);
        }

        public void Resize(int width, int height)
        {
            logview.Resize(width, height);
        }

        public void ScrollV(int value)
        {
            logview.ScrollV(value);
        }

        public void ScrollH(int value)
        {
            logview.ScrollH(value);
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
            logview.SetLogArea(logArea);
            return true;
        }
        #endregion


    }
}
