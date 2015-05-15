using BackupEssentials.Backup;
using BackupEssentials.Backup.Data;
using BackupEssentials.Controls;
using BackupEssentials.Pages;
using BackupEssentials.Utils;
using System;
using System.Diagnostics;
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

        private object DropData = null;

        public MainWindow() : this(null,null){}
        public MainWindow(SplashScreen splashScreen) : this(splashScreen,null){}
        public MainWindow(Action<MainWindow> runOnLoad) : this(null,runOnLoad){}

        public MainWindow(SplashScreen splashScreen, Action<MainWindow> runOnLoad){
            InitializeComponent();
            Instance = this;
            
            ContentFrame.Navigated += (sender2, args2) => { ContentFrame.NavigationService.RemoveBackEntry(); };

            Loaded += (sender, args) => {
                DataStorage.SetupForSaving(true);
                DataStorage.Load();
                if (runOnLoad != null)runOnLoad(this);
                if (splashScreen != null)splashScreen.Close(new TimeSpan());
            };

            Closing += (sender, args) => {
                IPageSwitchHandler switchHandler = ContentFrame.Content as IPageSwitchHandler;
                if (switchHandler != null && switchHandler.OnSwitch())args.Cancel = true;
            };

            Closed += (sender, args) => {
                DataStorage.Save(true);
                ExplorerIntegration.Refresh(true);
            };
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

            if (!ShowPage(GetType().Assembly.GetType("BackupEssentials."+btn.ClickPage,false)))return;
            
            for(int child = 0; child < VisualTreeHelper.GetChildrenCount(btn.Parent); child++){
                ButtonMainMenu childBtn = VisualTreeHelper.GetChild(btn.Parent,child) as ButtonMainMenu;
                if (childBtn != null && childBtn != btn)childBtn.IsChecked = false;
            }

            btn.IsChecked = true;
        }

        private void OnDragEnter(object sender, DragEventArgs e){
            IPageDragDrop dragDropOverride = ContentFrame.Content as IPageDragDrop;

            if (DropData == null && dragDropOverride != null){
                DropData = dragDropOverride.DragEnter(e);
                e.Handled = true;
                return;
            }

            if (DropData == null && e.Data.GetDataPresent(DataFormats.FileDrop)){
                DropData = e.Data.GetData(DataFormats.FileDrop) as string[];
                DropOverlayLabel.Visibility = Visibility.Visible;
                NativeMethods.SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
            }
            else e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void OnDragLeave(object sender, DragEventArgs e){
            IPageDragDrop dragDropOverride = ContentFrame.Content as IPageDragDrop;
            if (dragDropOverride != null)dragDropOverride.DragExit(e);

            DropData = null;
            DropOverlayLabel.Visibility = Visibility.Hidden;
        }

        private void OnDragDrop(object sender, DragEventArgs e){
            if (DropData != null){
                IPageDragDrop dragDropOverride = ContentFrame.Content as IPageDragDrop;

                if (dragDropOverride != null)dragDropOverride.DragDrop(e,DropData);
                else{
                    DropOverlayLabel.Visibility = Visibility.Hidden;
                    ShowPage(typeof(BackupDrop),new object[]{ DropData, ContentFrame.Content == null ? null : (ContentFrame.Content as Page).GetType() });
                }

                DropData = null;
            }
        }

        public bool ShowPage(Type pageType){
            return ShowPage(pageType,null);
        }

        public bool ShowPage(Type pageType, object data){
            IPageSwitchHandler switchHandler = ContentFrame.Content as IPageSwitchHandler;
            if (switchHandler != null && switchHandler.OnSwitch())return false;

            Page page = null;
            ContentFrame.Content = pageType == null ? null : page = AppPageManager.GetPage(pageType);

            IPageShowData pageDataHandler = page as IPageShowData;
            if (pageDataHandler != null && data != IgnoreShowData)pageDataHandler.OnShow(data);

            if (page != null)SetupPageDropEvents(page);
            return true;
        }

        public void SetupPageDropEvents(Page page){
            if (!page.AllowDrop){
                page.AllowDrop = true;
                page.DragEnter += new DragEventHandler(OnDragEnter);
                page.DragLeave += new DragEventHandler(OnDragLeave);
                page.Drop += new DragEventHandler(OnDragDrop);
            }
        }
    }
}
