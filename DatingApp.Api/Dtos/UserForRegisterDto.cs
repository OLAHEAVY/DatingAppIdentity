using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Api.Dtos
{
    //This is a data transfer object for the register method
    public class UserForRegisterDto
    {
        [Required]
        public string Username{get; set;}

        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage="You must specify passwords between 4 and 8 characters")]
        public string Password{get;set;}

         [Required]
        public string Gender{get;set;}
         [Required]
        public string knownAs{get;set;}
         [Required]
        public DateTime DateofBirth{get;set;}
         [Required]
        public string City {get;set;}
         [Required]
        public string Country {get;set;}
         [Required]
        public DateTime Created {get;set;}
         [Required]
        public DateTime LastActive {get;set;}

        public UserForRegisterDto()
        {
            Created = DateTime.Now;
            LastActive = DateTime.Now;
        }
    }
}