using System;
using System.Collections.Generic;

namespace TelegramWallet.Database.Models
{
    public partial class Question
    {
        public int Id { get; set; }
        public string CreatorId { get; set; } = null!;
        public string? Question1 { get; set; }
        public string? Answer { get; set; }
        public string? Language { get; set; }
    }
}
