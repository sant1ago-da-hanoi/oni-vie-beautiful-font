using System;
using System.IO;
using oni_vietnamese.Config;
using TMPro;
using UnityEngine;

namespace oni_vietnamese.Utils {
	public static class FontUtil {
        public static TMP_FontAsset LoadFontAsset(FontConfig config) {
            TMP_FontAsset font = null;
            AssetBundle ab = null;
            try {
                var platform = Application.platform == RuntimePlatform.WindowsPlayer ? "win" : "other";
                var assetPath = Path.Combine(ConfigManager.Instance.configPath, "Assets", platform, config.Filename);
                ab = AssetBundle.LoadFromFile(assetPath);

                if (ab == null) {
                    Debug.LogWarning("[ONI Tiếng Việt] Gặp lỗi khi tải font asset.");
                    return null;
                }

                font = ab.LoadAllAssets<TMP_FontAsset>()[0];
                font.fontInfo.Scale = config.Scale;

                if (Application.platform == RuntimePlatform.LinuxPlayer) {
                    font.material.shader = Resources.Load<TMP_FontAsset>("RobotoCondensed-Regular").material.shader;
                }
            } catch (Exception e) {
                Debug.LogError($"[FontLoader] {e.Message}");
            }

            AssetBundle.UnloadAllAssetBundles(false);
            return font;
        }
    }
}
