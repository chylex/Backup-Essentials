using BackupEssentials.Utils.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace BackupEssentials.Data{
    public class Language{
        private readonly Dictionary<string,string> Data;
        
        public string File { get; private set; }
        public string LangName { get; private set; }

        /// <summary>
        /// Returns translated text using language key. If there is no text assigned, the key itself is returned.
        /// </summary>
        public string this[string key] {
            get {
                if (Data.Count == 0)LoadLanguage();

                string res;
                return Data.TryGetValue(key,out res) ? res : key;
            }
        }

        /// <summary>
        /// Returns translated text using language key. If there is no text assigned, the key itself is returned.
        /// This override allows parameters to be passed to the translated text. In the text, $x signifies an element of the data object array.
        /// </summary>
        public string this[string key, params string[] data] {
            get {
                string text = this[key];
                for(int a = data == null ? -1 : data.Length-1; a >= 0; a--)text = text.Replace("$"+a,data[a]);
                return text;
            }
        }

        /// <summary>
        /// Returns translated text using language key with the number parameter. If the exact key with number parameter is not found, the one with an asterisk is returned.
        /// The key should be provided without the number, ending with a dot. The number will be added and checked automatically.
        /// This override allows parameters to be passed to the translated text. In the text, $x signifies an element of the data object array.
        /// </summary>
        public string this[string key, int paramNumber, params string[] data] {
            get {
                if (Data.ContainsKey(key+paramNumber))return this[key+paramNumber,data];
                else return this[key+"*",data];
            }
        }

        public Language(string file, string langName){
            this.File = file;
            this.LangName = langName;
            this.Data = new Dictionary<string,string>();
        }

        private void LoadLanguage(){
            FileUtils.ReadFile(Path.Combine("Resources/Lang",File),FileMode.Open,(line) => {
                if (line.Length == 0 || line[0] == '#')return;

                string[] data = line.Split(new string[]{ " = " },2,StringSplitOptions.None);
                if (data.Length == 2)Data.Add(data[0],data[1]);
            });
        }
    }
}
