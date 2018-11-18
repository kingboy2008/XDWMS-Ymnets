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

	public partial class Flow_StepModel:Virtual_Flow_StepModel
	{
		
	}
	public class Virtual_Flow_StepModel
	{
		[Display(Name = "GUID主键")]
		public virtual string Id { get; set; }
		[Display(Name = "步骤名称")]
		public virtual string Name { get; set; }
		[Display(Name = "步骤说明")]
		public virtual string Remark { get; set; }
		[Display(Name = "排序")]
		public virtual int Sort { get; set; }
		[Display(Name = "所属表单")]
		public virtual string FormId { get; set; }
		[Display(Name = "流转规则")]
		public virtual int FlowRule { get; set; }
		[Display(Name = "该流程的 发起人/创建者 是否可以 自行选择 该步骤的审批者")]
		public virtual bool IsCustom { get; set; }
		[Display(Name = "当规则或者角色被选择为多人时候，是否启用多人审核才通过")]
		public virtual bool IsAllCheck { get; set; }
		[Display(Name = "执行者与规则对应")]
		public virtual string Execution { get; set; }
		[Display(Name = "是否可以强制完成整个流程")]
		public virtual bool CompulsoryOver { get; set; }
		[Display(Name = "审核者是否可以编辑发起者的附件")]
		public virtual bool IsEditAttr { get; set; }
		}
}