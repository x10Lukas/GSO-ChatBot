using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aufgabe_GSOChatBot
{
    internal class Nachricht
    {
        public int Id { get; set; }
        public int ParenId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime Gesendet { get; set; }
        public string Sender { get; set; } = null!;
        public int ChatId { get; set; }
        public Chat Chat { get; set; } = null!;
    }
}
