using System;
using System.Collections.Generic;

namespace TelegramWallet.Database.Models
{
    public partial class Admin
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string? CurrentQuestionLanguage { get; set; }
        public byte? QuestionSteps { get; set; }
        public byte? CommandSteps { get; set; }
    }
}
