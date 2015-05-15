using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace BackupEssentials.Utils{
    static class FileUtils{
        public static bool ReadFile(string filename, FileMode mode, Action<string> lineAction){
            if (!File.Exists(filename))return false;

            try{
                using(FileStream fileStream = new FileStream(filename,mode,FileAccess.Read,FileShare.Read)){
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
                using(FileStream fileStream = new FileStream(filename,mode,FileAccess.Write)){
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

        public static string ReadFileCompressed(string filename, FileMode mode){
            if (!File.Exists(filename))return null;

            try{
                string data;

                using(FileStream fileStream = new FileStream(filename,mode,FileAccess.Read,FileShare.Read)){
                    using(GZipStream compressed = new GZipStream(fileStream,CompressionMode.Decompress)){
                        using(StreamReader reader = new StreamReader(compressed)){
                            data = reader.ReadToEnd();
                        }
                    }
                }

                return data;
            }catch(Exception e){
                Debug.WriteLine(e.ToString());
                return null;
            }
        }

        public static bool WriteFileCompressed(string filename, FileMode mode, string data){
            try{
                using(FileStream fileStream = new FileStream(filename,mode,FileAccess.Write)){
                    using(GZipStream compressed = new GZipStream(fileStream,CompressionMode.Compress)){
                        byte[] bytes = Encoding.UTF8.GetBytes(data);
                        compressed.Write(bytes,0,bytes.Length);
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
