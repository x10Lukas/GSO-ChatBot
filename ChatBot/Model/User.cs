using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aufgabe_GSOChatBot.Model
{
    internal class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Passwort { get; set; } = null!;
        public string Token { get; set; }
    }
}
