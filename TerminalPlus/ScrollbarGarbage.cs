using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static TerminalPlus.TerminalPatches;
using static UnityEngine.UI.ScrollRect;
using System.Diagnostics;

namespace TerminalPlus
{
    partial class Nodes
    {
        public static int customSens = 136;
        public static bool terminalKeyPressed = false;
        public static float currentScrollPosition = 1f;
        public static int currentStep = 0;
        public static float stepValue = 0f;
        public static int loopCount = 0;
        public static List<TerminalNode> testNodes = new List<TerminalNode>();

        //----------------------------------------------------------------------------

        [HarmonyPatch(typeof(Scrollbar), "Set")]
        [HarmonyPostfix]
        public static void Ssdfdwesasd(float input, bool sendCallback)
        {
            if (terminal != null && terminal.terminalInUse)
            {
                StackTrace stackTrace = new StackTrace();

                if (stackTrace.GetFrame(3).GetMethod().Name == "MoveNext") loopCount++;
                else if (loopCount >= 4) loopCount = 0;
            }
        }

        // ----------------------------------------------------------------------------

        [HarmonyPatch(typeof(Terminal), "Update")]
        [HarmonyPostfix]
        public static void TUPatch(ref float ___timeSinceLastKeyboardPress, Terminal __instance)
        {
            if (__instance.terminalInUse)
            {
                __instance.scrollBarCanvasGroup.alpha = 1;
                terminalKeyPressed = ___timeSinceLastKeyboardPress < 0.08;
                if (timeSinceSubmit < 0.07) __instance.scrollBarVertical.value = 1f;
            }
            timeSinceSubmit += Time.deltaTime;
        }

        [HarmonyPatch(typeof(Terminal), "QuitTerminal")]
        [HarmonyPostfix]
        public static void QTPatch(Terminal __instance)
        {
            __instance.scrollBarVertical.value = currentScrollPosition = 1f;
        }

        [HarmonyPatch(typeof(Terminal), "BeginUsingTerminal")]
        [HarmonyPrefix]
        public static void ResetSubmitPatch()
        {
            timeSinceSubmit = 0f;
        }

        // ----------------------------------------------------------------------------

        [HarmonyPatch(typeof(ScrollRect), "UpdateScrollbars")]
        [HarmonyPostfix]
        public static void ScrollTest(Vector2 offset, ref Bounds ___m_ContentBounds, ref Bounds ___m_ViewBounds, ref RectTransform ___m_Content, ref Vector2 ___m_PrevPosition, ScrollRect __instance)
        {
            if (terminal != null && terminal.terminalInUse)
            {
                __instance.verticalScrollbarVisibility = ScrollbarVisibility.AutoHideAndExpandViewport;

                float scrollbarSize = ___m_ContentBounds.size.y - ___m_ViewBounds.size.y;


                int totalSteps = customSens != 0 && Mathf.RoundToInt(scrollbarSize / Mathf.Abs(customSens)) > 1 ? Mathf.CeilToInt(scrollbarSize / Mathf.Abs(customSens)) : 1;


                if (scrollbarSize < 200f && scrollbarSize > 0f) totalSteps = Mathf.CeilToInt(scrollbarSize / 65);
                else if (totalSteps <= 1 && scrollbarSize > 0f) totalSteps = 2;
                if (totalSteps < 1) totalSteps = 1;

                if (___m_PrevPosition != null && Mathf.RoundToInt(___m_PrevPosition.y) != Mathf.RoundToInt(___m_Content.anchoredPosition.y))
                {
                    if ((___m_Content.anchoredPosition.y - ___m_PrevPosition.y) > 0) currentStep++;
                    else if ((___m_Content.anchoredPosition.y - ___m_PrevPosition.y) < 0) currentStep--;
                }

                Mathf.Clamp(currentStep, 0, totalSteps);

                if (timeSinceSubmit <= 0.08f || loopCount > 0)
                {
                    currentScrollPosition = 1f;
                    currentStep = loopCount = 0;
                    ___m_Content.anchoredPosition = Vector2.zero;
                }
                else if (terminalKeyPressed)
                {
                    currentScrollPosition = 0f;
                    currentStep = totalSteps;
                }
                else
                {
                    currentScrollPosition = Mathf.Clamp01(1f - ((float)currentStep / totalSteps));
                }

                __instance.verticalNormalizedPosition = currentScrollPosition;
            }
        }
    }
}