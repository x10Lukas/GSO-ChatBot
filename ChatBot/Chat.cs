using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aufgabe_GSOChatBot
{
    internal class Chat
    {
        public int Id { get; set; }
        public User User { get; set; } = null!;
        public List<Nachricht> Nachricht { get; set; }
        public string Name { get; set; }
        public string Charakter { get; set; } = null!;
    }
}
