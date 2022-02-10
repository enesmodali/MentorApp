using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MentorAppAPI.Models
{
    public class Admin
    {
        private static Admin _admin;

        private Admin() { }

        public static Admin admin
        {
            get
            {
                 if (_admin == null)
                    _admin = new Admin();
            
                return _admin;
            }

        }
        public string Mail { get; set; }
        /*
         * 
         */
    }
}
