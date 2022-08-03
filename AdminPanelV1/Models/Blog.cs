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
    
    public partial class Blog
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Blog()
        {
            this.Comment = new HashSet<Comment>();
        }
    
        public int BlogId { get; set; }
        public int CategoryId { get; set; }
        public Nullable<int> SubCategoryId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImgUrl { get; set; }
    
        public virtual Category Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comment> Comment { get; set; }
        public virtual SubCategory SubCategory { get; set; }
    }
}
