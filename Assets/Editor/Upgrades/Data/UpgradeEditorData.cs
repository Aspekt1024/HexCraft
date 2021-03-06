using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Aspekt.Hex.Upgrades
{
    [Serializable]
    public class UpgradeEditorData
    {
        public TechTreeData techTreeData;
        public string currentPage;
        
        private const string FilePath = "Assets/Editor/Upgrades/Data/upgradeEditorData.json";

        public static UpgradeEditorData Load()
        {
            try
            {
                var text = File.ReadAllText(FilePath);
                var data = JsonUtility.FromJson<UpgradeEditorData>(text);
                return data;
            }
            catch
            {
                Debug.LogError($"Failed to read from {FilePath}");
                return new UpgradeEditorData();
            }
        }

        public static void Save(UpgradeEditorData data)
        {
            data ??= new UpgradeEditorData();

            var json = JsonUtility.ToJson(data, true);
            try
            {
                File.WriteAllText(FilePath, json);
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to write to {FilePath}: {e.Message}");
            }
            AssetDatabase.Refresh();
        }
    }
}