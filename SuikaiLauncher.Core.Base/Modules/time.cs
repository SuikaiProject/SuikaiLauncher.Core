namespace SuikaiLauncher.Core.Time{
    public class Time{
        public static long getCurrentTime(){
            return long.Parse((DateTime.Now - new DateTime(1970,1,1)).TotalSeconds.ToString().Split(".")[0]);
        }
    }
}