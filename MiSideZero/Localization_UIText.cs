#if MISIDEZERO
using MelonLoader;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace MiSlE.MiSideZero
{
    //[AddComponentMenu("Functions/Localization/Localization UI Text")]
#if MSZ_IL2CPP
    [RegisterTypeInIl2Cpp]
#endif
    public class Localization_UIText : MonoBehaviour
    {
        public bool EveryEnable;

        public string NameFile;

        public int StringNumber = 1;

        public int StringReserveNumber = 1;

        public bool GrandSymbol = true;

        public bool data;

        public bool dontAutoTranslate;

        public bool dontAutoFont;

        public bool fontPixel;

        public bool onEnable;

        //[SerializeField]
        private string add;

        //[SerializeField]
        private string reffect0;

        //[HideInInspector]
        public bool deactiveTextTranslate;

        //[HideInInspector]
        public bool fs;

#if MSZ_IL2CPP
        public Localization_UIText(IntPtr ptr) : base(ptr) { }
#endif

        public void OnEnable()
        {
            if (!dontAutoTranslate && fs && (EveryEnable || onEnable))
            {
                onEnable = false;
                TextTranslate();
            }
        }

        private void Start()
        {
            if (!dontAutoTranslate && !fs)
            {
                fs = true;
                TextTranslate();
            }
        }

        //[Button("Перевести", EButtonEnableMode.Always)]
        public void TextTranslate()
        {
            if (deactiveTextTranslate)
                return;

            fs = true;
            /* TODO
            if (GlobalGame.fontUse != null && !dontAutoFont)
            {
                GetComponent<Text>().font = fontPixel ? GlobalGame.fontUse : GlobalGame.fontPixelUse;
            }
            */

            string text = "";
            if (!data)
            {
                if (!LocalizationManager.TryGetString(NameFile, StringNumber - 1, out text))
                    LocalizationManager.TryGetString(NameFile, StringReserveNumber - 1, out text);
            }
            else
            {
                string[] lines = File.ReadAllLines($"{Application.absoluteURL}/Data/{NameFile}.txt");
                if (lines.Length > StringNumber - 1)
                    text = lines[StringNumber - 1];
            }

            if (text != "")
            {
                if (GrandSymbol)
                    text = text.ToUpper();
                text = text.Replace("[0]", reffect0);
                text += add;
            }
            else
                text = "???";

            GetComponent<Text>().text = text;

        }

        public void DestroyObject()
        {
            Destroy(gameObject);
        }
    }
}
#endif