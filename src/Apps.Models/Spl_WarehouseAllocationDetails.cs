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
    
    public partial class Spl_WarehouseAllocationDetails
    {
        public string Id { get; set; }
        public string WareDetailsId { get; set; }
        public string WarehouseId { get; set; }
        public string WarehouseAllocationId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public string Defined { get; set; }
        public System.DateTime CreateTime { get; set; }
    
        public virtual Spl_WareDetails Spl_WareDetails { get; set; }
        public virtual Spl_Warehouse Spl_Warehouse { get; set; }
        public virtual Spl_WarehouseAllocation Spl_WarehouseAllocation { get; set; }
    }
}
