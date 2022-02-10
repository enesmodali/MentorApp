using MentorApp.Graph;
using MentorAppAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MentorAppAPI.Controllers
{
    [Route("api/Admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly GraphServiceClient _graphClient;

        [HttpPost("NewMeetingPost/{meeting}")]
        public async Task<IActionResult> NewMeeting([Bind("Mentor,Mentee,Yonetici,ToplantiBasligi,ToplantiTarihi,ToplantiSuresi")] Meeting meeting)
        {
            var timeZone = User.GetUserGraphTimeZone();

            // Create a Graph event with the required fields
            var graphEvent = new Event
            {
                Subject = meeting.ToplantiBasligi,
                Start = new DateTimeTimeZone
                {
                    DateTime = meeting.ToplantiTarihi.ToString("o"),
                    // Use the user's time zone
                    TimeZone = timeZone
                },
                End = new DateTimeTimeZone
                {
                    DateTime = meeting.ToplantiTarihi.Add(meeting.ToplantiSuresi).ToString("o"),
                    // Use the user's time zone
                    TimeZone = timeZone
                }
            };


            // Add attendees if present
                
                
                    var attendeeList = new List<Attendee>();

            attendeeList.Add(new Attendee
            {
                EmailAddress = new EmailAddress
                {
                    Address = meeting.Mentor.Mail,

                },
                Type = AttendeeType.Required
            }); 
            attendeeList.Add(new Attendee
            {
                EmailAddress = new EmailAddress
                {
                    Address = meeting.Mentee.Mail,

                },
                Type = AttendeeType.Required
            });
            attendeeList.Add(new Attendee
            {
                EmailAddress = new EmailAddress
                {
                    Address = meeting.Yonetici.Mail,

                },
                Type = AttendeeType.Optional
            });

            graphEvent.Attendees = attendeeList;
            

            try
            {
                // Add the event
                await _graphClient.Me.Events
                    .Request()
                    .AddAsync(graphEvent);

                // Redirect to the calendar view with a success message
                return Ok();
            }
            catch (ServiceException ex)
            {
                // Redirect to the calendar view with an error message
                return BadRequest("Calendar view with an error message");


            }
        }

    }
}
