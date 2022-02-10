using MentorApp.Graph;
using MentorAppAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MentorAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly GraphServiceClient _graphClient;
        private readonly ILogger<UserController> _logger;
        private readonly Admin admin;


        public UserController(
         GraphServiceClient graphClient,
         ILogger<UserController> logger)
        {
            _graphClient = graphClient;
            _logger = logger;
        }

        [HttpGet("/{dateTime}&{userMail}&{days}")]
        public async Task<ActionResult<DailyViewModel>> GetCalendar(DateTime dateTime, string userMail, int days)//IAction????????????
        {
            try
            {
                UserAPI user = new UserAPI(userMail);
                var day = dateTime;

                var viewOptions = new List<QueryOption>
                {
                    new QueryOption("startDateTime", dateTime.ToString("0")),
                    new QueryOption("endDateTime", dateTime.AddDays(days).ToString("0"))
                };

                var events = await _graphClient.Me
                    .CalendarView
                    .Request(viewOptions)
                    .Header("Prefer", $"outlook.timezone=\"{User.GetUserGraphTimeZone()}\"")
                    .Top(50)
                    .Select(a => new
                    {
                        a.Subject,
                        a.Organizer,
                        a.Start,
                        a.End,
                        a.Attendees
                    }).OrderBy("start/dateTime").GetAsync();
                //var newEvent = events.Where(a=>a.Organizer.EmailAddress.Address=admin.mail);
                var adminPick = events.Where(a => a.Organizer.EmailAddress.Address.ToString() == admin.Mail);
                return Ok(events);

            }
            catch (Exception)
            {

                return BadRequest();
            }


            
        }

    }
}
