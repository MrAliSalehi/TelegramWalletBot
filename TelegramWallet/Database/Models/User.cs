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
        public string? WithDrawAccount { get; set; }
        public string? Language { get; set; }
        public string? WithDrawAmount { get; set; }
        public string? WitchDrawPaymentMethod { get; set; }
    }
}
