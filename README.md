# zResourcePrecacher7GC for Counter-Strike 2 (SwiftlyS2)

An ultra-optimized, high-performance CS2 resource precacher plugin for **SwiftlyS2**. Automatically indexes workshop & custom VPK packages, registers engine precache items, logs detailed results, and sends execution summaries to **Discord via Webhooks** with a downloadable `[PASS]` log report file.

---

## Features

- Official ValvePak Integration: Powered by SteamDatabase.ValvePak for robust and complete VPK format parsing, bundled with required runtime dependencies.
- High Performance (Zero-Lag): Pre-compiled path cache, normalized forward slashes `/`, and consolidated logging to eliminate main-thread hitches during map changes.
- Detailed [PASS] Report: Generates structured execution logs with execution duration, map name, VPK package counts, and individual entries marked as `[PASS]`, `[WARN]`, or `[FAIL]`.
- Discord Webhook Integration: Asynchronously sends rich embeds to your Discord channel along with the attached `.txt` report file without affecting server tick rates.
- Fully Configurable (No Hardcoded Values): All supported file extensions, directories, permissions, and webhook options can be customized in `config.jsonc`.
- Dual Localization: Native English (`en.jsonc`) and Vietnamese (`vn.jsonc`) translations with prefix support following SwiftlyS2 standards.
- SwiftlyS2 Native Permissions: Admin commands secured by `admin.precache.reload` and `admin.precache.status`.

---

## Installation

1. Download the latest release `zResourcePrecacher7GC.zip`.
2. Extract the archive into your SwiftlyS2 plugins directory:
   ```text
   addons/swiftlys2/plugins/zResourcePrecacher7GC/
   ```
3. Place any custom `.vpk` packages containing skins, agents, sounds, or particles into:
   ```text
   addons/swiftlys2/plugins/zResourcePrecacher7GC/Assets/
   ```
4. The plugin automatically generates `config.jsonc` upon initial startup.

---

## Configuration (`config.jsonc`)

```jsonc
{
  "zResourcePrecacher7GC": {
    "Log": true,
    "VerboseLog": false,
    "ScanAssetsDirectory": true,
    "AssetsDirectory": "Assets",
    "ReloadPermission": "admin.precache.reload",
    "StatusPermission": "admin.precache.status",
    "ResourceExtensions": [
      "vmdl", "vmdl_c",
      "vpcf", "vpcf_c",
      "vmat", "vmat_c",
      "vcompmat", "vcompmat_c",
      "vtex", "vtex_c",
      "vsnd", "vsnd_c",
      "vdata", "vdata_c",
      "vpost", "vpost_c",
      "vsurf", "vsurf_c",
      "vanmgrph", "vanmgrph_c",
      "vmix", "vmix_c",
      "vnmclip", "vnmclip_c",
      "vrman", "vrman_c",
      "vrr_c",
      "vsc",
      "vsmart", "vsmart_c",
      "vsnap", "vsnap_c",
      "vsndevts", "vsndevts_c",
      "vsndgrps",
      "vsndstck", "vsndstck_c",
      "vsvg", "vsvg_c",
      "vts", "vts_c",
      "vxml", "vxml_c"
    ],
    "Discord": {
      "Enabled": false,
      "WebhookUrl": "https://discord.com/api/webhooks/...",
      "BotName": "zResourcePrecacher7GC",
      "AvatarUrl": "",
      "SendEmbed": true,
      "AttachLogFile": true,
      "EmbedColorHex": "#2ECC71",
      "SaveLocalLogFile": true,
      "LocalLogDirectory": "logs/precache"
    },
    "Resources": [],
    "Version": 1
  }
}
```

---

## Commands & Permissions

| Command | In-Game Chat Alias | Permission | Description |
| :--- | :--- | :--- | :--- |
| `sw_precache_reload` | `!precache_reload` / `!resprecache_reload` | `admin.precache.reload` | Re-scans the `Assets/` directory, reloads `config.jsonc`, and rebuilds the resource cache. |
| `sw_precache_status` | `!precache_status` / `!resprecache_status` | `admin.precache.status` | Displays total prepared resources, scanned VPK count, and Discord webhook status. |
| `sw_precache_testwebhook` | `!precache_testwebhook` / `!resprecache_testwebhook` | `admin.precache.reload` | Dispatches a test precache execution report to Discord to verify webhook connectivity. |

*Note: Server Console (RCON) can execute commands at all times without requiring player permission checks.*

---

## Sample Precache Report (`.txt`)

When a map loads and precaching executes, a report like the following is generated and posted to Discord:

```text
================================================================================
           zResourcePrecacher7GC - Resource Precache Execution Log
================================================================================
Timestamp       : 2026-11-10 02:45:12 UTC
Server Name     : [VN] Competitive CS2 Server 128T
Map Name        : de_dust2
Elapsed Time    : 18 ms
Total Evaluated : 54
Passed (Precached) : 52
Warnings (Skipped) : 2
Failed (Errors)    : 0

Scanned VPK Packages:
  - Assets/custom_knives.vpk (30 items)
  - Assets/vip_models_dir.vpk (22 items)
================================================================================
[PASS] characters/models/custom/admin_ct.vmdl
[PASS] characters/models/custom/vip_t.vmdl
[PASS] materials/models/custom/knife_karambit.vmat
[PASS] soundevents/soundevents_custom.vsndevts
[PASS] particles/weapons/cs_weapons_custom.vpcf
[WARN] materials/models/custom/readme.txt (Unsupported extension: 'txt')
================================================================================
End of Precache Log.
```

---

## Building from Source

```bash
dotnet publish -c Release
```

The compiled plugin and ready-to-deploy zip archive will be generated at `build/zResourcePrecacher7GC.zip`.
