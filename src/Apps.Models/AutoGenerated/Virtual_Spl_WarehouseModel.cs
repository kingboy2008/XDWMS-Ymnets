//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

using Apps.Models;
using System;
using System.ComponentModel.DataAnnotations;
namespace Apps.Models.Spl
{

	public partial class Spl_WarehouseModel:Virtual_Spl_WarehouseModel
	{
		
	}
	public class Virtual_Spl_WarehouseModel
	{
		[Display(Name = "主键ID")]
		public virtual string Id { get; set; }
		[Display(Name = "仓库名称")]
		public virtual string Name { get; set; }
		[Display(Name = "仓库编码")]
		public virtual string Code { get; set; }
		[Display(Name = "是否默认")]
		public virtual bool IsDefault { get; set; }
		[Display(Name = "负责人")]
		public virtual string ContactPerson { get; set; }
		[Display(Name = "联系电话")]
		public virtual string ContactPhone { get; set; }
		[Display(Name = "地址")]
		public virtual string Address { get; set; }
		[Display(Name = "备注")]
		public virtual string Remark { get; set; }
		[Display(Name = "状态（true启用，false停用）")]
		public virtual bool Enable { get; set; }
		[Display(Name = "创建时间")]
		public virtual System.DateTime CreateTime { get; set; }
		[Display(Name = "未设置")]
		public virtual string WarehouseCategoryId { get; set; }
		}
}
