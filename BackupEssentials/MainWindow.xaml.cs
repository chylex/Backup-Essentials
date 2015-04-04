using BackupEssentials.Controls;
using BackupEssentials.Pages;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace BackupEssentials{
    public partial class MainWindow : Window{
        public static MainWindow Instance { get; private set; }

        private new Rect RestoreBounds = new Rect();
        private bool IsMaximized = false;

        public MainWindow(){
            InitializeComponent();
            Instance = this;
        }

        private void ButtonWindowCloseClick(object sender, RoutedEventArgs e){
            Close();
        }

        private void ButtonWindowToggleClick(object sender, RoutedEventArgs e){
            if (IsMaximized){
                Left = RestoreBounds.X;
                Top = RestoreBounds.Y;
                Width = RestoreBounds.Width;
                Height = RestoreBounds.Height;
                
                ResizeMode = ResizeMode.CanResizeWithGrip;
                IsMaximized = false;
                ButtonWindowToggle.CXPathData = (string)FindResource("PathButtonMaximized");
            }
            else{
                RestoreBounds.X = Left;
                RestoreBounds.Y = Top;
                RestoreBounds.Width = Width;
                RestoreBounds.Height = Height;

                Screen screen = Screen.FromPoint(new System.Drawing.Point((int)Math.Round(Left+Width/2),(int)Math.Round(Top+Height/2)));
                Left = screen.WorkingArea.X;
                Top = screen.WorkingArea.Y;
                Width = screen.WorkingArea.Width;
                Height = screen.WorkingArea.Height;
                
                ResizeMode = ResizeMode.CanResize;
                IsMaximized = true;
                ButtonWindowToggle.CXPathData = (string)FindResource("PathButtonWindowed");
            }

            VisualStateManager.GoToState(ButtonWindowToggle,"Normal",false);
        }

        private void ButtonWindowMinimizeClick(object sender, RoutedEventArgs e){
            WindowState = WindowState.Minimized;
        }

        private void TitleBarLeftButtonDown(object sender, MouseButtonEventArgs e){
            if (e.ClickCount == 2)ButtonWindowToggleClick(sender,e);
            else if (!IsMaximized)DragMove();
        }

        private void ButtonMainMenuClick(object sender, RoutedEventArgs e){
            ButtonMainMenu btn = (ButtonMainMenu)sender;
            
            for(int child = 0; child < VisualTreeHelper.GetChildrenCount(btn.Parent); child++){
                ButtonMainMenu childBtn = VisualTreeHelper.GetChild(btn.Parent,child) as ButtonMainMenu;
                if (childBtn != null && childBtn != btn)childBtn.IsChecked = false;
            }

            btn.IsChecked = true;
            ShowPage(GetType().Assembly.GetType("BackupEssentials."+btn.ClickPage,false));
        }

        public void ShowPage(Type pageType){
            ShowPage(pageType,null);
        }

        public void ShowPage(Type pageType, object data){
            Page page = null;
            ContentFrame.Navigate(pageType == null ? null : page = AppPageManager.GetPage(pageType));
            if (page is IPageShowData)((IPageShowData)page).OnShow(data);
        }
    }
}
