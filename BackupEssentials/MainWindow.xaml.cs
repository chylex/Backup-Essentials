using BackupEssentials.Backup;
using BackupEssentials.Controls;
using BackupEssentials.Pages;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BackupEssentials{
    public partial class MainWindow : Window{
        public static MainWindow Instance { get; private set; }

        /// <summary>
        /// Use this to not call OnShow(data) when changing the page. Only use when showing 'overlay' windows that do not modify any data.
        /// </summary>
        public static readonly object IgnoreShowData = new object();

        private new Rect RestoreBounds = new Rect();
        private bool IsMaximized = false;

        private string[] DropData = null;

        public MainWindow(){
            InitializeComponent();
            Instance = this;

            Loaded += (args, sender) => {
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded,new Action(() => {
                    DataStorage.Load();
                }));
            };

            Closed += (args, sender) => {
                DataStorage.Save(true);
                ExplorerIntegration.Refresh(true);
            };

            ShowPage(typeof(Home));
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

                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)Math.Round(Left+Width/2),(int)Math.Round(Top+Height/2)));
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

        private void OnDragEnter(object sender, DragEventArgs e){
            if (DropData == null && e.Data.GetDataPresent(DataFormats.FileDrop)){
                DropData = e.Data.GetData(DataFormats.FileDrop) as string[];
                DropOverlayLabel.Visibility = Visibility.Visible;
            }
            else e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void OnDragLeave(object sender, DragEventArgs e){
            DropData = null;
            DropOverlayLabel.Visibility = Visibility.Hidden;
        }

        private void OnDragDrop(object sender, DragEventArgs e){
            if (DropData != null){
                ShowPage(typeof(BackupDrop),DropData);
                DropOverlayLabel.Visibility = Visibility.Hidden;
                DropData = null;
            }
        }

        public void ShowPage(Type pageType){
            ShowPage(pageType,null);
        }

        public void ShowPage(Type pageType, object data){
            Page page = null;
            ContentFrame.Navigate(pageType == null ? null : page = AppPageManager.GetPage(pageType));

            IPageShowData pageDataHandler = page as IPageShowData;
            if (pageDataHandler != null && data != IgnoreShowData)pageDataHandler.OnShow(data);

            if (!page.AllowDrop){
                page.AllowDrop = true;
                page.DragEnter += new DragEventHandler(OnDragEnter);
                page.DragLeave += new DragEventHandler(OnDragLeave);
                page.Drop += new DragEventHandler(OnDragDrop);
            }
        }
    }
}
