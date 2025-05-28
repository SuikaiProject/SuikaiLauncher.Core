namespace SuikaiLauncher.Core.Minecraft.JsonModels
{
    public class VersionList
    {
        public required VersionLatest latest { get; set; }
        public required List<Version> versions { get; set; }
    }

    public class VersionLatest
    {
        public required string release { get; set; }
        public required string snapshot { get; set; }
    }

    public class Version
    {
        public required string id { get; set; }
        public required string type { get; set; }
        public required string url { get; set; }
        public required string time { get; set; }
        public required string releaseTime { get; set; }
        public string? sha1 { get; set; }
        public int? complianceLevel { get; set; }
    }

    public class VersionJson
    {
        public Arguments? arguments { get; set; }
        public required AssetIndex assetIndex { get; set; }
        public string? assets { get; set; }
        public int? complianceLevel { get; set; }
        public required Dictionary<string, DownloadMeta> downloads { get; set; }
        public string? id { get; set; }
        public JavaVersion? javaVersion { get; set; }
        public required List<Library> libraries { get; set; }
        public required Logging logging { get; set; }
        public required string mainClass { get; set; }
        public string? minecraftArguments { get; set; }
        public required int minimumLauncherVersion { get; set; }
        public required string releaseTime { get; set; }
        public required string time { get; set; }
        public required string type { get; set; }
    }

    public class Arguments
    {
        public required dynamic game { get; set; }
    }

    public class AssetIndex
    {
        public required string id { get; set; }
        public required string sha1 { get; set; }
        public required long size { get; set; }
        public required long totalSize { get; set; }
        public required string url { get; set; }
    }

    public class DownloadMeta
    {
        public required string sha1 { get; set; }
        public required long size { get; set; }
        public required string url { get; set; }
    }

    public class JavaVersion
    {
        public string? component { get; set; }
        public int? majorVersion { get; set; }
    }

    public class Library
    {
        public required string name { get; set; }
        public required LibraryDownloads downloads { get; set; }
        public List<Rule>? rules { get; set; }
        public dynamic? natives { get; set; }
        public Extract? extract { get; set; }
    }

    public class LibraryDownloads
    {
        public Artifact? artifact { get; set; }
        public Dictionary<string, Artifact>? classifiers { get; set; }
    }

    public class Artifact
    {
        public required string path { get; set; }
        public required string sha1 { get; set; }
        public required long size { get; set; }
        public required string url { get; set; }
    }

    public class Extract
    {
        public required List<string> exclude { get; set; }
    }

    public class Rule
    {
        public required string action { get; set; }
        public Os? os { get; set; }
    }

    public class Os
    {
        public required string name { get; set; }
    }

    public class Logging
    {
        public required ClientLogging client { get; set; }
    }

    public class ClientLogging
    {
        public required string argument { get; set; }
        public required LoggingFile file { get; set; }
        public required string type { get; set; }
    }

    public class LoggingFile
    {
        public required string id { get; set; }
        public required string sha1 { get; set; }
        public required long size { get; set; }
        public required string url { get; set; }
    }

    public class FileInfo
    {
        public required string hash { get; set; }
        public required long size { get; set; }
    }

    public class Objects
    {
        public required Dictionary<string, FileInfo> objects { get; set; }
    }
}