using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


#if !MSZ_MONO
using Il2Cpp;
#endif
#if MISIDEZERO
#if MSZ_IL2CPP
using Il2CppTMPro;
#else
using TMPro;
#endif
#endif

namespace MiSlE
{
    internal static class MainMenuManager
    {
        internal static RectTransform ScenarioContainer {  get; private set; }
        internal static RectTransform FirstLocationButton {  get; private set; }


        private static MelonLogger.Instance loggerInstance = new MelonLogger.Instance("MiSlE:MainMenu", MiSideSlaynashExtensionMelon.LogColor);

        private static bool isAddonMenuShown = false;

        internal static void OnMenuLoaded()
        {
            PopulateMenu();
        }


#if MISIDE
        private static void PopulateMenuMiSide()
        {
            // Version
            GameObject textversion = GameObject.Find("MenuGame/Canvas/NameGame/TextVersion");
            if (textversion == null) { loggerInstance.Error("Failed to find TextVersion"); return; }

            // World loader
            GameObject loadmenu = GameObject.Find("MenuGame/Canvas/FrameMenu/Location Load");
            if (loadmenu == null) { loggerInstance.Error("Failed to find Load menu"); return; }

            /* // Original idea, specific menu inside the "load" menu to select an addon map

            GameObject loadModdedMenu = GameObject.Instantiate(loadmenu, loadmenu.transform);
            loadModdedMenu.name = "Location Load Modded";

            {
                Transform backButton = loadmenu.transform.Find("Button Back");
                if (backButton == null) { LoggerInstance.Error("Failed to find Back button"); return; }
                GameObject moddedMapsToggleButton = GameObject.Instantiate(backButton.gameObject, backButton.parent);
                moddedMapsToggleButton.name = "Button Addons";
                RectTransform moddedMapsToggleButtonRT = moddedMapsToggleButton.GetComponent<RectTransform>();
                moddedMapsToggleButtonRT.anchoredPosition = new Vector3(0, -595, 0);
                UnityEngine.UI.Text moddedMapsToggleButtonText = moddedMapsToggleButton.transform.Find("Text").GetComponent<UnityEngine.UI.Text>();
                ScheduleTranslation(moddedMapsToggleButtonText, "АДДОНЫ");

                loadmenu.transform.Find("Scroll View").GetComponent<RectTransform>().sizeDelta = new Vector2(500, 650);

                MenuNextLocation moddedMapsToggleButtonMNL = moddedMapsToggleButton.GetComponent<MenuNextLocation>();
                moddedMapsToggleButtonMNL.nextLocation = loadModdedMenu;
                moddedMapsToggleButtonMNL.changeCase = loadModdedMenu.transform.Find("Button Back").gameObject;
                //ButtonMouseClick moddedMapsToggleButtonBMC = moddedMapsToggleButton.GetComponent<ButtonMouseClick>();
                //moddedMapsToggleButtonBMC.eventClick.RemoveAllListeners();

                MenuLocation loadmenuML = loadmenu.GetComponent<MenuLocation>();
                loadmenuML.objects.Insert(loadmenuML.objects.Count - 1, moddedMapsToggleButtonRT);

            }

            {
                // TODO Modded menu
                Transform backButton = loadModdedMenu.transform.Find("Button Back");

                MenuNextLocation backButtonMNL = backButton.GetComponent<MenuNextLocation>();
                backButtonMNL.nextLocation = GameObject.Find("MenuGame/Canvas/FrameMenu/Location Load");
                backButtonMNL.changeCase = GameObject.Find("MenuGame/Canvas/FrameMenu/Location Load/Button Addons");

                Transform addonListTransform = loadModdedMenu.transform.Find("Scroll View/Viewport/Content");
                for (int i = addonListTransform.childCount - 1; i >= 0; --i)
                    GameObject.Destroy(addonListTransform.GetChild(i).gameObject);

                MenuLocation loadmenuML = loadmenu.GetComponent<MenuLocation>();
                loadmenuML.objects.Clear();
                for (int i = 0; i < loadmenuML.transform.childCount; i++)
                    loadmenuML.objects.Add(loadmenuML.transform.GetChild(i).GetComponent<RectTransform>()); // Extremely slow
                loadmenuML.buttonBack = backButton.gameObject;
            }

            */
            // next idea, only change the viewport --'

            {
                Transform backButton = loadmenu.transform.Find("Button Back");
                if (backButton == null) { loggerInstance.Error("Failed to find Back button"); return; }
                GameObject moddedMapsToggleButton = GameObject.Instantiate(backButton.gameObject, backButton.parent);
                moddedMapsToggleButton.name = "Button Addons";
                RectTransform moddedMapsToggleButtonRT = moddedMapsToggleButton.GetComponent<RectTransform>();
                moddedMapsToggleButtonRT.anchoredPosition = new Vector3(0, -595, 0);
                UnityEngine.UI.Text moddedMapsToggleButtonText = moddedMapsToggleButton.transform.Find("Text").GetComponent<UnityEngine.UI.Text>();
                moddedMapsToggleButtonText.GetComponent<Localization_UIText>().NameFile = "MiSlE";
                moddedMapsToggleButtonText.GetComponent<Localization_UIText>().StringNumber = 1;

                GameObject.Destroy(moddedMapsToggleButton.GetComponent<MenuNextLocation>());

                MenuLocation loadmenuML = loadmenu.GetComponent<MenuLocation>();
                loadmenuML.objects.Insert(loadmenuML.objects.Count - 1, moddedMapsToggleButtonRT);

                RectTransform scrollViewRT = loadmenu.transform.Find("Scroll View").GetComponent<RectTransform>();
                scrollViewRT.sizeDelta = new Vector2(500, 650);

                RectTransform scrollviewModdedRT = GameObject.Instantiate(scrollViewRT, scrollViewRT.parent);
                ScenarioContainer = scrollviewModdedRT.Find("Viewport/Content").GetComponent<RectTransform>();
                FirstLocationButton = scrollViewRT.Find("Viewport/Content").GetChild(0).GetComponent<RectTransform>();

                Transform addonListTransform = scrollviewModdedRT.Find("Viewport/Content");
                for (int i = addonListTransform.childCount - 1; i >= 0; --i)
                    GameObject.Destroy(addonListTransform.GetChild(i).gameObject);
                scrollviewModdedRT.gameObject.SetActive(false);

                // Group them under the same parent
                GameObject scrollviewsParent = new GameObject("ScrollviewsParent");
                scrollviewsParent.transform.SetParent(loadmenu.transform);
                RectTransform scrollviewsParentRT = scrollviewsParent.GetComponent<RectTransform>() ?? scrollviewsParent.AddComponent<RectTransform>();
                scrollviewsParentRT.anchorMin = new Vector2(0, 1);
                scrollviewsParentRT.anchorMax = new Vector2(0, 1);
                scrollviewsParentRT.pivot = new Vector2(0, 1);
                scrollviewsParentRT.position = scrollViewRT.position;
                scrollViewRT.SetParent(scrollviewsParentRT, true);
                scrollviewModdedRT.SetParent(scrollviewsParentRT, true);
                for (int i = 0; i < loadmenuML.objects.Count; ++i)
                    if (loadmenuML.objects[i] == scrollViewRT)
                        loadmenuML.objects[i] = scrollviewsParentRT;
                //scrollviewModdedRT.GetComponent<UI_Colors>().Reload();

                ButtonMouseClick moddedMapsToggleButtonBMC = moddedMapsToggleButton.GetComponent<ButtonMouseClick>();
                moddedMapsToggleButtonBMC.eventClick = new UnityEvent();
                moddedMapsToggleButtonBMC.eventClick.AddListener((UnityAction)(() =>
                {
                    // TODO toggle the addon menu properly (+ animate)
                    if (!isAddonMenuShown)
                    {
                        scrollviewModdedRT.gameObject.SetActive(true);
                        scrollViewRT.gameObject.SetActive(false);
                    }
                    else
                    {
                        scrollviewModdedRT.gameObject.SetActive(false);
                        scrollViewRT.gameObject.SetActive(true);
                    }
                    isAddonMenuShown = !isAddonMenuShown;
                }));
            }

        }
#else
        private static void PopulateMenuMSZ()
        {
            // Version
            GameObject textversion = GameObject.Find("Canvas/Game Name/Version");
            if (textversion == null) { loggerInstance.Error("Failed to find TextVersion"); return; }

            textversion.GetComponent<TextMeshProUGUI>().text += " - MiSlE " + MiSideSlaynashExtensionMelon.VERSION_DISPLAY; // TODO use localizations once implemented

            // For now MSZ doesn't have a "Load" menu, so let's just roll with an Addon menu

            GameObject mainmenu = GameObject.Find("Canvas/Menus/Main Menu");
            if (mainmenu == null) { loggerInstance.Error("Failed to find Main Menu"); return; }

            // Let's dusplicate the language menu, since that's the only one with a scrollview

            GameObject languagemenu = GameObject.Find("Canvas/Menus/Language Menu");
            if (languagemenu == null) { loggerInstance.Error("Failed to find Language Menu"); return; }

            GameObject addonsMenu = GameObject.Instantiate(languagemenu, languagemenu.transform.parent);
            addonsMenu.name = "Addons Menu";
            Transform addonsText = addonsMenu.transform.Find("LanguageText");
            addonsText.name = "AddonsText";
            addonsText.GetComponent<Text>().text = "ADDONS";
            addonsText.Find("Icon").gameObject.SetActive(false);

            Transform addonListTransform = addonsMenu.transform.Find("Scroll View/Viewport/Content");
            ScenarioContainer = addonListTransform.GetComponent<RectTransform>();
            FirstLocationButton = languagemenu.transform.Find("Scroll View/Viewport/Content").GetChild(0).GetComponent<RectTransform>();

            for (int i = addonListTransform.childCount - 1; i >= 0; --i)
                GameObject.Destroy(addonListTransform.GetChild(i).gameObject);

            // Update the "Back" button actions
            UIButtonCore addonsMenuBackUIBC = addonsMenu.transform.Find("Back").GetComponent<UIButtonCore>();
            addonsMenuBackUIBC.onClick = new UnityEvent();
            addonsMenuBackUIBC.onClick.AddListener((UnityAction)addonsMenu.GetComponent<UIGroupController>().HideGroup);
            addonsMenuBackUIBC.onClick.AddListener((UnityAction)GameObject.Find("Main Camera").GetComponent<SimpleCameraMover>().MoveToStart);
            addonsMenuBackUIBC.onClick.AddListener((UnityAction)GameObject.Find("Canvas/Character").GetComponent<UIGroupController>().ShowGroup);
            addonsMenuBackUIBC.onClick.AddListener((UnityAction)mainmenu.GetComponent<UIGroupController>().ShowGroup);

            // Duplicate the start button as "addon" right under "start"

            Transform buttonStart = mainmenu.transform.Find("Start");
            if (buttonStart == null) { loggerInstance.Error("Failed to find Start button"); return; }
            GameObject moddedMapsButton = GameObject.Instantiate(buttonStart.gameObject, buttonStart.parent);
            moddedMapsButton.name = "Addons";
            moddedMapsButton.transform.Find("Text").GetComponent<Text>().text = "ADDONS";
            int moddedMapsButtonIndex = buttonStart.GetSiblingIndex() + 1;
            moddedMapsButton.transform.SetSiblingIndex(moddedMapsButtonIndex);
            
            // Shift all buttons starting from self to the bottom
            for (int i = 0; i < mainmenu.transform.childCount; ++i)
            {
                Transform child = mainmenu.transform.GetChild(i);
                if (child.transform.GetSiblingIndex() < moddedMapsButtonIndex)
                    continue;
                child.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, 78.6f);
            }

            moddedMapsButton.transform.Find("Icon").gameObject.SetActive(false);
            UIButtonCore moddedMapsButtonUIBC = moddedMapsButton.GetComponent<UIButtonCore>();
            moddedMapsButtonUIBC.onClick = new UnityEvent();
            moddedMapsButtonUIBC.onClick.AddListener((UnityAction)mainmenu.GetComponent<UIGroupController>().HideGroup);
            moddedMapsButtonUIBC.onClick.AddListener((UnityAction)GameObject.Find("Canvas/Character").GetComponent<UIGroupController>().HideGroup);
            moddedMapsButtonUIBC.onClick.AddListener((UnityAction)(() => GameObject.Find("Main Camera").GetComponent<SimpleCameraMover>().MoveToTarget(3)));
            moddedMapsButtonUIBC.onClick.AddListener((UnityAction)addonsMenu.GetComponent<UIGroupController>().ShowGroup);

        }
#endif

        private static void PopulateMenu()
        {
            // Reset values
            isAddonMenuShown = false;

#if MISIDE
            PopulateMenuMiSide();
#else
            PopulateMenuMSZ();
#endif
        }
    }
}
