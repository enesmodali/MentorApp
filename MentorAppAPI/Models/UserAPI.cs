using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MentorAppAPI.Models
{
 
    public class UserAPI
    {
        private string _mail;

        public string Mail 
        {
            get 
            {
                return _mail;
            }
            set
            {
                if (value.EndsWith("@bilgeadamboost.com"))
                {
                    _mail = value;
                }
                else
                {
                    throw new Exception("Kullanıcı mail adresi @bilgeadamboost.com ile bitmelidir.");
                }
            }
        }

        public Role Role { get; set; }

        public UserAPI(string userMail)
        {
            Mail = userMail;   
        }

    }
}
