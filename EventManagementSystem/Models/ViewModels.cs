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
        [RegularExpression(@"[A-Za-z ]+", ErrorMessage = "Name should contain alphabets only")]
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
        public string duration { get; set; }
        [Required]
        public string organized_by { get; set; }
        public bool approvalStat { get; set; }
        public bool status { get; set; }
        public int? venueId { get; set; }
        public HttpPostedFileBase Photo { get; set; }

    }

    public class EventEditVM
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        [RegularExpression(@"[A-Za-z ]+", ErrorMessage = "Name should contain alphabets only")]
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
        public string duration { get; set; }
        [Required]
        public string organized_by { get; set; }
        public bool approvalStat { get; set; }
        public HttpPostedFileBase Photo { get; set; }
        public string photoURL { get; set; }
    }
    public class UserInsertVM
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
        [Remote("IsUserNameAvailable", "Account", ErrorMessage = "Username already exists")]
        [MinLength(5, ErrorMessage = "5 minimum length")]
        [MaxLength(15, ErrorMessage = "15 maximum length")]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
        [System.ComponentModel.DataAnnotations.Compare("password", ErrorMessage = "Password not matched")]
        public string confirmPassword { get; set; }
        [Required]
        public HttpPostedFileBase Photo { get; set; }
        public Role role { get; set; }
        public Nullable<bool> organizer { get; set; }
        public int status { get; set; }
        public string recoveryCode { get; set; }
        public string activationCode { get; set; }

    }
    public enum Role
    {
        Student,
        Staff,
        Admin,
        Outsider
    }
    public class UserEditVM
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
        public string username { get; set; }
        [Required]
        public string password { get; set; }
        [System.ComponentModel.DataAnnotations.Compare("password", ErrorMessage = "Password not matched")]
        public string confirmPassword { get; set; }
        public HttpPostedFileBase Photo { get; set; }
        public string photoURL { get; set; }
        public Role role { get; set; }
        public Nullable<bool> organizer { get; set; }
        public bool status { get; set; }
        public string recoveryCode { get; set; }
        public string activationCode { get; set; }

    }
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
        [Remote("IsUserNameAvailable", "Account", ErrorMessage = "Username already exists")]
        [MinLength(5, ErrorMessage = "5 minimum length")]
        [MaxLength(15, ErrorMessage = "15 maximum length")]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
        [System.ComponentModel.DataAnnotations.Compare("password", ErrorMessage = "Password not matched")]
        public string confirmPassword { get; set; }
        [Required]
        public HttpPostedFileBase Photo { get; set; }
        public string role { get; set; }
        public Nullable<bool> organizer { get; set; }
        public string recoveryCode { get; set; }
        public string activationCode { get; set; }
    }

    public class EditProfileVM
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
        [Remote("IsUserNameAvailable", "Account", ErrorMessage = "Username already exists")]
        [MinLength(5, ErrorMessage = "5 minimum length")]
        [MaxLength(15, ErrorMessage = "15 maximum length")]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
        [System.ComponentModel.DataAnnotations.Compare("password", ErrorMessage = "Password not matched")]
        public string confirmPassword { get; set; }
        //[Required]
        public HttpPostedFileBase Photo { get; set; }
        public string PhotoUrl { get; set; }
        public string role { get; set; }
        public Nullable<bool> organizer { get; set; }
        public int status { get; set; }
        public string recoveryCode { get; set; }
        public string activationCode { get; set; }
    }

    public class LoginVM
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }


    public class AdvertManageVM
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        [RegularExpression(@"[A-Za-z ]+", ErrorMessage = "Name should contain alphabets only")]
        public string name { get; set; }
        [Required]
        [StringLength(100)]
        public string des { get; set; }
        [Required]
        public decimal charge { get; set; }
        [Required]
        public DateTime startDate { get; set; }
        [Required]
        public DateTime endDate { get; set; }
        [Required]
        public TimeSpan startTime { get; set; }
        [Required]
        public TimeSpan endTime { get; set; }
        public string duration { get; set; }
        public bool status { get; set; }
        public int? userId { get; set; }
        public HttpPostedFileBase Photo { get; set; }
        public string photoURL { get; set; }
      
    }
    public class RegOrgVM
    {
        [Required]
        public string represent { get; set; }
        [Required]
        public string position { get; set; }
    }
}