using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminPanelV1.Models.Dto
{
    public class CategoriesDto
    {
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> SubCategories { get; set; }
    }
}