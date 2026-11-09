namespace zResourcePrecacher7GC.Config;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public sealed class PluginConfig
{
    [JsonPropertyName("Log")]
    public bool Log { get; set; } = true;

    [JsonPropertyName("VerboseLog")]
    public bool VerboseLog { get; set; } = false;

    [JsonPropertyName("ScanAssetsDirectory")]
    public bool ScanAssetsDirectory { get; set; } = true;

    [JsonPropertyName("AssetsDirectory")]
    public string AssetsDirectory { get; set; } = "Assets";

    [JsonPropertyName("ReloadPermission")]
    public string ReloadPermission { get; set; } = "admin.precache.reload";

    [JsonPropertyName("StatusPermission")]
    public string StatusPermission { get; set; } = "admin.precache.status";

    [JsonPropertyName("ResourceExtensions")]
    public HashSet<string> ResourceExtensions { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
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
    };

    [JsonPropertyName("Discord")]
    public DiscordConfig Discord { get; set; } = new();

    [JsonPropertyName("Resources")]
    public HashSet<string> Resources { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("Version")]
    public int Version { get; set; } = 1;
}

public sealed class DiscordConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("WebhookUrl")]
    public string WebhookUrl { get; set; } = string.Empty;

    [JsonPropertyName("BotName")]
    public string BotName { get; set; } = "zResourcePrecacher7GC";

    [JsonPropertyName("AvatarUrl")]
    public string AvatarUrl { get; set; } = string.Empty;

    [JsonPropertyName("SendEmbed")]
    public bool SendEmbed { get; set; } = true;

    [JsonPropertyName("AttachLogFile")]
    public bool AttachLogFile { get; set; } = true;

    [JsonPropertyName("EmbedColorHex")]
    public string EmbedColorHex { get; set; } = "#2ECC71";

    [JsonPropertyName("SaveLocalLogFile")]
    public bool SaveLocalLogFile { get; set; } = true;

    [JsonPropertyName("LocalLogDirectory")]
    public string LocalLogDirectory { get; set; } = "logs/precache";
}
