using System.Text.Json.Serialization;


namespace SuikaiLauncher.Core.Account.JsonModel
{
    public record OAuthDeviceCode
    {
        [JsonPropertyName("user_code")]
        public required string UserCode;
        [JsonPropertyName("device_code")]
        public required string DeviceCode;
        public required int interval;
        [JsonPropertyName("verification_uri")]
        public required string Verification;
        [JsonPropertyName("verification_uri_complete")]
        public string? VerificationComplete;
    }
    public class MinecraftRPLogin
    {
        public required string identityToken;
    }
    public record YggdrasilUserAuth
    {
        public required string accessToken;
        public string? clientToken;
        public List<Profile?>? availableProfiles;
        public Profile? selectedProfile;
    }
    public record YggdrasilAuthError
    {
        public dynamic? errorMessage;
        public dynamic? Message;
    }
    public record Profile
    {
        public required string name;
        public required string id;
        public required List<PlayerProperties> Properties;
    }
    public record PlayerProperties
    {
        public required string name;
        public required string value;


        public string? signature;
    }
    public record XboxLiveAuth
    {
        public required string IssueInstant;
        public required string NotAfter;
        public required string Token;
        public required DisplayClaims DisplayClaims;
    }
    public record DisplayClaims
    {
        public required List<Xui> xui;
    }
    public record Xui
    {
        public required string uhs;
    }
    public record MinecraftOfficialYggdrasil
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
    public record Ownership
    {
        public required List<dynamic?> items;
        public required string signature;
        public required string keyId;
    }
    public record MinecraftUserProfile
    {
        public required string id;
        public required string name;
        public required List<MinecraftProfileSkin> skins;
        public required List<MinecraftProfileSkin> capes;
    }
    public record MinecraftProfileSkin
    {
        public required string id;
        public required string state;
        public string? variant;
        public required string url;
        public required string alias;
    }
    public record Texture
    {
        public required long timestamp;
        public required string profileId;
        public required string profileName;
        public required Textures textures;
    }
    public record Textures {
        [JsonPropertyName("skin")]
        public Skin? Skin;
        [JsonPropertyName("cape")]
        public Skin? Cape;
    }
    public record Skin {
        public required string url;
        public Metadata? metadata;
    }
    public record Metadata {
        public required string model;
    }
    public class YggdrasilLogin
    {
        public required string username;
        public required string password;
        public bool requestUser = true;
        public Agent agent = new();
    }
    public record Agent
    {
        public string name = "minecraft";
        public int version = 1;
    }
}