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
namespace Apps.Models.Flow
{

	public partial class Flow_ExternalModel:Virtual_Flow_ExternalModel
	{
		
	}
	public class Virtual_Flow_ExternalModel
	{
		[Display(Name = "未设置")]
		public virtual string Id { get; set; }
		[Display(Name = "未设置")]
		public virtual string Title { get; set; }
		[Display(Name = "未设置")]
		public virtual string Remark { get; set; }
		[Display(Name = "未设置")]
		public virtual string Photo { get; set; }
		[Display(Name = "未设置")]
		public virtual string SysUserId { get; set; }
		[Display(Name = "未设置")]
		public virtual string TrueName { get; set; }
		[Display(Name = "未设置")]
		public virtual System.DateTime CreateTime { get; set; }
		}
}