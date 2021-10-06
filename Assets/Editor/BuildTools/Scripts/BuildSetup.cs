using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Himeki.Build
{

    [Serializable]
    public class BuildSetup : ScriptableObject
    {
        private string rootDirectory = "";
        public bool abortBatchOnFailure = false;
        public List<BuildSetupEntry> entriesList;

        public string RootDirectory
        {
            get
            {
                if(string.IsNullOrEmpty(rootDirectory))
                {
                    rootDirectory = EditorPrefs.GetString("EnhancedBuildsRootDirectory");
                }
                return rootDirectory;
            }
            set
            {
                if (string.Compare(value,rootDirectory) != 0)
                {
                    rootDirectory = value;
                    EditorPrefs.SetString("EnhancedBuildsRootDirectory", rootDirectory);
                }
            }
        } 

       

        public static BuildSetup Create()
        {
            BuildSetup asset = ScriptableObject.CreateInstance<BuildSetup>();

            AssetDatabase.CreateAsset(asset, BuildUtils.SETUPS_REL_DIRECTORY + "buildSetup.asset");
            AssetDatabase.SaveAssets();
            return asset;
        }

        public void addBuildSetupEntry()
        {
            BuildSetupEntry buildEntry = new BuildSetupEntry();

            var currentBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            buildEntry.buildName = Application.productName;
            buildEntry.target = EditorUserBuildSettings.activeBuildTarget;
            buildEntry.customScenes = new List<string>();
            buildEntry.scriptingBackend = PlayerSettings.GetScriptingBackend(currentBuildTargetGroup);

            entriesList.Add(buildEntry);
        }

        public void deleteBuildSetupEntry(BuildSetupEntry entry)
        {
            entriesList.Remove(entry);
        }

        public void duplicateBuildSetupEntry(BuildSetupEntry entry)
        {
            var index = entriesList.IndexOf(entry);
            BuildSetupEntry buildEntry = BuildSetupEntry.Clone(entry);
            buildEntry.buildName = buildEntry.buildName + "_clone";
            entriesList.Insert(index + 1, buildEntry);
        }

        public void rearrangeBuildSetupEntry(BuildSetupEntry entry, bool up)
        {
            var oldIndex = entriesList.IndexOf(entry);
            var newIndex = up ? oldIndex - 1 : oldIndex + 1;
            
            if(newIndex >= 0 && newIndex < entriesList.Count)
            {
                var otherEntry = entriesList[newIndex];
                entriesList[newIndex] = entry;
                entriesList[oldIndex] = otherEntry;
            }
        }

        public bool isReadyToBuild()
        {
            var hasPath = !string.IsNullOrEmpty(RootDirectory);
            var hasEntries = entriesList.Any(e => e.enabled);

            return hasPath && hasEntries;
        }
        public bool isReadyToBuildAndRun()
        {
            var hasPath = !string.IsNullOrEmpty(RootDirectory);
            var hasEntries = entriesList.Any(e => e.enabled);
            var hasOnlyOneEntry = entriesList.Count == 1;

            return hasPath && hasEntries && hasOnlyOneEntry;
        }
    }

}