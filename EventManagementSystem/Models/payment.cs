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
    
    public partial class Payment
    {
        public int Id { get; set; }
        public decimal price { get; set; }
        public Nullable<System.DateTime> paymentdate { get; set; }
        public decimal commision { get; set; }
        public decimal addCharge { get; set; }
        public bool status { get; set; }
    
        public virtual Registration Registration { get; set; }
    }
}
