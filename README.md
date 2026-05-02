# ONI Vietnamese Beautiful Font

Mod thay font cho **Oxygen Not Included** — hỗ trợ hiển thị tiếng Việt (và các ngôn ngữ khác) với font đẹp hơn font mặc định.

## Cách hoạt động

- Load font TMP (TextMeshPro) custom từ AssetBundle lúc runtime
- Dùng Harmony patch `Localization.GetLocale` để inject font vào locale system
- Đọc `config.json` để cấu hình font, ngôn ngữ, hướng text, scale

## Cài đặt

### Steam Workshop (khuyến nghị)

1. Đăng ký mod **[ONI Tiếng Việt](https://steamcommunity.com/sharedfiles/filedetails/?id=2574634278)** trên Steam Workshop
2. Bật mod trong game, khởi động lại
3. Vào **Settings → Language** chọn **Tiếng Việt**

### Cài thủ công (local mod)

1. Build DLL theo hướng dẫn bên dưới (hoặc tải từ [Releases](https://github.com/sant1ago-da-hanoi/oni-vie-beautiful-font/releases))
2. Tạo thư mục local mod và copy files vào:
   ```
   # macOS
   ~/Library/Application Support/unity.Klei.Oxygen Not Included/mods/Local/VieBeautifulFont/

   # Windows
   %USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\VieBeautifulFont\
   ```
   Cấu trúc thư mục:
   ```
   VieBeautifulFont/
   ├── oni-vietnamese.dll
   ├── mod_info.yaml
   ├── mod.yaml
   ├── config.json
   └── Assets/
       ├── other/
       │   └── font          ← AssetBundle cho macOS/Linux
       └── win/
           └── font          ← AssetBundle cho Windows
   ```
3. Mở game → **Mods** → bật **VieBeautifulFont** → khởi động lại
4. Vào **Settings → Language** chọn **Tiếng Việt**

> **Lưu ý:** Nếu đang dùng bản Workshop, tắt mod Workshop trước khi bật local mod để tránh conflict.

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
- .NET SDK 6.0+ (hoặc bất kỳ version nào hỗ trợ `netstandard2.1`)
- Game Oxygen Not Included đã cài qua Steam

Project tự detect đường dẫn game DLLs trên macOS, Windows, và Linux (Steam default paths). Nếu cài ở chỗ khác, set property `ONIManagedDir` khi build.

```bash
dotnet build oni-vietnamese.csproj
```

Output: `bin/Debug/netstandard2.1/oni-vietnamese.dll`

Custom game path:
```bash
dotnet build oni-vietnamese.csproj -p:ONIManagedDir="/path/to/OxygenNotIncluded_Data/Managed"
```

## License

MIT
