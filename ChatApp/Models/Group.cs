using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Models
{
    public class Group
    {
        public Group()
        {
            UserGroups = new List<UserGroup>();
            When = DateTime.Now;
        }
        public string GroupId { get; set; }
        public DateTime When { get; set; }
        public virtual ICollection<UserGroup> UserGroups { get; set; }
    }
}
