using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;


namespace Himeki.Build
{
    public class EnhancedBuildsWindow : EditorWindow
    {
        private const string EDITOR_PREFS_KEY = "ObjectPath";
        private const string WINDOW_TITLE = "Enhanced Builds";
        public BuildSetup buildSetup;
        private Vector2 buildEntriesListScrollPos;

        [MenuItem("Kairo MX/Open Enhanced Builds %#e")]
        static void Init()
        {
            EnhancedBuildsWindow window = (EnhancedBuildsWindow)EditorWindow.GetWindow(typeof(EnhancedBuildsWindow), false, WINDOW_TITLE, true);

            window.Show();
        }

        void OnEnable()
        {
            Undo.undoRedoPerformed += onUndoRedo;

            if (EditorPrefs.HasKey(EDITOR_PREFS_KEY))
            {
                string objectPath = EditorPrefs.GetString(EDITOR_PREFS_KEY);
                buildSetup = AssetDatabase.LoadAssetAtPath(objectPath, typeof(BuildSetup)) as BuildSetup;
            }
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= onUndoRedo;
        }

        private void onUndoRedo()
        {
            if (buildSetup)
            {
                EditorUtility.SetDirty(buildSetup);
                Repaint();
            }
        }

        void OnGUI()
        {
            EditorGUIUtility.labelWidth = 0f;

            GUILayout.Label("Build Setup Editor", EditorStyles.boldLabel);
            GUILayout.Space(10);
            if (buildSetup != null)
            {
                string objectPath = EditorPrefs.GetString(EDITOR_PREFS_KEY);
                EditorGUILayout.LabelField("Current Build Setup File", objectPath);
            }

            GUILayout.BeginHorizontal();

            if (buildSetup != null)
            {
                if (GUILayout.Button("Show in Library"))
                {
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = buildSetup;
                }
            }

            if (GUILayout.Button("Select Build File"))
            {
                selectBuildFile();
            }

            if (GUILayout.Button("Create New Build File"))
            {
                createNewBuildSetup();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (buildSetup != null)
            {
                GUILayout.Label("Loaded Build Setup", EditorStyles.boldLabel);

                GUILayout.Space(10);

                EditorGUIUtility.labelWidth = 200f;
                if (GUILayout.Button("Choose Root Directory", GUILayout.ExpandWidth(false)))
                {
                    Undo.RecordObject(buildSetup, "Set Build Setup Root Directory");
                    string tmp = EditorUtility.SaveFolderPanel("Choose Location", "", "");
                    if(!string.IsNullOrEmpty(tmp))
                    {
                        buildSetup.RootDirectory = tmp;
                    }
                }
                EditorGUILayout.LabelField("Root Directory", buildSetup.RootDirectory);

                GUILayout.Space(10); 

                buildSetup.abortBatchOnFailure = EditorGUILayout.Toggle("Abort batch on failure", buildSetup.abortBatchOnFailure);

                int buildsAmount = buildSetup.entriesList.Count;

                GUILayout.Space(10);
                GUILayout.Label("Builds (" + buildsAmount + ")", EditorStyles.label);
                GUILayout.Space(5);

                if (buildsAmount > 0)
                {
                    buildEntriesListScrollPos = EditorGUILayout.BeginScrollView(buildEntriesListScrollPos, false, false, GUILayout.Width(position.width), GUILayout.MaxHeight(500));

                    var list = buildSetup.entriesList;
                    for (var i = 0; i < list.Count; i++)
                    {
                        var b = list[i];
                        EditorGUILayout.BeginHorizontal();

                        b.enabled = EditorGUILayout.Toggle("", b.enabled, GUILayout.MaxWidth(15.0f));
                        b.guiShowOptions = EditorGUILayout.Foldout(b.guiShowOptions, b.buildName, EditorStyles.foldout);

                        using (new EditorGUI.DisabledScope(i == 0))
                        {
                            if (GUILayout.Button(new GUIContent("↑", "Rearranges Build Entry up"), GUILayout.ExpandWidth(false)))
                            {
                                Undo.RecordObject(buildSetup, "Rearranged Build Entry up");
                                buildSetup.rearrangeBuildSetupEntry(b, true);
                            }
                        }

                        using (new EditorGUI.DisabledScope(i == list.Count - 1))
                        {
                            if (GUILayout.Button(new GUIContent("↓", "Rearranges Build Entry down"), GUILayout.ExpandWidth(false)))
                            {
                                Undo.RecordObject(buildSetup, "Rearranged Build Entry down");
                                buildSetup.rearrangeBuildSetupEntry(b, false);
                            }
                        }

                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button(new GUIContent("x", "Deletes Build Entry"), GUILayout.ExpandWidth(false)))
                        {
                            if(EditorUtility.DisplayDialog("Delete Build Entry?",
                                "Are you sure you want to delete build entry " + b.buildName
                                , "Yes", "No"))
                            {
                                Undo.RecordObject(buildSetup, "Removed Build Setup Entry");
                                buildSetup.deleteBuildSetupEntry(b);
                            }
                        }

                        GUI.backgroundColor = Color.green;
                        if (GUILayout.Button(new GUIContent("c", "Duplicates Build Entry"), GUILayout.ExpandWidth(false)))
                        {
                            Undo.RecordObject(buildSetup, "Duplicate Build Setup Entry");
                            buildSetup.duplicateBuildSetupEntry(b);
                        }
                        GUI.backgroundColor = Color.white;

                        using (new EditorGUI.DisabledScope(!b.enabled))
                        {
                            if (GUILayout.Button(new GUIContent("Build and Run", "Build and Run Platform"), GUILayout.ExpandWidth(false)))
                            {
                              buildAndRunGame(b);
                            }
                        }        

                        EditorGUILayout.EndHorizontal();
                        if (b.guiShowOptions)
                        {
                            EditorGUI.indentLevel++;
                            drawBuildEntryGUI(b);
                            EditorGUI.indentLevel--;
                        }

                        GUILayout.Space(5);
                    }

                    EditorGUILayout.EndScrollView();

                }
                else
                {
                    GUILayout.Label("This Built List is Empty");
                }

                GUILayout.Space(10);

                if (GUILayout.Button(new GUIContent("Add Entry", "Adds a new build entry to the list"), GUILayout.ExpandWidth(true)))
                {
                    Undo.RecordObject(buildSetup, "Add Build Setup Entry");
                    buildSetup.addBuildSetupEntry();
                }

                GUILayout.Space(10);

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                GUILayout.Space(10);

                var isReady = buildSetup.isReadyToBuild();
                using (new EditorGUI.DisabledScope(!isReady))
                {
                    if (GUILayout.Button(new GUIContent("Build","Build all enabled platforms"), GUILayout.ExpandWidth(true)))
                    {
                        buildGame();
                    }
                }           

                if (!isReady)
                {
                    GUILayout.Label("Define a Root directory and at least one active build entry");
                }
            }
            else
            {
                GUILayout.Label("Select or Create a new Build Setup", EditorStyles.boldLabel);
            }
            if (GUI.changed)
            {
                EditorUtility.SetDirty(buildSetup);
            }
        }

        private void drawBuildEntryGUI(BuildSetupEntry b)
        {
            b.buildName = EditorGUILayout.TextField("Build Name", b.buildName);
            b.target = (BuildTarget)EditorGUILayout.EnumPopup("Target", b.target);
            if(b.target > 0)
            {
                b.debugBuild = EditorGUILayout.Toggle("Debug Build", b.debugBuild);
                b.scriptingDefineSymbols = EditorGUILayout.TextField("Scripting Define Symbols", b.scriptingDefineSymbols);

                drawScenesSectionGUI(b);
                drawAdvancedOptionsSectionGUI(b);
                drawVROptionSectionGUI(b);
            }
        }

        private void drawScenesSectionGUI(BuildSetupEntry b)
        {
            b.useDefaultBuildScenes = EditorGUILayout.Toggle("Use Default Build Scenes", b.useDefaultBuildScenes);

            if (!b.useDefaultBuildScenes)
            {
                b.guiShowCustomScenes = EditorGUILayout.Foldout(b.guiShowCustomScenes, "Custom Scenes");
                if (b.guiShowCustomScenes)
                {
                    EditorGUI.indentLevel++;
                    if (b.customScenes.Count > 0)
                    {
                        var scenes = b.customScenes;

                        for (int i = 0; i < scenes.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            scenes[i] = EditorGUILayout.TextField("Scene " + i, scenes[i]);
                            if (GUILayout.Button("Select Scene", GUILayout.ExpandWidth(false)))
                            {
                                string absPath = EditorUtility.OpenFilePanel("Select Scene file", "", "unity");
                                if (absPath.StartsWith(Application.dataPath))
                                {
                                    string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
                                    scenes[i] = relPath;
                                }
                            }
                            if (GUILayout.Button("Remove Scene", GUILayout.ExpandWidth(false)))
                            {
                                Undo.RecordObject(buildSetup, "Remove Build Setup Entry Custom scene");
                                b.customScenes.RemoveAt(i);
                                i--;
                            }
                            GUILayout.EndHorizontal();
                        }

                    }

                    using(var horizontalScope = new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20f);
                    
                        if (GUILayout.Button("Add Scene", GUILayout.ExpandWidth(false)))
                        {
                            Undo.RecordObject(buildSetup, "Add Build Setup Entry Custom scene");
                            b.customScenes.Add(string.Empty);
                        }
                    }
                    

                    EditorGUI.indentLevel--;
                }
            }
        }

        private void drawAdvancedOptionsSectionGUI(BuildSetupEntry b)
        {
            b.guiShowAdvancedOptions = EditorGUILayout.Foldout(b.guiShowAdvancedOptions, "Advanced Options");
            if (b.guiShowAdvancedOptions)
            {
                EditorGUI.indentLevel++;

#if UNITY_2020_1_OR_NEWER
                b.detailedBuildReport = EditorGUILayout.Toggle("Detailed Build Report", b.detailedBuildReport);
#endif

#if ADDRESSABLES
                b.rebuildAddressables = EditorGUILayout.Toggle("Rebuild Addressables", b.rebuildAddressables);
#endif

#if UNITY_2018_3_OR_NEWER
                b.strippingLevel = (ManagedStrippingLevel)EditorGUILayout.EnumPopup("Stripping Level", b.strippingLevel);
#endif

                b.strictMode = EditorGUILayout.Toggle(new GUIContent("Strict Mode",
                                                    "Do not allow the build to succeed if any errors are reported."),
                                                    b.strictMode);
                b.assetBundleManifestPath = EditorGUILayout.TextField("AssetBundle Manifest Path", b.assetBundleManifestPath);
                b.scriptingBackend = (ScriptingImplementation)EditorGUILayout.EnumPopup("Scripting Backend", b.scriptingBackend);

                if (b.target == BuildTarget.iOS)
                {
                    b.iosSymlinkLibraries = EditorGUILayout.Toggle("XCode - Symlink Library", b.iosSymlinkLibraries);
                }

                if (b.target == BuildTarget.Android)
                {
                    #if UNITY_2017_4_OR_NEWER
                    b.androidAppBundle = EditorGUILayout.Toggle("Build Android App Bundle", b.androidAppBundle);
                    b.androidArchitecture = (AndroidArchitecture)EditorGUILayout.EnumPopup("Android Architecture", b.androidArchitecture);
                    #endif
                }

                if(b.target == BuildTarget.PS4)
                {
                    b.ps4BuildSubtarget = (PS4BuildSubtarget)EditorGUILayout.EnumPopup("PS4 Build Subtarget", b.ps4BuildSubtarget);
                    b.ps4HardwareTarget = (PS4HardwareTarget)EditorGUILayout.EnumPopup("PS4 Hardware Target", b.ps4HardwareTarget);
                }

                if(b.target == BuildTarget.StandaloneWindows   ||
                   b.target == BuildTarget.StandaloneWindows64 ||
                   b.target == BuildTarget.StandaloneLinux64   || 
                   b.target == BuildTarget.StandaloneOSX )
                {
                    b.headlessModeServerBuild = EditorGUILayout.Toggle(new GUIContent(
                        "Headless Mode",
                        "If enabled the created build is a headless server build."),
                        b.headlessModeServerBuild);
                }

                EditorGUI.indentLevel--;
            }
        }

        private void drawVROptionSectionGUI(BuildSetupEntry b)
        {
             b.guiShowXROptions = EditorGUILayout.Foldout(b.guiShowXROptions, "XR Options");

             if (b.guiShowXROptions)
            {
                EditorGUI.indentLevel++;

                XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
                EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
                if (buildTargetSettings != null) 
                { 
                    EditorGUILayout.HelpBox("Make sure all needed 'Plug-in Providers' are enabled within Project Settings --> XR Management", UnityEditor.MessageType.None);
                    b.xrInitOnStart = EditorGUILayout.Toggle("Init On Start",  b.xrInitOnStart);
                    
                    XRGeneralSettings instance = buildTargetSettings.SettingsForBuildTarget(BuildPipeline.GetBuildTargetGroup(b.target));
                    if (instance != null) {
#pragma warning disable CS0618 // Currently the preview version do not offer any other way to access all loaders
                        foreach (XRLoader loader in instance.Manager.loaders) {
#pragma warning restore CS0618
                            bool isInList = b.xrLoaders.Find(loaderName => string.Compare(loaderName,loader.name) == 0) != null;
                            bool tmp = EditorGUILayout.Toggle(loader.name,  isInList);
                            if(tmp == true && tmp != isInList) {
                                b.xrLoaders.Add(loader.name);
                            } 
                            else if (tmp == false && tmp != isInList) {
                                b.xrLoaders.Remove(loader.name);
                            }
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        private void buildGame()
        {
            BuildProcess.Build(buildSetup);
        }

        private void buildAndRunGame(BuildSetupEntry entryToBuildAndRun) 
        {
            //Switch current platform if necessary 
            if(EditorUserBuildSettings.activeBuildTarget != entryToBuildAndRun.target) {        
                BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(entryToBuildAndRun.target);                  
                EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, entryToBuildAndRun.target);
            }

            //Temporary disable all other build platforms and add autoRun flag
            bool [] enableStates = new bool[buildSetup.entriesList.Count];
            for(int i = 0;i < buildSetup.entriesList.Count; i++) {
                enableStates[i] = buildSetup.entriesList[i].enabled;
                if(buildSetup.entriesList[i] != entryToBuildAndRun) {
                    buildSetup.entriesList[i].enabled = false;
                }
            } 
            entryToBuildAndRun.autoRun = true;
                              
            //Start Auto Run Build Process
            buildGame();
                               
            //Revert Temporary stored enable states
            entryToBuildAndRun.autoRun = false;
            for(int i = 0;i < buildSetup.entriesList.Count; i++) {
               buildSetup.entriesList[i].enabled = enableStates[i];
            } 
        }

        private void createNewBuildSetup()
        {
            buildSetup = BuildSetup.Create();
            if (buildSetup)
            {
                buildSetup.entriesList = new List<BuildSetupEntry>();
                string relPath = AssetDatabase.GetAssetPath(buildSetup);
                EditorPrefs.SetString(EDITOR_PREFS_KEY, relPath);
            }
        }
        private void selectBuildFile()
        {
            string absPath = EditorUtility.OpenFilePanel("Select Build Setup file", BuildUtils.SETUPS_REL_DIRECTORY, "asset");
            if (absPath.StartsWith(Application.dataPath))
            {
                string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
                var loadedBuildAsset = AssetDatabase.LoadAssetAtPath(relPath, typeof(BuildSetup)) as BuildSetup;

                if (loadedBuildAsset)
                {
                    buildSetup = loadedBuildAsset;
                    EditorPrefs.SetString(EDITOR_PREFS_KEY, relPath);
                }
            }
        }
    }
}