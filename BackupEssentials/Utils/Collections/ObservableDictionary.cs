using System.Collections.Generic;
using System.ComponentModel;

namespace BackupEssentials.Utils.Collections{
    class ObservableDictionary<TKey,TValue> : Dictionary<TKey,TValue>, INotifyPropertyChanged{
        public event PropertyChangedEventHandler PropertyChanged;
        public bool PauseObservation = false;

        public new TValue this[TKey key] {
            get { return base[key]; }
            set { base[key] = value; OnChanged(key); }
        }

        public new void Add(TKey key, TValue value){
            base.Add(key,value);
            OnChanged(key);
        }

        public void Add(KeyValuePair<TKey,TValue> item){
            base.Add(item.Key,item.Value);
            OnChanged(item.Key);
        }

        public new bool Remove(TKey key){
            bool val = base.Remove(key);
            if (val)OnChanged(key);
            return val;
        }

        private void OnChanged(TKey key){
            if (PropertyChanged != null && !PauseObservation)PropertyChanged(this,new PropertyChangedEventArgs(key.ToString()));
        }
    }
}
