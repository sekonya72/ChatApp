using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Models
{
    [MessagePackObject]
    public class Message
    {
        public Message()
        {
            When = DateTime.Now;
        }
        public string MessageId { get; set; }
        [Key(0)]
        public string UserId { get; set; }
        [Key(1)]
        public string Text { get; set; }
        public DateTime When { get; set; }

        
        public virtual User User { get; set; }
    }
}
