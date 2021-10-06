using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.XR.Management;

namespace Himeki.Build
{

    [Serializable]
    public class BuildSetupEntry
    {
        public bool enabled = true;
        public string buildName = "";
        public BuildTarget target = BuildTarget.NoTarget;
        public bool debugBuild = false;
        public string scriptingDefineSymbols = "";
        public bool useDefaultBuildScenes = true;
        public List<string> customScenes;

        // Advanced Options
#if UNITY_2018_3_OR_NEWER
        public ManagedStrippingLevel strippingLevel;
#endif
        public ScriptingImplementation scriptingBackend = ScriptingImplementation.IL2CPP;
        public string assetBundleManifestPath = "";
        public bool strictMode = false;

        public bool xrInitOnStart = false;
        public List<string> xrLoaders = new List<string>();
        public bool test = false;

#if UNITY_2020_1_OR_NEWER
        public bool detailedBuildReport = false;
#endif

#if ADDRESSABLES
        public bool rebuildAddressables = false;
#endif

        //iOS
        public bool iosSymlinkLibraries = false;

        //Android
#if UNITY_2017_4_OR_NEWER
        public bool androidAppBundle = false;
        public AndroidArchitecture androidArchitecture;

        public UnityEngine.Rendering.GraphicsDeviceType[] androidGraphicsDeviceType;
#endif

        //PS4
        public PS4BuildSubtarget ps4BuildSubtarget;
        public PS4HardwareTarget ps4HardwareTarget;

        //Standalone Headless Mode Server Build
        public bool headlessModeServerBuild = false;

        // GUI status
        [NonSerialized] public bool guiShowOptions = true;
        [NonSerialized] public bool guiShowCustomScenes = false;
        [NonSerialized] public bool guiShowAdvancedOptions = false;
        [NonSerialized] public bool guiShowXROptions = false;
        public static BuildSetupEntry Clone(BuildSetupEntry source)
        {
            BuildSetupEntry newbuildSetupEntry = source.MemberwiseClone() as BuildSetupEntry;
            //Deep Copy xrLoaders List
            newbuildSetupEntry.xrLoaders = new List<string>(source.xrLoaders);

            return newbuildSetupEntry;
        }
        
        [NonSerialized] public bool autoRun = false;

    }

}