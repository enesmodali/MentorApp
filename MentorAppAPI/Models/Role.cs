using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MentorAppAPI.Models
{
    public class Role
    {
        public string RoleName { get; set; }

        public List<UserAPI> Users { get; set; }
    }
}
