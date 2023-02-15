using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }

        [ValidateNever]
        // Create the dropdown list for Categories
        public IEnumerable<SelectListItem> CategoryList { get; set; }

        [ValidateNever]
        // Create the dropdown list for Cover Types
        public IEnumerable<SelectListItem> CoverTypeList { get; set; }
    }
}
