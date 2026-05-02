using System;
using System.IO;
using System.Reflection;
using oni_vietnamese.Config;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace oni_vietnamese.Utils {
	public static class FontUtil {
        public static TMP_FontAsset LoadFontAsset(FontConfig config) {
            TMP_FontAsset tmpFont = null;
            try {
                var fontPath = Path.Combine(ConfigManager.Instance.configPath, "Fonts", config.Filename);

                if (!File.Exists(fontPath)) {
                    Debug.LogWarning($"[ONI Tiếng Việt] Không tìm thấy file font: {fontPath}");
                    return null;
                }

                Debug.Log($"[ONI Tiếng Việt] Đang tải font từ: {fontPath}");

                // Create Font object via reflection (Internal_CreateFontFromPath is private)
                var font = new Font();
                var createMethod = typeof(Font).GetMethod(
                    "Internal_CreateFontFromPath",
                    BindingFlags.Static | BindingFlags.NonPublic
                );

                if (createMethod != null) {
                    Debug.Log("[ONI Tiếng Việt] Dùng Internal_CreateFontFromPath");
                    createMethod.Invoke(null, new object[] { font, fontPath });
                } else {
                    // Fallback: try loading font face via FontEngine and use empty Font
                    Debug.LogWarning("[ONI Tiếng Việt] Internal_CreateFontFromPath không tìm thấy, thử FontEngine");
                }

                // Load font face in FontEngine (required for TMP atlas generation)
                var loadResult = FontEngine.LoadFontFace(fontPath);
                if (loadResult != FontEngineError.Success) {
                    Debug.LogWarning($"[ONI Tiếng Việt] FontEngine.LoadFontFace thất bại: {loadResult}");
                    return null;
                }

                Debug.Log("[ONI Tiếng Việt] FontEngine.LoadFontFace thành công");

                // Create TMP_FontAsset
                tmpFont = TMP_FontAsset.CreateFontAsset(
                    font,
                    90,    // sampling point size
                    9,     // atlas padding
                    GlyphRenderMode.SDFAA,
                    1024,  // atlas width
                    1024   // atlas height
                );

                if (tmpFont == null) {
                    Debug.LogWarning("[ONI Tiếng Việt] CreateFontAsset(Font) trả về null, thử CreateFontAsset(string)");

                    // Try the string-path overload if it exists
                    var createFromPath = typeof(TMP_FontAsset).GetMethod(
                        "CreateFontAsset",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(string), typeof(int), typeof(int), typeof(GlyphRenderMode), typeof(int), typeof(int) },
                        null
                    );

                    if (createFromPath != null) {
                        Debug.Log("[ONI Tiếng Việt] Tìm thấy CreateFontAsset(string), đang thử...");
                        tmpFont = (TMP_FontAsset)createFromPath.Invoke(null, new object[] {
                            fontPath, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024
                        });
                    }
                }

                if (tmpFont == null) {
                    Debug.LogWarning("[ONI Tiếng Việt] Tất cả phương thức CreateFontAsset đều thất bại");
                    return null;
                }

                tmpFont.name = "VNFont-Runtime";
                Debug.Log($"[ONI Tiếng Việt] Font đã tạo thành công: {tmpFont.name}");

                // Enable dynamic atlas population so TMP renders glyphs on demand
                tmpFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;

                // Pre-populate atlas with ASCII + Vietnamese characters
                // TMP Dynamic mode needs characters added explicitly for runtime-created fonts
                string charsToAdd =
                    // ASCII printable
                    " !\"#$%&'()*+,-./0123456789:;<=>?@" +
                    "ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`" +
                    "abcdefghijklmnopqrstuvwxyz{|}~" +
                    // Vietnamese vowels with diacritics
                    "ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚÝàáâãèéêìíòóôõùúýĂăĐđĨĩŨũƠơƯư" +
                    "ẠạẢảẤấẦầẨẩẪẫẬậẮắẰằẲẳẴẵẶặẸẹẺẻẼẽẾếỀềỂểỄễỆệ" +
                    "ỈỉỊịỌọỎỏỐốỒồỔổỖỗỘộỚớỜờỞởỠỡỢợỤụỦủỨứỪừỬửỮữỰự" +
                    "ỲỳỴỵỶỷỸỹ";

                bool addResult = tmpFont.TryAddCharacters(charsToAdd, out string missingChars);
                int charCount = tmpFont.characterTable?.Count ?? 0;
                int glyphCount = tmpFont.glyphTable?.Count ?? 0;
                Debug.Log($"[ONI Tiếng Việt] TryAddCharacters: success={addResult}, chars={charCount}, glyphs={glyphCount}");
                if (!string.IsNullOrEmpty(missingChars)) {
                    Debug.LogWarning($"[ONI Tiếng Việt] Missing chars ({missingChars.Length}): {missingChars.Substring(0, Math.Min(50, missingChars.Length))}");
                }

                // Log atlas state after population
                if (tmpFont.atlasTextures != null && tmpFont.atlasTextures.Length > 0) {
                    var tex = tmpFont.atlasTextures[0];
                    Debug.Log($"[ONI Tiếng Việt] Atlas after populate: {tex?.width}x{tex?.height}, format={tex?.format}");
                }

                // Apply scale from config
                var faceInfo = tmpFont.faceInfo;
                faceInfo.scale = config.Scale;
                tmpFont.faceInfo = faceInfo;

            } catch (Exception e) {
                Debug.LogError($"[ONI Tiếng Việt] Lỗi khi tải font: {e.Message}\n{e.StackTrace}");
            }

            return tmpFont;
        }
    }
}
