using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aspekt.Hex.Util
{
    public class AssetBundleUtil
    {
        public const string DataBundleName = "data";
        
        public static readonly string AssetBundlesPath = Path.Combine(Application.streamingAssetsPath, "Bundles");

#region API

        public static T LoadAsset<T>(string bundleName, string assetPath, string fileName) where T : Object
        {
#if UNITY_EDITOR
            if (TestHandler.Instance.loadAssetsFromBundles)
            {
                return LoadFromAssetBundle<T>(bundleName, fileName);
            }
            else
            {
                return LoadFromEditorFolder<T>(assetPath, fileName);
            }
#else
                return LoadFromAssetBundle<T>(bundleName, fileName);
#endif
        }   
        
        public static T[] LoadAll<T>(string bundleName, string assetPath) where T : Object
        {
#if UNITY_EDITOR
            return TestHandler.Instance.loadAssetsFromBundles
                ? LoadAllFromAssetBundle<T>(bundleName)
                : LoadAllFromEditorFolder<T>(assetPath);
#else
            return LoadAllFromAssetBundle<T>(bundleName);
#endif
        }

        public static void LoadAllAsync<T>(string bundleName, string assetPath, Action<T[]> onLoadedCallback) where T : Object
        {
#if UNITY_EDITOR
            if (TestHandler.Instance.loadAssetsFromBundles)
            {
                LoadAllFromAssetBundleAsync(bundleName, onLoadedCallback);
            }
            else
            {
                LoadAllFromEditorFolderAsync(assetPath, onLoadedCallback);
            }
#else
                LoadAllFromAssetBundleAsync(bundleName, onLoadedCallback);
#endif
        }
        
#endregion API


#region Editor Functions

#if UNITY_EDITOR

        private static T LoadFromEditorFolder<T>(string assetPath, string fileName) where T : Object
        {
            string dbPath = Path.Combine("Assets", assetPath, fileName);    
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(dbPath);
        }
        
        private static T[] LoadAllFromEditorFolder<T>(string assetPath) where T : Object
        {
            var assetList = new List<T>();
            var absolutePath = Path.Combine(Application.dataPath, assetPath);
        
            AddAssetsToList(absolutePath, assetList);
            AddAssetsInSubDirectoriesToList(absolutePath, assetList);
            
            return assetList.ToArray();
        }

        private static void AddAssetsInSubDirectoriesToList<T>(string parentPath, List<T> list) where T : UnityEngine.Object
        {
            var subDirectories = Directory.GetDirectories(parentPath);
            foreach (var dir in subDirectories)
            {
                AddAssetsToList(dir, list);
                AddAssetsInSubDirectoriesToList<T>(dir, list);
            }
        }

        private static void AddAssetsToList<T>(string path, List<T> list) where T : Object
        {
            var assetPaths = Directory.GetFiles(path);

            foreach (var p in assetPaths)
            {
                string dbPath = "Assets" + p.Replace(Application.dataPath, "").Replace('\\', '/');    
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(dbPath);
                if (asset != null) list.Add(asset);
            }
        }

        private static void LoadAllFromEditorFolderAsync<T>(string assetPath, Action<T[]> onLoadedCallback) where T : Object
        {
            var absolutePath = Path.Combine(Application.dataPath, assetPath);
            var assetPaths = Directory.GetFiles(absolutePath);

            var assetList = new List<T>();
            foreach (var path in assetPaths)
            {
                string dbPath = "Assets" + path.Replace(Application.dataPath, "").Replace('\\', '/');    
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(dbPath);
                if (asset != null) assetList.Add(asset);
            }
            
            onLoadedCallback.Invoke(assetList.ToArray());
        }
#endif

#endregion Editor Functions


#region AssetBundle Functions

        private static T LoadFromAssetBundle<T>(string bundleName, string fileName) where T : Object
        {
            fileName = RemoveFolderPath(fileName);
            var bundle = GetAssetBundle(bundleName);
            if (fileName.EndsWith(".prefab"))
            {
                var asset = bundle.LoadAsset<GameObject>(fileName);
                return asset.GetComponent<T>();
            }
            else
            {
                return bundle.LoadAsset<T>(fileName);
            }
        }
        
        private static T[] LoadAllFromAssetBundle<T>(string bundleName) where T : Object
        {
            var bundle = GetAssetBundle(bundleName);
            var assets = bundle.LoadAllAssets<T>();
            
            if (assets.Length == 0)
            {
                assets = bundle.LoadAllAssets<GameObject>()
                    .Where(o => o.GetComponent<T>())
                    .Select(t => t.GetComponent<T>())
                    .ToArray();
            }

            return assets;
        }

        private static void LoadAllFromAssetBundleAsync<T>(string bundleName, Action<T[]> onLoadedCallback) where T : Object
        {
            var bundle = GetAssetBundle(bundleName);
            var request = bundle.LoadAllAssetsAsync<T>();
            
            request.completed += (operationHandle) =>
            {
                onLoadedCallback.Invoke(request.allAssets as T[]);
            };
        }

        private static AssetBundle GetAssetBundle(string bundleName)
        {
            var bundles = AssetBundle.GetAllLoadedAssetBundles();
            foreach (var bundle in bundles)
            {
                if (bundle.name == bundleName)
                {
                    return bundle;
                }
            }
            
            var file = Path.Combine(AssetBundlesPath, bundleName);
            return AssetBundle.LoadFromFile(file);
        }

        private static string RemoveFolderPath(string filePath)
        {
            var paths = filePath.Split('/');
            return paths[paths.Length - 1];
        }

#endregion        
    }
}