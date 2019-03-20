namespace SQLiteAdapter.Extensions
{
    public static class StringExtension
    {
        public static T DeserializeJson<T>(this string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json,
                                    new Newtonsoft.Json.JsonSerializerSettings()
                                    {
                                        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
                                    });
        }

        public static string SerializeJson<T>(this T obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj,
                new Newtonsoft.Json.JsonSerializerSettings
                {
                    TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                });
        }
    }
}
