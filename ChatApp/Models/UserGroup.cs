using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Models
{
    public class UserGroup
    {
        public string UserID { get; set; }
        public string GroupID { get; set; }
        public User User { get; set; }
        public Group Group { get; set; }
    }
}
