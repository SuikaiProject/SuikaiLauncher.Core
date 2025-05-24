
namespace SuikaiLauncher.Core.Minecraft.JsonModels
{
    public class VersionList
    {
        public required VersionLatest latest;
        public required List<Version> versions;
    }

    public class VersionLatest
    {
        public required string release;
        public required string snapshot;
    }
    public class Version
    {
        public required string id;
        public required string type;
        public required string url;
        public required string time;
        public required string releaseTime;
        public string? sha1;
        public int? complianceLevel;
    }

    public class VersionJson
    {
        public Arguments? arguments;
        public required AssetsIndex assetsIndex;
        public string? assets;
        public int? complianceLevel;
        public required MainDownloads downloads;
        public string? id;
        public JavaVersion? javaVersion;
        public required List<Downloads> libraries;
        public required Logging logging;
        public required string mainClass;
        public required string minimumLauncherVersiom;
        public required string releaseTime;
        public required string time;
        public required string type;
    }

    public class Arguments
    {
        public required dynamic game;
    }

    public class AssetsIndex
    {
        public required string id;
        public required string sha1;
        public required long size;
        public required long totalSize;
        public required string url;
    }

    public class DownloadMeta
    {
        public required string sha1;
        public required long size;
        public required string url;
    }


    public class MainDownloads
    {
        public required DownloadMeta client;
        public DownloadMeta? client_mapping;
        public DownloadMeta? server;
        public DownloadMeta? server_mapping;
    }
    public class JavaVersion
    {
        public string? component;
        public int? mojarVersion;
    }
    public class Downloads
    {
        public required Artifact artifact;
        public dynamic? classifiers;
        public dynamic? natives;
        public Extract? extract;
        public List<Rules?>? rules;

    }
    public class Artifact
    {
        public required string path;
        public required string sha1;
        public required long size;
        public required string url;
    }

    public class Native
    {
        public required string path;
        public required string sha1;
        public required long size;
        public required string url;
    }
    public class Extract
    {
        public required List<string> exclude;
    }

    public class Rules
    {
        public required string action;
        public Os? os;
    }
    public class Os
    {
        public required string name;
    }

    public class Logging
    {
        public required ClientLogging client;
    }
    public class ClientLogging
    {
        public required string argument;
        public required Logging file;
        public required string type;
    }
    public class LoggingFile
    {
        public required string id;
        public required string sha1;
        public required long size;
        public required string url;
    }
    public class FileInfo
    {
        public required string hash;
        public required long size;
    }
    public class Objects
    {
        public required Dictionary<string, FileInfo> objects;
    }

}