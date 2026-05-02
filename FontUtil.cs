using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace oni_vietnamese.Utils {
	public static class FontUtil {
        private static readonly string ns = "[ONI Tiếng Việt]";

        // Vietnamese characters to pre-populate in atlas
        private static readonly string vietnameseChars =
            " !\"#$%&'()*+,-./0123456789:;<=>?@" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`" +
            "abcdefghijklmnopqrstuvwxyz{|}~" +
            "ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚÝàáâãèéêìíòóôõùúýĂăĐđĨĩŨũƠơƯư" +
            "ẠạẢảẤấẦầẨẩẪẫẬậẮắẰằẲẳẴẵẶặẸẹẺẻẼẽẾếỀềỂểỄễỆệ" +
            "ỈỉỊịỌọỎỏỐốỒồỔổỖỗỘộỚớỜờỞởỠỡỢợỤụỦủỨứỪừỬửỮữỰự" +
            "ỲỳỴỵỶỷỸỹ";

        /// <summary>
        /// Load a font file (.otf/.ttf) and create a TMP_FontAsset with pre-populated Vietnamese glyphs.
        /// </summary>
        public static TMP_FontAsset LoadFont(string fontPath, string displayName, float scale = 1.0f) {
            try {
                if (!File.Exists(fontPath)) {
                    Debug.LogWarning($"{ns} Không tìm thấy file font: {fontPath}");
                    return null;
                }

                // Create Font object via reflection
                var font = new Font();
                var createMethod = typeof(Font).GetMethod(
                    "Internal_CreateFontFromPath",
                    BindingFlags.Static | BindingFlags.NonPublic
                );

                if (createMethod != null) {
                    createMethod.Invoke(null, new object[] { font, fontPath });
                } else {
                    Debug.LogWarning($"{ns} Internal_CreateFontFromPath không tìm thấy");
                    return null;
                }

                // Load font face in FontEngine
                var loadResult = FontEngine.LoadFontFace(fontPath);
                if (loadResult != FontEngineError.Success) {
                    Debug.LogWarning($"{ns} FontEngine.LoadFontFace thất bại cho {displayName}: {loadResult}");
                    return null;
                }

                // Create TMP_FontAsset
                var tmpFont = TMP_FontAsset.CreateFontAsset(
                    font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024
                );

                if (tmpFont == null) {
                    Debug.LogWarning($"{ns} CreateFontAsset thất bại cho {displayName}");
                    return null;
                }

                tmpFont.name = displayName;
                tmpFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;

                // Pre-populate atlas with Vietnamese characters
                tmpFont.TryAddCharacters(vietnameseChars, out string missing);
                int charCount = tmpFont.characterTable?.Count ?? 0;
                Debug.Log($"{ns} Font '{displayName}': {charCount} chars loaded");

                // Apply scale
                if (Math.Abs(scale - 1.0f) > 0.001f) {
                    var faceInfo = tmpFont.faceInfo;
                    faceInfo.scale = scale;
                    tmpFont.faceInfo = faceInfo;
                }

                return tmpFont;
            } catch (Exception e) {
                Debug.LogError($"{ns} Lỗi khi tải font {displayName}: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }
    }
}
