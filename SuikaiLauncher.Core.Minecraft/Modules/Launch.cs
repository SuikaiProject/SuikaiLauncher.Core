using System.Text;
using SuikaiLauncher.Core.Account;
using SuikaiLauncher.Core.Base;

namespace SuikaiLauncher.Core.Minecraft.Modules;

public class Launch
{
    public required McVersion? McVersion { get; set; }

    private Profile LaunchProfile = ProfileManager.CurrentProfile;
    
    public async Task LaunchGame()
    {
        Logger.Log("[Minecraft] 获取启动档案成功");
        Logger.Log("[Minecraft] 启动预检查阶段开始");
        this.PreCheck();
        Logger.Log("[Minecraft] 环境预检通过");
        Task[] LaunchTask =
        [
            this.CheckFile(),
            this.CheckRuntime()
        ];
        await Task.WhenAll(LaunchTask);
        Logger.Log("[Minecraft] ====== 启动信息 ======");
        Logger.Log($"[Minecraft] 游戏版本：{McVersion.Version}");
        Logger.Log($"[Minecraft] 游戏用户名：{LaunchProfile.Name}");
        Logger.Log($"[Minecraft] ");
        ProcessBuilder PBuilder = ProcessBuilder
            .Create()
            .Executable("java.exe")
            .RequireEncoding(Encoding.UTF8)
            .WithArgument(await this.GetJvmArgument())
            
        

    }
    /// <summary>
    /// 检查运行环境是否正确
    /// </summary>
    public async Task CheckRuntime()
    {
        
    }
    /// <summary>
    /// 预检运行环境和游戏信息
    /// </summary>
    public void PreCheck()
    {
        
    }
    /// <summary>
    /// 检查文件完整性
    /// </summary>
    private async Task CheckFile()
    {
        
    }

    private async Task<string> GetJvmArgument()
    {
        return "";
    }

    private async Task CrashCallback(TaskCanceledException ex)
    {
        Logger.Log("[Minecraft] Minecraft 已崩溃");
    }
}