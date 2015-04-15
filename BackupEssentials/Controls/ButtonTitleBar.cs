using System.Windows;
using System.Windows.Controls;

namespace BackupEssentials.Controls{
    public class ButtonTitleBar:Button{
        public static DependencyProperty PathDataProperty = DependencyProperty.Register("CXPathData",typeof(string),typeof(ButtonTitleBar));
        public static DependencyProperty PathWidthProperty = DependencyProperty.Register("CXPathWidth",typeof(string),typeof(ButtonTitleBar));
        public static DependencyProperty PathHeightProperty = DependencyProperty.Register("CXPathHeight",typeof(string),typeof(ButtonTitleBar));
        public static DependencyProperty PathMarginProperty = DependencyProperty.Register("CXPathMargin",typeof(string),typeof(ButtonTitleBar));
        public static DependencyProperty PathRotationProperty = DependencyProperty.Register("CXPathRotation",typeof(double),typeof(ButtonTitleBar));

        public string CXPathData{
            get { return (string)base.GetValue(PathDataProperty); }
            set { base.SetValue(PathDataProperty,(string)value); }
        }

        public string CXPathWidth{
            get { return (string)base.GetValue(PathWidthProperty); }
            set { base.SetValue(PathWidthProperty,(string)value); }
        }

        public string CXPathHeight{
            get { return (string)base.GetValue(PathHeightProperty); }
            set { base.SetValue(PathHeightProperty,(string)value); }
        }

        public string CXPathMargin{
            get { return (string)base.GetValue(PathMarginProperty); }
            set { base.SetValue(PathMarginProperty,(string)value); }
        }

        public double CXPathRotation{
            get { return (double)base.GetValue(PathRotationProperty); }
            set { base.SetValue(PathRotationProperty,(double)value); }
        }
    }
}
