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
    
    public partial class TablesLogs
    {
        public int TablesLogsId { get; set; }
        public int UserId { get; set; }
        public int ItemId { get; set; }
        public string TableName { get; set; }
        public string ItemName { get; set; }
        public string Process { get; set; }
        public System.DateTime LogDate { get; set; }
    
        public virtual Users Users { get; set; }
    }
}
