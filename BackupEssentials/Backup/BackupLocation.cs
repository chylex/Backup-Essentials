using BackupEssentials.Sys;
using BackupEssentials.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace BackupEssentials.Backup{
    public class BackupLocation : StringDictionarySerializer.IObjectToDictionary{
        private static readonly Regex NameValidation = new Regex(@"[^\P{Cc}]");

        private string _name;

        public string Name {
            get { return _name; }
            set { _name = NameValidation.Replace(value,""); }
        }

        public string Directory { get; set; }

        public Visibility LayoutDirectoryCautionVisibility { get { return GetDirectoryStatus() == DirectoryStatus.Ok ? Visibility.Collapsed : Visibility.Visible; } }

        public string LayoutDirectoryCautionTooltip {
            get {
                switch(GetDirectoryStatus()){
                    case DirectoryStatus.Empty: return Settings.Default.Language["Backup.Location.Caution.Empty"];
                    case DirectoryStatus.Invalid: return Settings.Default.Language["Backup.Location.Caution.Invalid"];
                    case DirectoryStatus.NotAbsolute: return Settings.Default.Language["Backup.Location.Caution.NotAbsolute"];
                    case DirectoryStatus.NotExists: return Settings.Default.Language["Backup.Location.Caution.NotExists"];
                    default: return null;
                }
            }
        }

        public BackupLocation(){
            Name = "";
            Directory = "";
        }

        public DirectoryStatus GetDirectoryStatus(){
            if (Directory.Length == 0)return DirectoryStatus.Empty;

            try{
                Path.GetFullPath(Directory);
            }catch(ArgumentException){
                return DirectoryStatus.Invalid;
            }

            if (!Path.IsPathRooted(Directory))return DirectoryStatus.NotAbsolute;
            if (!System.IO.Directory.Exists(Directory))return DirectoryStatus.NotExists;

            return DirectoryStatus.Ok;
        }

        public bool ShouldRegister(){
            DirectoryStatus status = GetDirectoryStatus();
            return status == DirectoryStatus.Ok || status == DirectoryStatus.NotExists;
        }

        public void Set(BackupLocation newData){
            this.Name = newData.Name;
            this.Directory = newData.Directory;
        }

        public BackupLocation Clone(){
            return new BackupLocation(){ Name = this.Name, Directory = this.Directory };
        }

        void StringDictionarySerializer.IObjectToDictionary.ToDictionary(SafeDictionary<string,string> dict){
            dict["Name"] = Name;
            dict["Dir"] = Directory;
        }

        void StringDictionarySerializer.IObjectToDictionary.FromDictionary(SafeDictionary<string,string> dict){
            Name = dict["Name"] ?? Settings.Default.Language["Backup.Location.UnknownName"];
            Directory = dict["Dir"] ?? Settings.Default.Language["Backup.Location.UnknownDirectory"];
        }

        public enum DirectoryStatus{
            Ok, NotExists, NotAbsolute, Invalid, Empty
        }
    }
}
