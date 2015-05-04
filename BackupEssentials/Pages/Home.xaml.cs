using System;
using System.Windows;
using System.Windows.Controls;

namespace BackupEssentials.Pages{
    public partial class Home : Page{
        public Home(){
            InitializeComponent();
            HomePageFrame.Navigated += (sender2, args2) => { HomePageFrame.NavigationService.RemoveBackEntry(); };
            ChangeScreen(BackupGuide,new RoutedEventArgs());
        }

        private void ChangeScreen(object sender, RoutedEventArgs e){
            Type pageType = GetType().Assembly.GetType("BackupEssentials.Pages.HomeSub."+((RadioButton)sender).Name,false);
            Page page = null;
            HomePageFrame.Content = pageType == null ? null : page = AppPageManager.GetPage(pageType);
            if (page != null)MainWindow.Instance.SetupPageDropEvents(page);
        }
    }
}
