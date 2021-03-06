namespace TelegramWallet.Classes;

public static class Dependencies
{
    public record BotInformation
    {
        public static string Token => "5021311514:AAEPIeUMQnS9Srvs8GkxB6Md7lqryFzER0Y";
        public static string Id => "someTelegramWalletTestBot";
    }

    public enum Languages
    {
        English=0,
        Spanish=1,
        Arabic=2,
        France=3,
        Russian=4,
        Italic=5,
        Mexico=6,
        SouthKorean=7,
        NorthKorean=8,
        Turkish=9,
        India=10,
        Indonesia=11,
        Pakistan=12,
        Germany=13,
        Egypt=14,
        Portuguese=15,
        Nigeria=16,
        Vietnamese=17,
        Filipino=18,
        Argentina=19,
        Swiss=20,
    }

    public static List<string> LanguagesList => Enum.GetNames(typeof(Dependencies.Languages)).ToList();

    public static Dictionary<Languages, Dictionary<string, string>> LangDictionary = new()
    {

        { Languages.English, new Dictionary<string, string>()
        {
            { "Login", "Login" }, 
            { "Register", "Register" }
        } },

        { Languages.Spanish ,new Dictionary<string, string>() { {"Welcome", "Bienvenidos"},{"Enter Your Name:", "Introduzca su nombre:" } }}
    };

    public static string NewConnectionString(string user,string pass,string dbName) => $@"Server=.;Database={dbName};Integrated Security=True;Connect Timeout=30;User ID={user};Password={pass}";

    public static string ConnectionString { get; set; } = @"Data Source=.;Initial Catalog=MexinamitBot;User ID=bot;Password=alisalehi!@#123";

    public static List<string> StatusList => new() { "admin", "Admin", "owner", "Owner", "Member", "Administrator", "administrator", "Creator" };
    public static string ApiUrl => "https://mexinamit.ali-chv.com/api";
    public static string PerfectMoneyApiUrl => "https://mexinamit.com/gateways/prefect-money";
}