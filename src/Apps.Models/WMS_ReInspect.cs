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
    
    public partial class WMS_ReInspect
    {
        public WMS_ReInspect()
        {
            this.WMS_ReturnOrder = new HashSet<WMS_ReturnOrder>();
        }
    
        public int Id { get; set; }
        public int AIId { get; set; }
        public string OCheckOutResult { get; set; }
        public Nullable<decimal> OQualifyQty { get; set; }
        public Nullable<decimal> ONoQualifyQty { get; set; }
        public string OCheckOutRemark { get; set; }
        public Nullable<System.DateTime> OCheckOutDate { get; set; }
        public string NCheckOutResult { get; set; }
        public Nullable<decimal> NQualifyQty { get; set; }
        public Nullable<decimal> NNoQualifyQty { get; set; }
        public string NCheckOutRemark { get; set; }
        public System.DateTime NCheckOutDate { get; set; }
        public string Remark { get; set; }
        public string AdjustMan { get; set; }
        public Nullable<System.DateTime> AdjustDate { get; set; }
        public string Attr1 { get; set; }
        public string Attr2 { get; set; }
        public string Attr3 { get; set; }
        public string Attr4 { get; set; }
        public string Attr5 { get; set; }
        public string CreatePerson { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string ModifyPerson { get; set; }
        public Nullable<System.DateTime> ModifyTime { get; set; }
    
        public virtual WMS_AI WMS_AI { get; set; }
        public virtual ICollection<WMS_ReturnOrder> WMS_ReturnOrder { get; set; }
    }
}
