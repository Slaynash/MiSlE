using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

#if !MSZ_MONO
using Il2Cpp;
using Il2CppInterop.Runtime;
#endif
#if MISIDEZERO
using MiSlE.MiSideZero;
using UnityEngine.UI;
#endif

namespace MiSlE
{
    internal static class ScenarioManager
    {
        private class LocationHaveImage
        {
            public int id;
            public Texture2D? preview;
        }

        //private static Dictionary<string, Dictionary<int, Texture2D?>> scenarioLocations = new(); // { name: { locationId, locationPreview } }
        private static Dictionary<string, Dictionary<int, string>> scenarioLanguages = new();

        private static MelonLogger.Instance loggerInstance = new MelonLogger.Instance("MiSlE:Scenarios", MiSideSlaynashExtensionMelon.LogColor);

        //private static AssetsManager assetsManager = new AssetsManager();
        private static Dictionary<string, Shader> cachedShaders = new();

#if MISIDE
        private const float ITEM_SPACING = 55;
#else
        private const float ITEM_SPACING = 32.9261f;
#endif

        public static void RegisterScenario(string scenarioPath)
        {
            loggerInstance.Msg("Registering scenario at " + scenarioPath);

            string scenarioName = Path.GetFileName(scenarioPath);

            if (Directory.Exists(Path.Combine(scenarioPath, "Locations")))
            {
                List<LocationHaveImage> locationsHaveImage = new();

                foreach (string file in Directory.GetFiles(Path.Combine(scenarioPath, "Locations"), "*.assetbundle"))
                {
                    string scenarioLocationFullname = Path.GetFileNameWithoutExtension(file);
                    if (scenarioLocationFullname.StartsWith(scenarioName) && int.TryParse(scenarioLocationFullname.Substring(scenarioName.Length), out int scenarioLocationId))
                    {
                        if (File.Exists(Path.Combine(Path.Combine(scenarioPath, "Locations"), scenarioLocationFullname + ".png")))
                        {
                            // TODO load png as Texture2D
                            // locationsHaveImage.Add(scenarioLevelId, previewImage);
                        }
                        else
                            locationsHaveImage.Add(new LocationHaveImage { id = scenarioLocationId, preview = null });
                    }
                    else
                        loggerInstance.Warning($"Couldn't load location for scenario \"{scenarioName}\" with invalid name {scenarioLocationFullname}");
                }

                if (locationsHaveImage.Count > 0)
                {
                    //scenarioLocations[scenarioName] = locationsHaveImage;

                    Vector2 containerSize = MainMenuManager.ScenarioContainer.sizeDelta;

                    RectTransform scenarioButtonRT = GameObject.Instantiate(MainMenuManager.FirstLocationButton, MainMenuManager.ScenarioContainer);
                    scenarioButtonRT.name = $"Button Scenario {scenarioName}";
#if MISIDE
                    //scenarioButtonRT.anchoredPosition = new Vector2(10, -(containerSize.y - 5));
#else
                    scenarioButtonRT.anchoredPosition = new Vector2(0, -(containerSize.y - 5) + 105);
                    scenarioButtonRT.pivot = new Vector2(0.5f, 1.0f);
                    scenarioButtonRT.anchorMin = new Vector2(0.5f, 1.0f);
                    scenarioButtonRT.anchorMax = new Vector2(0.5f, 1.0f);
#endif

                    MainMenuManager.ScenarioContainer.sizeDelta = containerSize + new Vector2(0, ITEM_SPACING);

#if MISIDE
                    Localization_UIText scenarioButtonLoc = scenarioButtonRT.Find("Text").GetComponent<Localization_UIText>();
#else
                    Localization_UIText scenarioButtonLoc = scenarioButtonRT.Find("Text").gameObject.AddComponent<Localization_UIText>();
#endif
                    scenarioButtonLoc.NameFile = $"LocationsDescription {scenarioName}";
                    scenarioButtonLoc.StringNumber = 1;

#if MISIDE
                    GameObject.DestroyImmediate(scenarioButtonRT.GetComponent<MenuNextLocation>());
                    ButtonMouseClick scenarioButtonBMC = scenarioButtonRT.GetComponent<ButtonMouseClick>();
                    UnityEvent scenarioButtonOnClick = scenarioButtonBMC.eventClick = new UnityEvent();
#else
                    UIButtonCore scenarioButtonUIBC = scenarioButtonRT.GetComponent<UIButtonCore>();
                    UnityEvent scenarioButtonOnClick = scenarioButtonUIBC.onClick = new UnityEvent();
#endif

                    float locationContainerHeight = locationsHaveImage.Count * ITEM_SPACING + 5;

                    GameObject scenarioLocationsGO = new GameObject($"LocationsContainer {scenarioName}");
                    scenarioLocationsGO.transform.parent = MainMenuManager.ScenarioContainer;
                    scenarioLocationsGO.transform.localScale = Vector3.one;
                    scenarioLocationsGO.transform.localRotation = Quaternion.identity;
                    RectTransform scenarioLocationsRT = scenarioLocationsGO.GetComponent<RectTransform>() ?? scenarioLocationsGO.AddComponent<RectTransform>();
                    scenarioLocationsRT.anchorMin = new Vector2(0, 1);
                    scenarioLocationsRT.anchorMax = new Vector2(0, 1);
                    scenarioLocationsRT.anchoredPosition = new Vector2(60, -(containerSize.y + ITEM_SPACING - 5));
                    scenarioLocationsRT.sizeDelta = new Vector2(scenarioLocationsRT.sizeDelta.x - ITEM_SPACING - 5, locationContainerHeight);
                    scenarioLocationsRT.pivot = new Vector2(0, 1);

                    //scenarioLocationsRT.gameObject.AddComponent<Image>().sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), Vector2.one * 0.5f);

                    scenarioButtonOnClick.AddListener((UnityAction)(() =>
                    {
                        if (scenarioLocationsRT.gameObject.activeSelf)
                        {
                            scenarioLocationsRT.gameObject.SetActive(false);
                            MainMenuManager.ScenarioContainer.sizeDelta = MainMenuManager.ScenarioContainer.sizeDelta - new Vector2(0, locationContainerHeight);

                            bool foundSelf = false;
                            for (int i = 0; i < MainMenuManager.ScenarioContainer.childCount; ++i)
                            {
                                RectTransform scenarioContainerChild = MainMenuManager.ScenarioContainer.GetChild(i).GetComponent<RectTransform>();
                                if (foundSelf)
                                    scenarioContainerChild.anchoredPosition = scenarioContainerChild.anchoredPosition + new Vector2(0, locationContainerHeight);
                                else if (scenarioContainerChild == scenarioButtonRT)
                                {
                                    foundSelf = true;
                                    ++i;
                                }
                            }
                        }
                        else
                        {
                            scenarioLocationsRT.gameObject.SetActive(true);
                            MainMenuManager.ScenarioContainer.sizeDelta = MainMenuManager.ScenarioContainer.sizeDelta + new Vector2(0, locationContainerHeight);

                            bool foundSelf = false;
                            for (int i = 0; i < MainMenuManager.ScenarioContainer.childCount; ++i)
                            {
                                RectTransform scenarioContainerChild = MainMenuManager.ScenarioContainer.GetChild(i).GetComponent<RectTransform>();
                                if (foundSelf)
                                    scenarioContainerChild.anchoredPosition = scenarioContainerChild.anchoredPosition - new Vector2(0, locationContainerHeight);
                                else if (scenarioContainerChild == scenarioButtonRT)
                                {
                                    foundSelf = true;
                                    ++i;
                                }
                            }
                        }
                    }));

                    for (int i = 0; i < locationsHaveImage.Count; ++i)
                    {
                        RectTransform locationButtonRT = GameObject.Instantiate(MainMenuManager.FirstLocationButton, scenarioLocationsRT);
                        locationButtonRT.anchoredPosition = new Vector2(0, -(i * ITEM_SPACING + 2.5f));
#if MISIDE
                        locationButtonRT.sizeDelta = locationButtonRT.sizeDelta - new Vector2(60, 0);
#else

#endif

#if MISIDE
                        Localization_UIText locationButtonLoc = locationButtonRT.Find("Text").GetComponent<Localization_UIText>();
#else
                        Localization_UIText locationButtonLoc = locationButtonRT.Find("Text").gameObject.AddComponent<Localization_UIText>();
#endif
                        locationButtonLoc.NameFile = $"LocationsDescription {scenarioName}";
                        locationButtonLoc.StringNumber = 2 + i;

#if MISIDE
                        //GameObject.DestroyImmediate(locationButtonRT.GetComponent<MenuNextLocation>());
                        ButtonMouseClick locationButtonBMC = locationButtonRT.GetComponent<ButtonMouseClick>();
                        UnityEvent locationButtonOnClick = locationButtonBMC.eventClick = new UnityEvent();
#else
                        UIButtonCore locationButtonUIBC = locationButtonRT.GetComponent<UIButtonCore>();
                        UnityEvent locationButtonOnClick = locationButtonUIBC.onClick = new UnityEvent();
#endif
                        string locationFilePath = $"CustomScenarios/{scenarioName}/Locations/{scenarioName}{locationsHaveImage[i].id}.assetbundle";
                        locationButtonOnClick.AddListener((UnityAction)(() => LoadCustomLocation(locationFilePath)));

                        // TODO setup custom image
                    }

                    scenarioLocationsRT.gameObject.SetActive(false);

                    //MainMenuManager.ScenarioContainer.parent.parent.GetComponent<UI_Colors>().Reload();
                }
            }

            if (Directory.Exists(Path.Combine(scenarioPath, "Languages")))
                LocalizationManager.RegisterLocalizationRoot(Path.Combine(scenarioPath, "Languages"));
        }

        internal static void Initialize(HarmonyLib.Harmony harmony)
        {
            // TODO setup FileSystemWatcher according to https://stackoverflow.com/a/21934908/4417115

#if MISIDE
            harmony.Patch(
                AccessTools.Method(typeof(World), nameof(World.Awake)),
                postfix: new HarmonyMethod(typeof(ScenarioManager), nameof(WorldAwakePostfix)));
#else
            harmony.Patch(
               AccessTools.Method(typeof(PlayerManager), "Start"),
               postfix: new HarmonyMethod(typeof(ScenarioManager), nameof(PlayerManagerStartPostfix)));
#endif
        }

#if MISIDE
        private static void WorldAwakePostfix(World __instance)
#else
        private static void PlayerManagerStartPostfix(PlayerManager __instance)
#endif
        {
#if MISIDE
            foreach (UnityEngine.Object matObj in Resources.FindObjectsOfTypeAll(Il2CppType.Of<Material>()))
#else
            foreach (Material matObj in Resources.FindObjectsOfTypeAll<Material>())
#endif
            {
                if (matObj == null)
                    continue;

#if MISIDE
                Material mat = matObj.Cast<Material>();
#else
                Material mat = matObj;
#endif

                if (!mat.shader)
                    continue;

                string shaderName = mat.shader.name;
                if (cachedShaders.TryGetValue(shaderName, out Shader cachedShader))
                {
                    if (mat.shader == cachedShader)
                        continue;

                    mat.shader = cachedShader;

                    loggerInstance.Msg($"Replaced shader of {mat.name} (shader: {shaderName})");
                }
            }
        }

        internal static void OnMenuLoaded()
        {
            if (cachedShaders.Count == 0)
            {
                GameObject shaderCache = new GameObject("ShaderCacheParent");
                GameObject.DontDestroyOnLoad(shaderCache);
                shaderCache.SetActive(false);
                MeshRenderer shaderCacheMR = shaderCache.AddComponent<MeshRenderer>();
                shaderCacheMR.enabled = false;

                cachedShaders = new();
                List<Shader> toStoreShaders = new List<Shader>();
#if MISIDE
                foreach (UnityEngine.Object shaderObj in Resources.FindObjectsOfTypeAll(Il2CppType.Of<Shader>()))
#else
                foreach (Shader shaderObj in Resources.FindObjectsOfTypeAll<Shader>())
#endif
                {
                    if (shaderObj == null)
                        continue;
#if MISIDE
                    Shader shader = shaderObj.Cast<Shader>();
#else
                    Shader shader = shaderObj;
#endif
                    string shaderNameLower = shader.name.ToLower();
                    if ((shaderNameLower.StartsWith("aihasto") || shaderNameLower.StartsWith("voidway") || shaderNameLower.StartsWith("realtoon")) && !cachedShaders.ContainsKey(shaderNameLower))
                    {
                        loggerInstance.Msg("Caching shader " + shader.name);
                        cachedShaders[shader.name] = shader;
                        toStoreShaders.Add(shader);
                    }
                }

                Material[] materialCacheArray = new Material[cachedShaders.Count];
                int offset = 0;
                foreach (Shader toStoreShader in toStoreShaders)
                    materialCacheArray[offset++] = new Material(toStoreShader);
                shaderCacheMR.materials = materialCacheArray;
            }

            if (!Directory.Exists("CustomScenarios"))
                Directory.CreateDirectory("CustomScenarios");

            MainMenuManager.ScenarioContainer.sizeDelta = new Vector2(MainMenuManager.ScenarioContainer.sizeDelta.x, 5);

            foreach (string scenarioPath in Directory.GetDirectories("CustomScenarios"))
            {
                RegisterScenario(scenarioPath);
            }
        }



        private static void LoadCustomLocation(string path)
        {
            loggerInstance.Msg("Loading location " + path);

            //Dictionary<string, Shader> referenceShaders = new();
            //Shader[] shaders = Resources.FindObjectsOfTypeAll<Shader>();
            //foreach (Shader shader in shaders)
            //{
            //    string shaderNameLower = shader.name.ToLower();
            //    if (shaderNameLower.StartsWith("aihasto") && !referenceShaders.ContainsKey(shaderNameLower))
            //        referenceShaders[shaderNameLower] = shader;
            //    shader.
            //}

            //BundleFileInstance bundleFileInstance = assetsManager.LoadBundleFile(path);
            //AssetsFileInstance assetsFileInstance = assetsManager.LoadAssetsFileFromBundle(bundleFileInstance, 0);

            //foreach (var matInfo in assetsFileInstance.file.GetAssetsOfType(AssetClassID.Material))
            //{
            //    var texBase = assetsManager.GetBaseField(assetsFileInstance, matInfo);
            //    loggerInstance.Msg(texBase["m_Shader"].TypeName);
            //    texBase["m_Shader.m_FileID"].AsInt = 0;
            //    texBase["m_Shader.m_FileID"].AsInt = 0;
            //    //texBase["m_Shader"].TypeName += " abcdefg";
            //    //matInfo.SetNewData(texBase);
            //}
            ////bundleFileInstance.file.

#if MISIDE
            Il2CppAssetBundle ab = Il2CppAssetBundleManager.LoadFromFile(path);
#else
            AssetBundle ab = AssetBundle.LoadFromFile(path);
#endif
            if (ab == null)
            {
                loggerInstance.Error("Failed to load AB " + path + " from file");
                return;
            }
            if (!ab.isStreamedSceneAssetBundle)
            {
                loggerInstance.Error("AB " + path + "is not a world AB!");
                return;
            }

            //// Patch assetbundle shaders
            //foreach (string objName in ab.GetAllAssetNames())
            //{
            //    //loggerInstance.Msg(objName);

            //    UnityEngine.Object obj = ab.Load(objName);
            //    if (obj is GameObject go)
            //        FixupAssetbundleRootGO(go);
            //    else if (obj is Material mat)
            //        FixupAssetbundleMaterial(mat);
                
            //    //loggerInstance.Msg(" " + obj.GetIl2CppType().Name + " " + obj.name);
            //}

            string[] scenePaths = ab.GetAllScenePaths();
            if (scenePaths.Length > 1)
                loggerInstance.Warning($"Location contains more than one scene. Only {scenePaths[0]} will be loaded");


            string sceneName = Path.GetFileNameWithoutExtension(scenePaths[0]);

#if MISIDE
            GlobalGame.nameLoadedScene = sceneName;
            GlobalGame.levelLoad = 0;
            GameObject.Find("MenuGame").GetComponent<Menu>().ButtonLoadScene(sceneName);
#elif MSZ_IL2CPP
            Il2Cpp.Void.instance.load.targetSceneName = sceneName;
            Il2Cpp.Void.instance.StartCoroutine(Il2Cpp.Void.instance.load.LoadRoutine());
#else
            Void.instance.load.targetSceneName = sceneName;
            Void.instance.StartCoroutine(Void.instance.load.LoadRoutine());
#endif
        }
    }
}
