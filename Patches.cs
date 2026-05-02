using System;
using System.Reflection;
using HarmonyLib;
using KMod;
using oni_vietnamese.Config;
using oni_vietnamese.Utils;
using TMPro;
using UnityEngine;

namespace oni_vietnamese {
	public class Patches : UserMod2 {
		private static readonly string ns = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
		public static string rootPath;
		private static FontConfig fc;
		internal static TMP_FontAsset font;
		private static bool fontLoadAttempted;

		public override void OnLoad(Harmony harmony) {
            harmony.PatchAll();
            rootPath = mod.file_source.GetRoot();
            ConfigManager.Instance.configPath = mod.file_source.GetRoot();
            fc = ConfigManager.Instance.LoadConfigFile();
        }

        private static void EnsureFontLoaded() {
            if (fontLoadAttempted) return;
            fontLoadAttempted = true;

            font = FontUtil.LoadFontAsset(fc);
            if (font == null) {
                Debug.LogWarning($"[{ns}] Tải font thất bại.");
            }
        }

        /// <summary>
        /// Directly inject our TMP_FontAsset into the game's font system,
        /// bypassing Localization.GetFont() which only finds Resources-loaded assets.
        /// </summary>
        internal static void ApplyFont() {
            if (font == null) return;

            Debug.Log($"[{ns}] Đang áp dụng font '{font.name}' vào game...");

            // 1. Set Localization.sFontAsset via reflection
            var sFontAssetField = typeof(Localization).GetField(
                "sFontAsset",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            if (sFontAssetField != null) {
                sFontAssetField.SetValue(null, font);
                Debug.Log($"[{ns}] Đã set Localization.sFontAsset = {font.name}");
            } else {
                Debug.LogWarning($"[{ns}] Không tìm thấy field Localization.sFontAsset");
                return;
            }

            // 2. Update all TextStyleSetting.sdfFont
            int styleCount = 0;
            foreach (var tss in Resources.FindObjectsOfTypeAll<TextStyleSetting>()) {
                if (tss != null) {
                    tss.sdfFont = font;
                    styleCount++;
                }
            }
            Debug.Log($"[{ns}] Đã cập nhật {styleCount} TextStyleSetting");

            // 3. Update all LocText components (SwapFont is internal, use reflection)
            bool isRTL = !fc.LeftToRight;
            int locTextCount = 0;
            var swapFontMethod = typeof(LocText).GetMethod(
                "SwapFont",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );
            foreach (var locText in Resources.FindObjectsOfTypeAll<LocText>()) {
                if (locText != null) {
                    try {
                        if (swapFontMethod != null) {
                            swapFontMethod.Invoke(locText, new object[] { font, isRTL });
                        } else {
                            locText.font = font;
                            locText.isRightToLeftText = isRTL;
                        }
                        locTextCount++;
                    } catch (Exception) {
                        // Some LocText may not be fully initialized yet
                    }
                }
            }
            Debug.Log($"[{ns}] Đã cập nhật {locTextCount} LocText");
        }

        [HarmonyPatch(typeof(Localization))]
        [HarmonyPatch(nameof(Localization.GetLocale))]
        [HarmonyPatch(new Type[] { typeof(string[]) })]
        public static class Localization_GetLocale_Patch {
            public static void Postfix(ref Localization.Locale __result) {
                try {
                    EnsureFontLoaded();

                    if (font == null) {
                        return;
                    }

                    var Language = fc.Code.Equals("zh") ? Localization.Language.Chinese : Localization.Language.Unspecified;
                    var Direction = fc.LeftToRight ? Localization.Direction.LeftToRight : Localization.Direction.RightToLeft;
                    __result = new Localization.Locale(Language, Direction, fc.Code, font.name);
                } catch (Exception ex) {
                    DebugUtil.LogWarningArgs(new object[] { ex });
                }
            }
        }

        /// <summary>
        /// Intercept SwapToLocalizedFont — always apply our font after game's swap.
        /// </summary>
        [HarmonyPatch(typeof(Localization))]
        [HarmonyPatch("SwapToLocalizedFont")]
        [HarmonyPatch(new Type[] { typeof(string) })]
        public static class Localization_SwapToLocalizedFont_Patch {
            public static void Postfix(string fontname) {
                try {
                    if (font == null) return;
                    Debug.Log($"[{ns}] SwapToLocalizedFont('{fontname}') intercepted — applying custom font");
                    ApplyFont();
                } catch (Exception ex) {
                    Debug.LogWarning($"[{ns}] SwapToLocalizedFont patch error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Apply font when Db initializes — this is when the game has loaded
        /// all resources and UI is ready. Covers the case where SwapToLocalizedFont
        /// is never called (e.g. no Font: header in .po file).
        /// </summary>
        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class Db_Initialize_Patch {
            public static void Postfix() {
                try {
                    EnsureFontLoaded();
                    if (font != null) {
                        Debug.Log($"[{ns}] Db.Initialize — applying font");
                        ApplyFont();
                    }
                } catch (Exception ex) {
                    Debug.LogWarning($"[{ns}] Db.Initialize patch error: {ex.Message}");
                }
            }
        }
    }
}
