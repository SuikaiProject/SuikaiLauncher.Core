using System.Text.Json.Serialization;

namespace SuikaiLauncher.Core.Subassembly.Forge.JsonModels
{
    public class ForgeVersionBMCLAPI
    {
        [JsonPropertyName("_id")]
        public required string Id;
        public required int build;
        [JsonPropertyName("__v")]
        public required int v;
        public required List<FileTypeBMCLAPI> files;
        public required string version;
        public required string mdoified;
        public required string mcversion;

    }


    public class FileTypeBMCLAPI
    {
        public required string format;
        public required string category;
        public required string hash;
    }
    public class ForgeInstallProfile
    {
        [JsonPropertyName("_comment")]
        public required string Comment;
        public bool? hideExtract;
        public int? spec;
        public string? profile;
        public required string version;
        public required string path;
        public required string minecraft;
    }
}