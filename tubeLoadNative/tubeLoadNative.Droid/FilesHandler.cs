using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace tubeLoadNative.Droid
{
    public static class FilesHandler
    {
        private static Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;
        private static readonly string PATH = sdCard.Path + "/TubeLoad/";

        public const string ID_FILE = "ids.json";

        static FilesHandler()
        {
            File.Create(PATH + ID_FILE);
        }

        public static Dictionary<T, K> ReadJsonFile<T, K>(string file)
        {
            Dictionary<T, K> values = new Dictionary<T, K>();

            if (File.Exists(file))
            {
                string json = File.ReadAllText(file, Encoding.UTF8);

                if (json != string.Empty)
                    values = JsonConvert.DeserializeObject<Dictionary<T, K>>(json); 
            }

            return values;
        }

        public static void WriteToJsonFile<T, K>(string fileName, T key, K value)
        {
            Dictionary<T, K> values = new Dictionary<T, K>();
            values.Add(key, value);

            WriteToJsonFile(fileName, values);
        }

        public static void WriteToJsonFile<T, K>(string fileName, Dictionary<T, K> files)
        {
            string filePath = PATH + fileName;

            Dictionary<T, K> values = ReadJsonFile<T, K>(filePath);

            foreach (var file in files)
                values[file.Key] = files[file.Key];


            using (FileStream fs = new FileStream(filePath, FileMode.Truncate, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.WriteAsync(JsonConvert.SerializeObject(values, Formatting.Indented));
                }
            }

            // serialize JSON directly to a file
            //using (StreamWriter sw = File.CreateText(filePath))
            //{
            //    JsonSerializer serializer = new JsonSerializer();
            //    serializer.Serialize(sw, files);
            //}
        }
    }
}