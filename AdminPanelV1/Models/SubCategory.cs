//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AdminPanelV1.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SubCategory
    {
        public int SubCategoryId { get; set; }
        public Nullable<int> CategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public string ImgUrl { get; set; }
    
        public virtual Category Category { get; set; }
    }
}
