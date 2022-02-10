using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MentorAppAPI.Models
{
    public class Meeting
    {
        [Required]
        public UserAPI Mentor { get; set; }

        [Required]
        public UserAPI Mentee { get; set; }

        [Required]
        public UserAPI Yonetici { get; set; }


        public Admin Admin { get; set; }

        public DateTime ToplantiTarihi { get; set; }
        public TimeSpan ToplantiSuresi { get; set; }


        public string ToplantiBasligi { get; set; }

        public Meeting()
        {
            ToplantiSuresi = new TimeSpan(0,1,0,0);
        }

      
    }
}
