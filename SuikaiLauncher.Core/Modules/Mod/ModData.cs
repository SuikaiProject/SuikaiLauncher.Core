using System;

namespace SuikaiLauncher.Core.Mod{
    public class ModData{
        private static Dictionary<string,object>? ModMetaData;
        private static Dictionary<string,string>? ModTranslate;
        private static readonly object ModTranslateLock = new object[1];
        public static void GetModI18nEN(string Input){
            try{
                List<string> search_key = new();
                if (ModTranslate is null) LoadModData();
                foreach (var name in ModTranslate)
                {
                    if (name.Key.Contains(Input)) search_key.Add(name.Value);
                }
                //return search_key;
            }catch(Exception ex){
                Logger.Log(ex,"获取工程列表搜索文本失败");
            }
        }
        public static void LoadModData(){
            
        }
    }
}