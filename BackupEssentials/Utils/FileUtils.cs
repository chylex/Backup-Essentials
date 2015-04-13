﻿using System;
using System.Diagnostics;
using System.IO;

namespace BackupEssentials.Utils{
    static class FileUtils{
        public static bool ReadFile(string filename, FileMode mode, Action<string> lineAction){
            try{
                using(FileStream fileStream = new FileStream(filename,mode)){
                    using(StreamReader reader = new StreamReader(fileStream)){
                        string line;

                        while((line = reader.ReadLine()) != null){
                            lineAction.Invoke(line);
                        }
                    }
                }

                return true;
            }catch(Exception e){
                Debug.WriteLine(e.ToString());
                return false;
            }
        }

        public static bool WriteFile(string filename, FileMode mode, Action<StreamWriter> writeAction){
            try{
                using(FileStream fileStream = new FileStream(filename,mode)){
                    using(StreamWriter writer = new StreamWriter(fileStream)){
                        writeAction.Invoke(writer);
                    }
                }

                return true;
            }catch(Exception e){
                Debug.WriteLine(e.ToString());
                return false;
            }
        }
    }
}