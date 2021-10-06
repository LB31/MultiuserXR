using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;

#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;


#endif

#if ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

namespace Himeki.Build
{
    public static class BuildProcess
    {

        private const string BUILD_FILE_RELATIVE_PATH_ARG = "-buildSetupRelPath";

        public static void Build(BuildSetup buildSetup)
        {
            var defaultScenes = ScenesUtils.getDefaultScenesAsArray();

            string path = buildSetup.RootDirectory;

            var playerSettingsSnapshot = new PlayerSettingsSnapshot();

            var setupList = buildSetup.entriesList;
            for (var i = 0; i < setupList.Count; i++)
            {
                var setup = setupList[i];
                if (setup.enabled)
                {
                    var target = setup.target;
                    var targetGroup = BuildPipeline.GetBuildTargetGroup(target);

                    playerSettingsSnapshot.takeSnapshot(targetGroup);

                    PlayerSettings.SetScriptingBackend(targetGroup, setup.scriptingBackend);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, setup.scriptingDefineSymbols);

#if UNITY_2018_3_OR_NEWER
                    PlayerSettings.SetManagedStrippingLevel(targetGroup, setup.strippingLevel);
#endif

                    if (target == BuildTarget.Android)
                    {
                        #if UNITY_2017_4_OR_NEWER
                        EditorUserBuildSettings.buildAppBundle = setup.androidAppBundle;
                        PlayerSettings.Android.targetArchitectures = setup.androidArchitecture;
                        #endif
                    }

                    if(target == BuildTarget.PS4)
                    {
                        EditorUserBuildSettings.ps4HardwareTarget = setup.ps4HardwareTarget;
                        EditorUserBuildSettings.ps4BuildSubtarget = setup.ps4BuildSubtarget;
                    }

                    if (target == BuildTarget.PS4)
                    {
                        EditorUserBuildSettings.ps4HardwareTarget = setup.ps4HardwareTarget;
                        EditorUserBuildSettings.ps4BuildSubtarget = setup.ps4BuildSubtarget;
                    }

                    if(target == BuildTarget.StandaloneWindows ||
                       target == BuildTarget.StandaloneWindows64 ||
                       target == BuildTarget.StandaloneLinux64 ||
                       target == BuildTarget.StandaloneOSX)
                    {
                        EditorUserBuildSettings.enableHeadlessMode = setup.headlessModeServerBuild;
                    }

#if ADDRESSABLES
                    if(setup.rebuildAddressables)
                    {
                        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
                        AddressableAssetSettings.BuildPlayerContent();
                    }
#endif

                        var buildPlayerOptions = BuildUtils.getBuildPlayerOptionsFromBuildSetupEntry(setup, path, defaultScenes);

                    if(setup.autoRun == true) {
                        buildPlayerOptions.options |= BuildOptions.AutoRunPlayer;
                    }


#if UNITY_2018_1_OR_NEWER
                   bool tmpRestoreInitManagerOnStart = false;
                   List<XRLoader> tmpRestoreLoader = new List<XRLoader>(); 
                   XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
                    EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
                    if (buildTargetSettings != null) 
                    { 
                        XRGeneralSettings instance = buildTargetSettings.SettingsForBuildTarget(targetGroup);
                        if (instance != null) {
                            
                            tmpRestoreInitManagerOnStart = instance.InitManagerOnStart;
                            instance.InitManagerOnStart = setup.xrInitOnStart;
                            
                            instance.Manager.loaders.ForEach(loader => tmpRestoreLoader.Add(loader));
                            tmpRestoreLoader.ForEach(xrLoader => {
                                if(setup.xrLoaders.Find(loaderName => string.Compare(xrLoader.name,loaderName) == 0) == null) {
                                    instance.Manager.loaders.Remove(xrLoader);
                                }
                            });                           
                        }
                    }

                    BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                    BuildSummary buildSummary = report.summary;
                    var success = (buildSummary.result == BuildResult.Succeeded);
                    UnityEngine.Debug.Log("Build " + setup.buildName + " ended with Status: " + buildSummary.result);
#else
                    var result = BuildPipeline.BuildPlayer(buildPlayerOptions);
                    var success = string.IsNullOrEmpty(result);
                    UnityEngine.Debug.Log("Build " + setup.buildName + " ended with Success: " + success);
#endif

                    // Revert group build player settings after building
                    playerSettingsSnapshot.applySnapshot();
                    if (buildTargetSettings != null) 
                    { 
                        XRGeneralSettings instance = buildTargetSettings.SettingsForBuildTarget(targetGroup);
                        if (instance != null) {
                            instance.InitManagerOnStart = tmpRestoreInitManagerOnStart;
                            instance.Manager.loaders = tmpRestoreLoader;
                        }
                    }

                    if (!success && buildSetup.abortBatchOnFailure)
                    {
                        UnityEngine.Debug.LogError("Failure - Aborting remaining builds from batch");
                        break;
                    }
                }
                else
                {
                    UnityEngine.Debug.Log("Skipping Build " + setup.buildName);
                }
            }
        }

        public static void Build(string buildSetupRelativePath)
        {
            var buildSetup = AssetDatabase.LoadAssetAtPath(buildSetupRelativePath, typeof(BuildSetup)) as BuildSetup;
            if (buildSetup != null)
            {
                Build(buildSetup);
            }
            else
            {
                UnityEngine.Debug.LogError("Cannot find build setup in path: " + buildSetupRelativePath);
            }
        }

        public static void BuildWithArgs()
        {
            string buildFilePath = CLIUtils.GetCommandLineArg(BUILD_FILE_RELATIVE_PATH_ARG);

            if (!string.IsNullOrEmpty(buildFilePath))
            {
                Build(buildFilePath);
            }
            else
            {
                UnityEngine.Debug.LogError("Cannot find build setup path, make sure to specify using " + BUILD_FILE_RELATIVE_PATH_ARG);
            }
        }

    }
}