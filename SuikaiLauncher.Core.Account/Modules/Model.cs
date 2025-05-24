using System.Text.Json.Serialization;


namespace SuikaiLauncher.Core.Account.JsonModel
{
    public class YggdrasilUserAuth
    {
        public required string accessToken;
        public string? clientToken;
        public List<Profile?>? availableProfiles;
        public Profile? selectedProfile;
    }
    public class YggdrasilAuthError
    {
        public dynamic? errorMessage;
        public dynamic? Message;
    }
    public class Profile
    {
        public required string name;
        public required string id;
        public required List<PlayerProperties> Properties;
    }
    public class PlayerProperties
    {
        public required string name;
        public required string value;


        public string? signature;
    }
    public class XboxLiveAuth
    {
        public required string IssueInstant;
        public required string NotAfter;
        public required string Token;
        public required DisplayClaims DisplayClaims;
    }
    public class DisplayClaims
    {
        public required List<Xui> xui;
    }
    public class Xui
    {
        public required string uhs;
    }
    public class MinecraftOfficialYggdrasil
    {
        public required string username;
        public List<dynamic>? roles;
        [JsonPropertyName("access_token")]
        public required string accessToken;
        [JsonPropertyName("token_type")]
        public required string TokenType;
        [JsonPropertyName("expires_in")]
        public required long ExpiredIn;
    }
    public class Ownership
    {
        public required List<dynamic?> items;
        public required string signature;
        public required string keyId;
    }
    public class MinecraftUserProfile
    {
        public required string id;
        public required string name;
        public required List<MinecraftProfileSkin> skins;
        public required List<MinecraftProfileSkin> capes;
    }
    public class MinecraftProfileSkin
    {
        public required string id;
        public required string state;
        public string? variant;
        public required string url;
        public required string alias;
    }
    public class Texture
    {
        public required long timestamp;
        public required string profileId;
        public required string profileName;
        public required Textures textures;
    }
    public class Textures {
        [JsonPropertyName("skin")]
        public Skin? Skin;
        [JsonPropertyName("cape")]
        public Skin? Cape;
    }
    public class Skin {
        public required string url;
        public Metadata? metadata;
    }
    public class Metadata {
        public required string model;
    }
}