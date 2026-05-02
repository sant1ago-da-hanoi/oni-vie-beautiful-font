# ONI Vietnamese Beautiful Font

Mod thay font cho **Oxygen Not Included** — hỗ trợ hiển thị tiếng Việt (và các ngôn ngữ khác) với font đẹp hơn font mặc định.

## Cách hoạt động

- Load font TMP (TextMeshPro) custom từ AssetBundle lúc runtime
- Dùng Harmony patch `Localization.GetLocale` để inject font vào locale system
- Đọc `config.json` để cấu hình font, ngôn ngữ, hướng text, scale

## Cài đặt

1. Download mod từ [Steam Workshop](#) hoặc clone repo này
2. Copy thư mục mod vào `Documents/Klei/OxygenNotIncluded/mods/Local/`
3. Bật mod trong game

## Cấu hình

Tạo file `config.json` trong thư mục mod:

```json
{
  "Filename": "font",
  "Code": "vi",
  "LeftToRight": true,
  "Scale": 1.0
}
```

| Field | Mô tả |
|-------|--------|
| `Filename` | Tên file AssetBundle chứa font (không cần extension) |
| `Code` | Mã ngôn ngữ (`vi`, `zh`, ...) |
| `LeftToRight` | Hướng text — `true` cho LTR, `false` cho RTL |
| `Scale` | Tỉ lệ font (1.0 = mặc định) |

## Build

Yêu cầu:
- .NET Framework 4.8
- Visual Studio 2019+ hoặc MSBuild
- Các DLL reference từ game ONI (đặt trong path tương ứng trong `.csproj`)

```bash
msbuild oni-vietnamese.csproj /p:Configuration=Debug
```

## License

MIT
