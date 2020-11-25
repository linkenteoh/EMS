using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Models
{
    public class RegisterVM
    {
        public int Id { get; set; }
        [Required]
        [RegularExpression(@"[A-Za-z ]+", ErrorMessage = "Name should contain alphabets only")]
        public string name { get; set; }
        [Required(ErrorMessage = "The contact number field is required")]
        [RegularExpression(@"(\+?6?01)[0-46-9]-*[0-9]{7,8}", ErrorMessage = "Invalid format")]
        public string contact_no { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid format")]
        public string email { get; set; }
        [Required]
        [MinLength(5, ErrorMessage = "5 minimum length")]
        [MaxLength(15, ErrorMessage = "15 maximum length")]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
        [Compare("password", ErrorMessage = "Password not matched")]
        public string confirmPassword { get; set; }
        [Required]
        public HttpPostedFileBase Photo { get; set; }
        public string role { get; set; }
        public Nullable<bool> organizer { get; set; }
        public int status { get; set; }
        public string recoveryCode { get; set; }
        public string activationCode { get; set; }
    }
}