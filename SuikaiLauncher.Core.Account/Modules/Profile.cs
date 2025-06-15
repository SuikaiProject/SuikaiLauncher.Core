using System.Text;
using SuikaiLauncher.Core.Base;

namespace SuikaiLauncher.Core.Account;
/// <summary>
/// 档案类
/// </summary>
public class Profile
{
    private static StringBuilder StrBuilder = new("*",30);
    /// <summary>
    /// 用户名
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// UUID
    /// </summary>
    public required string uuid  { get; set; }
    /// <summary>
    /// 访问令牌
    /// </summary>
    public required string accessToken { get; set; }
    /// <summary>
    /// 刷新令牌（可能为 null）
    /// </summary>
    public string? refreshToken { get; set; }
    /// <summary>
    /// 用户皮肤的下载地址，没有设置则为 null
    /// </summary>
    public string? Skin { get; set; }
    /// <summary>
    /// 披风下载地址，没有则为 null
    /// </summary>
    public string? Cape { get; set; }
    /// <summary>
    /// 账户类型
    /// </summary>
    public McLoginType LoginType {get; set;}
    /// <summary>
    /// 档案过期时间（Access Token）
    /// </summary>
    public long ExpiresIn { get; set; }
    /// <summary>
    /// 完全过期时间（Refresh Token）
    /// </summary>
    public long ExpiredAt { get; set; }
    /// <summary>
    ///  档案创建时间
    /// </summary>
    public long CreateAt { get; set; }

    public static string RemoveSecret(string SecretText)
    {
        return SecretText.Substring(0, 5) + StrBuilder + SecretText.Substring(SecretText.Length - 5,SecretText.Length -1);
    }
}
/// <summary>
/// 档案管理器
/// </summary>
public static class ProfileManager
{
    public static Profile CurrentProfile
    {
        private set
        {
            
        }
        get
        {
            
        }
    }
    private static List<Profile>? profiles;
    /// <summary>
    /// 初始化档案数据库
    /// </summary>
    private static void InitializeProfilesDatabase()
    {
        
    }
    /// <summary>
    /// 保存档案
    /// </summary>
    public static void SaveProfile()
    {
        
    }
    /// <summary>
    /// 清空档案数据库缓存（被移除的档案会在下次加载时恢复，除非数据库没有整这个档案）
    /// </summary>
    public static void Clear()
    {
        
    }
    /// <summary>
    /// 获取某一个档案
    /// </summary>
    /// <param name="ProfileId">档案索引</param>
    /// <returns>代表这个档案的 Profile 类</returns>
    public static Profile GetProfile(int ProfileId)
    {
        
    }
    /// <summary>
    /// 删除某个档案（这会导致档案永久丢失）
    /// </summary>
    /// <param name="ProfileId">要删除的档案</param>
    public static void DeleteProfile(int ProfileId)
    {
        
    }
    /// <summary>
    /// 删除整个档案数据库（这会导致存储在数据库的全部档案丢失）
    /// </summary>
    public static void DeleteProfiles()
    {
        
    }
    /// <summary>
    /// 刷新档案，如果档案过期，则会尝试重新登录
    /// </summary>
    public static void RefreshProfile()
    {
        
    }
}

/// <summary>
/// 账户类型
/// </summary>
public enum McLoginType
{
    /// <summary>
    /// 离线
    /// </summary>
    Offline = 0,
    /// <summary>
    /// 微软（正版）
    /// </summary>
    Microsoft = 1,
    /// <summary>
    /// Yggdrasil 第三方登录
    /// </summary>
    Auth = 2,
    /// <summary>
    /// 统一通行证
    /// </summary>
    Nide = 3
}