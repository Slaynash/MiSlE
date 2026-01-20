using MelonLoader;
using MelonLoader.Logging;

#if MISIDE && MISIDEZERO
#error Invalid game preprocessor. Please define either MISIDE or MISIDEZERO
#elif MISIDE
[assembly: MelonInfo(typeof(MiSlE.MiSideSlaynashExtensionMelon), "MiSlE", "0.0.1", "Slaynash")]
[assembly: MelonGame("AIHASTO", "MiSideFull")]
#elif MISIDEZERO
[assembly: MelonInfo(typeof(MiSlE.MiSideSlaynashExtensionMelon), "MiSlE:Zero", "0.0.1", "Slaynash")]
#if MSZ_IL2CPP
[assembly: MelonGame("Voidway", "MiSide: Zero")]
#elif MSZ_MONO
[assembly: MelonGame("Voidway", "MiSide Zero")]
#endif
#else
#error Invalid game preprocessor. Please define either MISIDE or MISIDEZERO
#endif
[assembly: MelonColor(255, 200, 50, 200)]

namespace MiSlE
{
    public class MiSideSlaynashExtensionMelon : MelonMod
    {
        public const string VERSION = "0.0.1";
        public const string VERSION_DISPLAY = "0.01";
        public static readonly ColorARGB LogColor = ColorARGB.FromArgb(255, 200, 50, 200);

        public override void OnInitializeMelon()
        {
            LocalizationManager.Init(HarmonyInstance);
            ScenarioManager.Initialize(HarmonyInstance);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            switch (sceneName)
            {
#if MISIDE
                case "SceneMenu":
#else
                case "VSMenu":
#endif
                    {
                    MainMenuManager.OnMenuLoaded();
                    ScenarioManager.OnMenuLoaded();
                    break;
                }
            }
        }
    }
}
