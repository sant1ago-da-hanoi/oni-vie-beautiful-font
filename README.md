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
