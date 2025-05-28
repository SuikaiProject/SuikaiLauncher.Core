using System.Text.Json.Serialization;

namespace SuikaiLauncher.Core.Mod.JsonModels
{
    // 各 Mod 平台 API Json 数据模型
    public class CFProjectData
    {
        public required int id;
        public required int gameId;
        public required string name;
        public required string slug;
        public required CFLinks links;
        public required string summary;
        public required int status;
        public required int downloadCount;
        public required bool isFeatured;
        public required string primaryCategoryId;
        public required List<CFCategories> categories;
        public required int classId;
        public required List<CFAuthor> authors;
        public required List<CFLogo?> logo;
        public required List<CFLogo?> screenshots;
        public required int mainFileId;
        public required List<CFLatestFile> latestFiles;
        public required List<CFLatestFileIndex> latestFilesIndexes;
        public required List<CFLatestFileIndex> latestEarlyAccessFilesIndexes;
        public required string dataCreated;
        public required string dataModified;
        public required string dataReleased;
        public required bool allowModDistribution;
        public required int gamePopularityRank;
        public required bool isAvailable;
        public required int thumbsUpCount;
        public required int rating;
    }
    public class CFSearchResult
    {
        public required List<CFProjectData> data;
        public required CFPagination pagination;
    }
    public class CFPagination
    {
        public required int index;
        public required int pageSize;
        public required int resultCount;
        public required int totalCount;

    }
    public class CFLatestFile
    {
        public required int id;
        public required int gameId;
        public required int modId;
        public required bool isAvailable;
        public required string displayName;
        public required string fileName;
        public required int releaseType;
        public required int fileStatus;
        public required List<CFHash> hashes;
        public required string fileDate;
        public required long fileLength;
        public required long downloadCount;
        public required long fileSizeOnDisk;
        public required string downloadUrl;
        public required List<string> gameVersions;
        public required List<CFGameVersion> sortableGameVersions;
        public required List<CFDependency> dependencies;
        public required bool exposeAsAlternative;
        public required int parentProjectFileId;
        public required int alternateFileId;
        public required bool isServerPack;
        public required int serverPackFileId;
        public required bool isEarlyAccessContent;
        public required string earlyAccessEndDate;
        public required int fileFingerprint;
        public required CFModules modules;
    }
    public class CFLatestFileIndex
    {
        public required string gameVersion;
        public required int fileId;
        public required string fileName;
        public required int releaseType;
        public required int gameVersionTypeId;
        public required int modLoader;
    }
    public class CFModules
    {
        public required string name;
        public required int fingerprint;
    }
    public class CFDependency
    {
        public required int modId;
        public required int relationType;
    }
    public class CFGameVersion
    {
        public required string gameVersionName;
        public required string gameVersionPadded;
        public required string gameVersionReleaseDate;
        public required int gameVersionTypeId;
    }
    public class CFHash
    {
        public required string value;
        public required int algo;
    }
    public class CFLogo
    {
        public required int id;
        public required int modId;
        public required string title;
        public required string description;
        public required string thumbnailUrl;
        public required string url;
    }
    public class CFAuthor
    {
        public required int id;
        public required string name;
        public required string url;
    }
    public class CFCategories
    {
        public required int id;
        public required int gameId;
        public required string name;
        public required string slug;
        public required string url;
        public string? iconUrl;
        public required string dateModified;
        public required bool isClass;
        public required int classId;
        public required int parentCategoryId;
        public required int displayIndex;
    }
    public class CFLinks
    {
        public required string websiteUrl;
        public required string wikiUrl;
        public required string issueUrl;
        public required string sourceUrl;
    }
    public class MRProjectData
    {
        public string? ModI18nzhName;
        [JsonPropertyName("project_id")]
        public required string ProjectId;
        [JsonPropertyName("project")]
        public required string ProjectType;
        public required string slug;
        public required string author;
        public required string title;
        public string? description;
        public required List<string> categories;
        [JsonPropertyName("display_categories")]
        public required List<string> DisplayCateories;
        public required List<string> versions;
        public required long downloads;
        public required long follows;
        [JsonPropertyName("icon_url")]
        public string? Icon;
        [JsonPropertyName("data_create")]
        public required string Create;
        [JsonPropertyName("data_modified")]
        public required string Modified;
        [JsonPropertyName("latest_version")]
        public required string Latest;
        public required string license;
        [JsonPropertyName("client_side")]
        public required string ClientRequired;
        [JsonPropertyName("server_side")]
        public required string ServerRequired;
        public required List<string> gallery;
        [JsonPropertyName("featured_gallery")]
        public List<string?>? FeaturedGallery;
        public int? color;
    }

    public class ModrinthSearchResult
    {
        public required List<MRProjectData?> hits;
        public int offset;
        public int limit;
        [JsonPropertyName("total_hits")]
        public int Total;
    }
}