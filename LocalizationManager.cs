using System.Collections.Generic;
using HarmonyLib;
using MelonLoader;
using System.IO;


#if MISIDE
using Il2Cpp;
#else
using MiSlE.MiSideZero;
using System.Reflection;
#endif

namespace MiSlE
{
    public static class LocalizationManager
    {
        private static bool patchApplied = false;

        private static Dictionary<string, string> fallbackLanguages = new Dictionary<string, string>()
        {
            { "French_Renew", "French" },

            // MSZ fallbacks
            { "en", "English" },
        };

        private static Dictionary<string, Dictionary<string, Dictionary<int, string>>> registeredLanguages = new()
        {
            { "Russian", new()
                {
                    { "MiSlE", new ()
                        {
                            { 0, "Аддоны" }
                        }
                    }
                }
            },
            { "English", new()
                {
                    { "MiSlE", new ()
                        {
                            { 0, "Addons" }
                        }
                    }
                }
            },
            { "French", new()
                {
                    { "MiSlE", new ()
                        {
                            { 0, "Addons" }
                        }
                    }
                }
            }
        };

        private static MelonLogger.Instance loggerInstance = new MelonLogger.Instance("MiSlE:Localizations", MiSideSlaynashExtensionMelon.LogColor);

        private static List<string> localizationRoots = new();

        private static Dictionary<string, Dictionary<int, string>> cachedLocalisations = new();
        private static Dictionary<string, Dictionary<int, string>> cachedLocalisationsFallback = new();
        private static Dictionary<string, Dictionary<int, string>> cachedLocalisationsEnglish = new();
        private static string lastLanguage = "";

#if MSZ_MONO
        private static FieldInfo LocalizationManager_currentLanguage = typeof(global::LocalizationManager).GetField("currentLanguage", (BindingFlags)(-1));
        private static string GameLanguage => (string)LocalizationManager_currentLanguage.GetValue(null);
#elif MSZ_IL2CPP
        private static string GameLanguage => Il2Cpp.LocalizationManager.currentLanguage;
#else
        private static string GameLanguage => GlobalGame.Language;
#endif


        public static void RegisterLocalization(string language, string name, int index, string _string)
        {
            if (!registeredLanguages.TryGetValue(language, out var registeredNames))
                registeredNames = registeredLanguages[language] = new ();
            if (!registeredNames.TryGetValue(name, out var registeredStrings))
                registeredStrings = registeredNames[name] = new();
            registeredStrings[index] = _string;
        }

        internal static void Init(HarmonyLib.Harmony harmony)
        {
            if (patchApplied)
                return;

            patchApplied = true;

            harmony.Patch(
                AccessTools.Method(typeof(Localization_UIText), nameof(Localization_UIText.TextTranslate)),
                postfix: new HarmonyMethod(typeof(LocalizationManager), nameof(TextTranslatePostfix)));

#if MISIDE
            harmony.Patch(
                AccessTools.Method(typeof(GlobalLanguage), nameof(GlobalLanguage.GetString)),
                postfix: new HarmonyMethod(typeof(LocalizationManager), nameof(GetStringPostfix)));
#endif

            if (lastLanguage != GameLanguage)
                ReloadLanguages();
        }


        internal static void RegisterLocalizationRoot(string path)
        {
            localizationRoots.Add(path);

            if (lastLanguage != "")
            {
                LoadAndCacheLocalisationFiles(path, lastLanguage);
                if (fallbackLanguages.TryGetValue(path, out var fallbackLanguage) && fallbackLanguage != "English")
                    LoadAndCacheLocalisationFiles(path, fallbackLanguage, true);
                if (lastLanguage != "English")
                    LoadAndCacheLocalisationFiles(path, "English");
            }
        }

        private static void ReloadLanguages()
        {
            cachedLocalisations.Clear();
            cachedLocalisationsFallback.Clear();
            cachedLocalisationsEnglish.Clear();

            loggerInstance.Msg($"Language changed to \"{GameLanguage}\"");
            lastLanguage = GameLanguage;

            {
                CacheRegisteredLocalization(lastLanguage);
                if (fallbackLanguages.TryGetValue(lastLanguage, out var fallbackLanguage) && fallbackLanguage != "English")
                    CacheRegisteredLocalization(fallbackLanguage, true);
                if (lastLanguage != "English")
                    CacheRegisteredLocalization("English");
            }

            foreach (string localizationRoot in localizationRoots)
            {
                LoadAndCacheLocalisationFiles(localizationRoot, lastLanguage);
                if (fallbackLanguages.TryGetValue(lastLanguage, out var fallbackLanguage) && fallbackLanguage != "English")
                    LoadAndCacheLocalisationFiles(localizationRoot, fallbackLanguage, true);
                if (lastLanguage != "English")
                    LoadAndCacheLocalisationFiles(localizationRoot, "English");
            }
        }

        private static void CacheRegisteredLocalization(string language, bool isFallback = false)
        {
            if (!registeredLanguages.TryGetValue(language, out var registeredLanguage))
                return;

            foreach (var registeredFile in registeredLanguage)
            {
                string file = registeredFile.Key;
                Dictionary<int, string> fileStrings = registeredFile.Value;

                if (language == "English")
                    cachedLocalisationsEnglish[file] = fileStrings;
                else if (isFallback)
                    cachedLocalisationsFallback[file] = fileStrings;
                else
                    cachedLocalisations[file] = fileStrings;
            }
        }

        private static void LoadAndCacheLocalisationFiles(string rootPath, string language, bool isFallback = false)
        {
            if (!Directory.Exists(Path.Combine(rootPath, language)))
                return;

            loggerInstance.Msg("Loading folder " + Path.Combine(rootPath, language));

            foreach (string file in Directory.GetFiles(Path.Combine(rootPath, language), "*.txt"))
            {
                Dictionary<int, string> fileStrings = new();

                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; ++i)
                    fileStrings[i] = lines[i];

                if (language == "English")
                    cachedLocalisationsEnglish[Path.GetFileNameWithoutExtension(file)] = fileStrings;
                else if (isFallback)
                    cachedLocalisationsFallback[Path.GetFileNameWithoutExtension(file)] = fileStrings;
                else
                    cachedLocalisations[Path.GetFileNameWithoutExtension(file)] = fileStrings;

                loggerInstance.Msg("Loaded " + Path.GetFileNameWithoutExtension(file));
            }
        }



        private static void TextTranslatePostfix(Localization_UIText __instance)
        {
            if (!__instance.data)
                return;
            loggerInstance.Msg($"[Data] {__instance.NameFile}");

            string text = __instance.NameFile;
            UnityEngine.UI.Text textComponent = __instance.GetComponent<UnityEngine.UI.Text>();
            string alreadyTranslatedText = textComponent.text;
            if (text == "Version")
                textComponent.text = alreadyTranslatedText + " - MiSlE " + MiSideSlaynashExtensionMelon.VERSION_DISPLAY;
                
        }

#if MISIDE
        private static void GetStringPostfix(string _name, int _string, ref string __result)
        {
            //loggerInstance.Msg($"[Text] {_name}:{_string}: {__result}");

            if (__result != "???")
                return;

            string localized;
            if (TryGetString(_name, _string, out localized))
            {
                __result = localized;
                return;
            }
        }
#endif

        internal static bool TryGetString(string name, int index, out string result)
        {
            if (lastLanguage != GameLanguage)
                ReloadLanguages();

            if (cachedLocalisations.TryGetValue(name, out var registeredLanguageFileStrings))
                if (index < registeredLanguageFileStrings.Count)
                {
                    result = registeredLanguageFileStrings[index];
                    return true;
                }

            if (cachedLocalisationsFallback.TryGetValue(name, out var fallbackLanguageFileStrings))
                if (index < fallbackLanguageFileStrings.Count)
                {
                    result = fallbackLanguageFileStrings[index];
                    return true;
                }

            if (cachedLocalisationsEnglish.TryGetValue(name, out var englishLanguageFileStrings))
                if (index < englishLanguageFileStrings.Count)
                {
                    result = englishLanguageFileStrings[index];
                    return true;
                }

            loggerInstance.Warning($"Failed to find localisation for {name}:{index}");

            result = string.Empty;
            return false;
        }
    }
}
