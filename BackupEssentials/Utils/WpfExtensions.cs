using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BackupEssentials.Utils{
    static class WpfExtensions{
        public static IEnumerable<DependencyObject> EnumerateMeAndChildren(this DependencyObject obj){
            yield return obj;

            for(int a = 0; a < VisualTreeHelper.GetChildrenCount(obj); a++){
                foreach(DependencyObject child in VisualTreeHelper.GetChild(obj,a).EnumerateMeAndChildren()){
                    yield return child;
                }
            }
        }

        public static void UpdateBindings(this DependencyObject parent){
            foreach(DependencyObject obj in parent.EnumerateMeAndChildren()){
                LocalValueEnumerator enumerator = obj.GetLocalValueEnumerator();

                while(enumerator.MoveNext()){
                    BindingExpressionBase binding = BindingOperations.GetBindingExpressionBase(obj,enumerator.Current.Property);
                    if (binding != null)binding.UpdateTarget();
                }
            }
        }
    }
}
