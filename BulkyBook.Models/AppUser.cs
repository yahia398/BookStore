using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name { get; set; }


        [Column(TypeName = "varchar(400)")]
        public string? StreetAddress { get; set; }


        [Column(TypeName = "varchar(200)")]
        public string? City { get; set; }


        [Column(TypeName = "varchar(200)")]
        public string? State { get; set; }


        [Column(TypeName = "varchar(200)")]
        public string? PostalCode { get; set; }
        public int? CompanyId { get; set; }
        [ValidateNever]
        public Company? Company { get; set; }
    }
}
