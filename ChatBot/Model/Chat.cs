﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aufgabe_GSOChatBot.Model;

namespace Aufgabe_GSOChatBot.Model
{
    internal class Chat
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<Nachricht> Nachricht { get; set; }
        public string Name { get; set; }
        public string Charakter { get; set; } = null!;
    }
}