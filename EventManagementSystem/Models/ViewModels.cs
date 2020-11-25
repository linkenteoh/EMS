using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EventManagementSystem.Models
{
    public class EventInsertVM
    {
        [Required]
        [StringLength(100)]
        public string name { get; set; }
        [Required]
        [StringLength(100)]
        public string des { get; set; }
        [Required]
        public decimal price { get; set; }
        [Required]
        public int availability { get; set; }
        [Required]
        public int participants { get; set; }
        [Required]
        public DateTime startDate { get; set; }
        [Required]
        public DateTime endDate { get; set; }
        [Required]
        public TimeSpan startTime { get; set; }
        [Required]
        public TimeSpan endTime { get; set; }
        [Required]
        public string duration { get; set; }
        [Required]
        public string organized_by { get; set; }
        [Required]
        public bool approvalStat { get; set; }
        [Required]
        public bool status { get; set; }
        [Required]
        public int venueId { get; set; }
        [Required]
        public HttpPostedFileBase Photo { get; set; }



    }
}