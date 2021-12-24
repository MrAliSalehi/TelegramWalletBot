using System;
using System.Collections.Generic;

namespace TelegramWallet.Database.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public byte? LoginStep { get; set; }
        public string? UserPass { get; set; }
        public byte? WithDrawStep { get; set; }
        public byte? DepositStep { get; set; }
        public string? WithDrawAccount { get; set; }
        public string? WithDrawAmount { get; set; }
        public string? WitchDrawPaymentMethod { get; set; }
        public string? DepositAmount { get; set; }
        public string? DepositAccount { get; set; }
        public string? Language { get; set; }
        public string? Token { get; set; }
        public string? ManualAccount { get; set; }
        public byte? PublicSteps { get; set; }
    }
}
