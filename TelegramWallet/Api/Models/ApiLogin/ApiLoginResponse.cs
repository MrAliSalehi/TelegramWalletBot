﻿namespace TelegramWallet.Api.Models.ApiLogin;

public class ApiLoginResponse
{
    public Data data { get; set; }
}

public class Data
{
    public string token { get; set; }
}