using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BackupEssentials{
    static class AppPageManager{
        private static Dictionary<Type,Page> cached = new Dictionary<Type,Page>();

        public static T GetPage<T>() where T : Page, new(){
            if (cached.ContainsKey(typeof(T)))return (T)cached[typeof(T)];

            T page = new T();
            cached[typeof(T)] = page;
            return page;
        }

        public static Page GetPage(Type type){
            if (cached.ContainsKey(type))return cached[type];

            Page page = (Page)Activator.CreateInstance(type);
            cached[type] = page;
            return page;
        }

        public static void ResetCache(){
            cached.Clear();
        }
    }
}
