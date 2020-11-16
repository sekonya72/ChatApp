using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Models
{
    public class User
    {
        public User()
        {
            Messages = new HashSet<Message>();
            UserGroups = new List<UserGroup>();
        }
        public string UserId { get; set; }
        public string ConnectionID { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public ICollection<UserGroup> UserGroups { get; set; }
    }
}
