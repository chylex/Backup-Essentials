using System.Windows;

namespace BackupEssentials.Pages{
    interface IPageDragDrop{
        object DragEnter(DragEventArgs e);
        void DragExit(DragEventArgs e);
        void DragDrop(DragEventArgs e, object data);
    }
}
