using SqliteHelper.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Configuration;
using SqliteHelper.DataModel;
using System.IO;

namespace SqliteHelper
{
    class Program
    {
        private static SQLiteHelper _sqliteHelper;

        private static List<String> _imgFullPathSet;

        static void Main(string[] args)
        {
            updateDoc();
            //insertBlob();
        }

        private static void updateDoc()
        {
            Console.WriteLine("开始更新");

            Console.WriteLine("----------------------------------");

            //Console.ReadKey();

            try
            {
                string markdownPath = ConfigurationManager.AppSettings["markdownPath"];

                string dbSource = ConfigurationManager.AppSettings["dbSource"];

                ContentProvider contentPrvd = new ContentProvider();

                List<UpdateMeta> contentUpdateMetaSet = contentPrvd.Acquire(markdownPath);

                using (SQLiteConnection conn = new SQLiteConnection(dbSource))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand())
                    {
                        cmd.Connection = conn;

                        conn.Open();

                        _sqliteHelper = new SQLiteHelper(cmd);

                        int count = 0;

                        foreach (var updateMeta in contentUpdateMetaSet)
                        {
                            _sqliteHelper.Update("BODY", updateMeta.updates, updateMeta.limits);

                            count++;

                            if (count % 10 == 0)
                            {
                                Console.WriteLine("----------------------------------");

                                Console.WriteLine(string.Format("已更新 {0} 条记录", count));
                            }
                        }
                        conn.Close();
                    }
                }

                Console.WriteLine("----------------------------------");
                Console.WriteLine("结束更新");

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("error: {0}", ex));
                Console.ReadKey();
                //throw;
            }
        }

        private static void insertBlob()
        {
            Console.WriteLine("开始导入");

            Console.WriteLine("----------------------------------");

            //Console.ReadKey();

            try
            {
                string tilesPath = ConfigurationManager.AppSettings["tilesPath"];

                string dbSource = ConfigurationManager.AppSettings["mbtilesSource"];

                _imgFullPathSet = new List<string>();

                ListFilesFullPath(new DirectoryInfo(tilesPath));

                var dic = new Dictionary<string, object>();

                string[] splits;

                FileStream fs;

                byte[] bytes;

                Console.WriteLine(string.Format("将导入 {0} 张图片", _imgFullPathSet.Count));

                using (SQLiteConnection conn = new SQLiteConnection(dbSource))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand())
                    {
                        cmd.Connection = conn;

                        conn.Open();

                        _sqliteHelper = new SQLiteHelper(cmd);

                        int count = 0;

                        foreach (string fullPath in _imgFullPathSet)
                        {
                            fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

                            bytes = new byte[fs.Length];

                            fs.Read(bytes, 0, (int)fs.Length);

                            splits = fullPath.Split('\\');

                            dic["zoom_level"] = splits[splits.Length - 3];
                            dic["tile_row"] = splits[splits.Length - 2];
                            dic["tile_column"] = splits[splits.Length - 1].Replace(".png", "");
                            dic["tile_data"] = bytes;

                            _sqliteHelper.Insert("tiles", dic);

                            count++;

                            if (count % 10 == 0)
                            {
                                Console.WriteLine("----------------------------------");

                                Console.WriteLine(string.Format("已导入 {0} 张", count));
                            }
                        }

                        conn.Close();
                    }
                }

                Console.WriteLine("----------------------------------");

                Console.WriteLine("结束导入");

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("error: {0}", ex));
                Console.ReadKey();
            }
        }

        public static void ListFilesFullPath(FileSystemInfo info)
        {
            if (!info.Exists) return;

            DirectoryInfo direc = info as DirectoryInfo;

            if (direc == null) return;

            FileSystemInfo[] files = direc.GetFileSystemInfos();

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i] as FileInfo;

                if (file != null)
                {
                    _imgFullPathSet.Add(file.FullName);
                }
                else
                {
                    ListFilesFullPath(files[i]);
                }
            }
        }
    }
}
