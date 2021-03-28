using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Mahou
{
    public static class SaveLoadJsonService
    {

        public static bool Save(string fileName, string jsonObject)
        {
            try
            {
                using (StreamWriter streamWriter = File.CreateText(Path.Combine(Application.persistentDataPath, fileName)))
                {
                    streamWriter.Write(jsonObject);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Exception thrown while saving {fileName}. {e.Message}");
                return false;
            }
            return true;
        }

        public static bool Save<T>(string fileName, T obj, bool prettyPrint = false)
        {
            string jsonObject = JsonUtility.ToJson(obj, prettyPrint);
            try
            {
                using (StreamWriter streamWriter = File.CreateText(Path.Combine(Directory.GetCurrentDirectory(), fileName)))
                {
                    streamWriter.Write(jsonObject);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception thrown while saving {fileName}. {e.Message}");
                return false;
            }
            return true;
        }

        public static T Load<T>(string path)
        {
            try
            {
                string p = Path.Combine(Application.persistentDataPath, path);
                if (File.Exists(p))
                {
                    string jsonString = null;
                    using (StreamReader streamReader = File.OpenText(p))
                    {
                        jsonString = streamReader.ReadToEnd();
                    }
                    return JsonUtility.FromJson<T>(jsonString);
                }

            }
            catch (Exception e)
            {
                Debug.Log($"Exception thrown while loading {path}. {e.Message}");
            }
            return default(T);
        }
    }
}
