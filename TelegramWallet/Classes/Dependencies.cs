namespace TelegramWallet.Classes;

public static class Dependencies
{
    public record BotInformation
    {
        public static string Token => "5016105194:AAHzTZx51UwTilPSXmFD5YChY-J_Wxhr04c";
        public static string Id => "someTelegramWalletTestBot";
    }

    public enum Languages
    {
        English,
        Spanish,
        Arabic,
        France,
        Russian,
        Italic,
        Mexico,
        SouthKorean,
        NorthKorean,
        Turkish,
        India,
        Indonesia,
        Pakistan,
        Germany,
        Egypt,
        Portuguese,
        Nigeria,
        Vietnamese,
        Filipino,
        Argentina,
        Swiss,
    }

    public static Dictionary<Languages, Dictionary<string, string>> LangDictionary = new()
    {

        { Languages.English, new Dictionary<string, string>() { { "Welcome", "Welcome" }, { "Enter Your Name:", "Enter Your Name:" } } },
        { Languages.Spanish ,new Dictionary<string, string>() { {"Welcome", "Bienvenidos"},{"Enter Your Name:", "Introduzca su nombre:" } }}
    };

}