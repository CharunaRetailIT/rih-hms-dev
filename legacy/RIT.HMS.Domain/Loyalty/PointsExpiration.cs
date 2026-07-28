using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Loyalty
{
    public class PointsExpiration:BaseEntity
    {
        public int PointsExpirationId { get; set; }

        [Required(ErrorMessage ="Year is required")]   
        public int Year { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Select a card type !")]
        public int CardType { get; set; }

        [Required(ErrorMessage = "First Reminder is required")]
        public string FirstReminderMessage { get; set; }
        public DateTime FirstReminderDate { get; set; }
        public string SecontReminderMessage { get; set; }
        public DateTime SecondReminderDate { get; set; }
        public DateTime PointsExpiryDate { get; set; }
    }
}
