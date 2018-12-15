//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Apps.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class WMS_PO
    {
        public WMS_PO()
        {
            this.WMS_AI = new HashSet<WMS_AI>();
        }
    
        public int Id { get; set; }
        public string PO { get; set; }
        public Nullable<System.DateTime> PODate { get; set; }
        public int SupplierId { get; set; }
        public int PartId { get; set; }
        public decimal QTY { get; set; }
        public Nullable<System.DateTime> PlanDate { get; set; }
        public string POType { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string Attr1 { get; set; }
        public string Attr2 { get; set; }
        public string Attr3 { get; set; }
        public string Attr4 { get; set; }
        public string Attr5 { get; set; }
        public string CreatePerson { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string ModifyPerson { get; set; }
        public Nullable<System.DateTime> ModifyTime { get; set; }
    
        public virtual WMS_Part WMS_Part { get; set; }
        public virtual WMS_Supplier WMS_Supplier { get; set; }
        public virtual ICollection<WMS_AI> WMS_AI { get; set; }

        public virtual WMS_AI WMS_AI_NEW { get; set; }
    }
}
