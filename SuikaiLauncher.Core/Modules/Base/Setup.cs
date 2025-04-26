using System.Text.Json.Nodes;


namespace SuikaiLauncher.Core.Base
{
    public class Environments
    {
        
        public static string MojangPath = $"{Environment.SpecialFolder.ApplicationData.ToString()}/.minecraft";
       
        public static string ApplicationDataPath = $"{Environment.SpecialFolder.ApplicationData.ToString()}/SuikaiLauncher/Core";

        public static string ConfigPath = ApplicationDataPath + "/config.json";

        public static void CreateConfig()
        {
            JsonObject ConfigData = new JsonObject();
            ConfigData["System"] = new JsonObject();
            ConfigData["Version"] = new JsonObject();
            ConfigData["Account"] = new JsonObject();
            ConfigData["DataBases"] = new JsonObject();
            File.WriteAllText(ConfigPath,ConfigPath.ToString());
        }
    }
}
