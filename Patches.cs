using System;
using System.Collections.Generic;
using System.IO;
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
		private static bool fontLoadAttempted;

		// Font assets for different game font families
		private static TMP_FontAsset graystrokeFont;
		private static TMP_FontAsset robotoRegularFont;
		private static TMP_FontAsset robotoBoldFont;
		private static TMP_FontAsset robotoItalicFont;
		private static TMP_FontAsset robotoBoldItalicFont;

		public override void OnLoad(Harmony harmony) {
            harmony.PatchAll();
            rootPath = mod.file_source.GetRoot();
            ConfigManager.Instance.configPath = mod.file_source.GetRoot();
            fc = ConfigManager.Instance.LoadConfigFile();
        }

        private static void EnsureFontLoaded() {
            if (fontLoadAttempted) return;
            fontLoadAttempted = true;

            var fontsDir = Path.Combine(ConfigManager.Instance.configPath, "Fonts");

            // Load GRAYSTROKE for heading fonts
            var graystrokePath = Path.Combine(fontsDir, fc.Filename);
            graystrokeFont = FontUtil.LoadFont(graystrokePath, "VNFont-Graystroke", fc.Scale);

            // Load RobotoCondensed variants for body text
            robotoRegularFont = FontUtil.LoadFont(
                Path.Combine(fontsDir, "RobotoCondensed-Regular.ttf"), "VNFont-Roboto-Regular");
            robotoBoldFont = FontUtil.LoadFont(
                Path.Combine(fontsDir, "RobotoCondensed-Bold.ttf"), "VNFont-Roboto-Bold");
            robotoItalicFont = FontUtil.LoadFont(
                Path.Combine(fontsDir, "RobotoCondensed-Italic.ttf"), "VNFont-Roboto-Italic");
            robotoBoldItalicFont = FontUtil.LoadFont(
                Path.Combine(fontsDir, "RobotoCondensed-BoldItalic.ttf"), "VNFont-Roboto-BoldItalic");

            int loaded = 0;
            if (graystrokeFont != null) loaded++;
            if (robotoRegularFont != null) loaded++;
            if (robotoBoldFont != null) loaded++;
            if (robotoItalicFont != null) loaded++;
            if (robotoBoldItalicFont != null) loaded++;
            Debug.Log($"[{ns}] Đã tải {loaded}/5 font");
        }

        /// <summary>
        /// Check if a game font is a GRAYSTROKE variant (heading/display font).
        /// </summary>
        private static bool IsGraystroke(string fontName) {
            return fontName.StartsWith("GRAYSTROKE", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if a TMP_FontAsset is one of our custom fonts.
        /// </summary>
        private static bool IsOurFont(TMP_FontAsset font) {
            return font == graystrokeFont || font == robotoRegularFont ||
                   font == robotoBoldFont || font == robotoItalicFont ||
                   font == robotoBoldItalicFont;
        }

        /// <summary>
        /// Get the replacement font for a body text font based on its style.
        /// </summary>
        private static TMP_FontAsset GetBodyReplacement(string gameFontName) {
            if (gameFontName.Contains("BoldItalic"))
                return robotoBoldItalicFont ?? robotoBoldFont ?? robotoRegularFont;
            if (gameFontName.Contains("Bold") || gameFontName.Contains("Economica"))
                return robotoBoldFont ?? robotoRegularFont;
            if (gameFontName.Contains("Italic"))
                return robotoItalicFont ?? robotoRegularFont;
            return robotoRegularFont;
        }

        internal static void ApplyFont() {
            // --- Phase 1: Add fallback to GRAYSTROKE fonts (heading/display) ---
            var allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            int fallbackCount = 0;
            foreach (var gameFont in allFonts) {
                if (gameFont == null || IsOurFont(gameFont)) continue;
                if (!IsGraystroke(gameFont.name)) continue;

                var fallbacks = gameFont.fallbackFontAssetTable;
                if (fallbacks != null && fallbacks.Contains(graystrokeFont)) continue;

                if (fallbacks == null)
                    gameFont.fallbackFontAssetTable = new List<TMP_FontAsset> { graystrokeFont };
                else
                    fallbacks.Add(graystrokeFont);
                fallbackCount++;
            }

            // --- Phase 2: Replace body text fonts directly on TextStyleSetting ---
            var allStyles = Resources.FindObjectsOfTypeAll<TextStyleSetting>();
            int styleCount = 0;
            foreach (var style in allStyles) {
                if (style == null || style.sdfFont == null) continue;
                if (IsOurFont(style.sdfFont)) continue;
                if (IsGraystroke(style.sdfFont.name)) continue;

                var replacement = GetBodyReplacement(style.sdfFont.name);
                if (replacement == null) continue;
                style.sdfFont = replacement;
                styleCount++;
            }

            // --- Phase 3: Replace body text fonts directly on LocText ---
            var allLocTexts = Resources.FindObjectsOfTypeAll<LocText>();
            bool isRTL = !fc.LeftToRight;
            int locTextCount = 0;
            foreach (var locText in allLocTexts) {
                if (locText == null) continue;
                var currentFont = locText.font;
                if (currentFont == null) continue;
                if (IsOurFont(currentFont)) continue;
                if (IsGraystroke(currentFont.name)) continue;

                var replacement = GetBodyReplacement(currentFont.name);
                if (replacement == null) continue;

                // Use reflection to call internal SwapFont(font, isRTL)
                try {
                    var swapMethod = typeof(LocText).GetMethod("SwapFont",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                        null, new Type[] { typeof(TMP_FontAsset), typeof(bool) }, null);
                    if (swapMethod != null) {
                        swapMethod.Invoke(locText, new object[] { replacement, isRTL });
                    } else {
                        locText.font = replacement;
                    }
                } catch {
                    locText.font = replacement;
                }
                locTextCount++;
            }

            // --- Phase 4: Set Localization.sFontAsset for new LocText created later ---
            try {
                var sFontField = typeof(Localization).GetField("sFontAsset",
                    BindingFlags.Static | BindingFlags.NonPublic);
                if (sFontField != null && robotoRegularFont != null) {
                    sFontField.SetValue(null, robotoRegularFont);
                }
            } catch (Exception ex) {
                Debug.LogWarning($"[{ns}] Không thể set sFontAsset: {ex.Message}");
            }

            if (fallbackCount > 0 || styleCount > 0 || locTextCount > 0)
                Debug.Log($"[{ns}] Applied: {fallbackCount} GRAYSTROKE fallbacks, {styleCount} TextStyleSettings, {locTextCount} LocTexts replaced");
        }

        [HarmonyPatch(typeof(Localization))]
        [HarmonyPatch(nameof(Localization.GetLocale))]
        [HarmonyPatch(new Type[] { typeof(string[]) })]
        public static class Localization_GetLocale_Patch {
            public static void Postfix(ref Localization.Locale __result) {
                try {
                    EnsureFontLoaded();

                    // Set locale for Vietnamese but keep font name empty —
                    // we inject via fallback, not via SwapToLocalizedFont
                    var Language = fc.Code.Equals("zh") ? Localization.Language.Chinese : Localization.Language.Unspecified;
                    var Direction = fc.LeftToRight ? Localization.Direction.LeftToRight : Localization.Direction.RightToLeft;
                    __result = new Localization.Locale(Language, Direction, fc.Code, "");
                } catch (Exception ex) {
                    DebugUtil.LogWarningArgs(new object[] { ex });
                }
            }
        }

        [HarmonyPatch(typeof(Localization))]
        [HarmonyPatch("SwapToLocalizedFont")]
        [HarmonyPatch(new Type[] { typeof(string) })]
        public static class Localization_SwapToLocalizedFont_Patch {
            public static void Postfix(string fontname) {
                try {
                    ApplyFont();
                } catch (Exception ex) {
                    Debug.LogWarning($"[{ns}] SwapToLocalizedFont patch error: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class Db_Initialize_Patch {
            public static void Postfix() {
                try {
                    EnsureFontLoaded();
                    ApplyFont();
                } catch (Exception ex) {
                    Debug.LogWarning($"[{ns}] Db.Initialize patch error: {ex.Message}");
                }
            }
        }
    }
}
