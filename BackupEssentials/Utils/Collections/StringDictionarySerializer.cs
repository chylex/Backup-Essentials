using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackupEssentials.Utils.Collections{
    static class StringDictionarySerializer{
        public static string ToString(Dictionary<string,string> dict){
            StringBuilder build = new StringBuilder();

            foreach(KeyValuePair<string,string> kvp in dict){
                build.Append(kvp.Key).Append((char)31).Append(kvp.Value).Append((char)31);
            }

            if (build.Length > 0)build.Remove(build.Length-1,1);
            return build.ToString();
        }
        
        public static string ToString(IObjectToDictionary obj){
            SafeDictionary<string,string> dict = new SafeDictionary<string,string>();
            obj.ToDictionary(dict);
            return ToString(dict);
        }

        public static SafeDictionary<string,string> FromString(string data){
            SafeDictionary<string,string> dict = new SafeDictionary<string,string>();
            string key = null;

            foreach(string part in data.Split((char)31)){
                if (key == null)key = part;
                else{
                    dict.Add(key,part);
                    key = null;
                }
            }

            return dict;
        }

        public static void FromString(IObjectToDictionary obj, string data){
            obj.FromDictionary(FromString(data));
        }

        public interface IObjectToDictionary{
            void ToDictionary(SafeDictionary<string,string> dict);
            void FromDictionary(SafeDictionary<string,string> dict);
        }
    }
}
