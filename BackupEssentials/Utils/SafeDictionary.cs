using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackupEssentials.Utils{
    class SafeDictionary<K,V> : Dictionary<K,V>{
        public new V this[K key]{
            get {
                V value;
                TryGetValue(key,out value);
                return value;
            }

            set {
                base[key] = value;
            }
        }
    }
}
