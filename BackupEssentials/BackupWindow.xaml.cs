using BackupEssentials.Backup;
using System.Windows;

namespace BackupEssentials{
    public partial class BackupWindow : Window{
        public BackupWindow(BackupRunner runner){
            InitializeComponent();
            runner.Start(TaskbarItemInfo);
        }
    }
}
