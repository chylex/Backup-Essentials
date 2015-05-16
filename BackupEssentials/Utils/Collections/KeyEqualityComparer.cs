using System.Collections.Generic;

namespace BackupEssentials.Utils.Collections{
    class KeyEqualityComparer<K,V> : IEqualityComparer<KeyValuePair<K,V>>{
        bool IEqualityComparer<KeyValuePair<K,V>>.Equals(KeyValuePair<K,V> kvp1, KeyValuePair<K,V> kvp2){
            return kvp1.Key.Equals(kvp2.Key);
        }

        int IEqualityComparer<KeyValuePair<K,V>>.GetHashCode(KeyValuePair<K,V> kvp){
            return kvp.Key.GetHashCode();
        }
    }
}
