using System;
using System.IO;
using UnityEngine;

namespace Aspekt.Hex.Upgrades
{
    [Serializable]
    public class UpgradeEditorData
    {
        public TechTreeData techTreeData;
        public string currentPage;
        
        private const string FilePath = "upgradeEditorData.json";

        public static UpgradeEditorData Load()
        {
            try
            {
                var data = File.ReadAllText(GetPath());
                return JsonUtility.FromJson<UpgradeEditorData>(data);
            }
            catch
            {
                Debug.LogError($"Failed to read from {GetPath()}");
                return new UpgradeEditorData();
            }
        }

        public static void Save(UpgradeEditorData data)
        {
            data ??= new UpgradeEditorData();
            
            var json = JsonUtility.ToJson(data);
            try
            {
                File.WriteAllText(GetPath(), json);
            }
            catch
            {
                Debug.LogError($"Failed to write to {GetPath()}");
            }
        }

        private static string GetPath()
        {
            return Path.Combine(Application.persistentDataPath, FilePath);
        }
    }
}