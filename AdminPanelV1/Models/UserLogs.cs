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
    
    public partial class UserLogs
    {
        public int UserLogId { get; set; }
        public int UserId { get; set; }
        public string State { get; set; }
        public Nullable<System.DateTime> LogDate { get; set; }
    
        public virtual Users Users { get; set; }
        public virtual Users Users1 { get; set; }
    }
}