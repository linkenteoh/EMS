//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EventManagementSystem.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Advertisement
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string des { get; set; }
        public System.DateTime startDate { get; set; }
        public System.DateTime endDate { get; set; }
        public System.TimeSpan startTime { get; set; }
        public System.TimeSpan endTime { get; set; }
        public string duration { get; set; }
        public decimal charge { get; set; }
        public string photoURL { get; set; }
        public bool status { get; set; }
        public int userId { get; set; }
    
        public virtual User User { get; set; }
    }
}
