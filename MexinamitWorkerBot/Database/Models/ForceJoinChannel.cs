using System;
using System.Collections.Generic;

namespace MexinamitWorkerBot.Database.Models
{
    public partial class ForceJoinChannel
    {
        public int Id { get; set; }
        public string ChId { get; set; } = null!;
        public string? ChName { get; set; }
    }
}
