﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Apps.Common;
using Apps.DAL;
using Apps.IBLL;
using Apps.Models;
using Unity.Attributes;
using Apps.Models.Sys;
using Apps.Models.WC;
using Apps.DAL.WC;
using Apps.DAL.Sys;
using Apps.BLL.Sys;

namespace Apps.Web.Core
{
    public static class LogHandler
    {
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="oper">操作人</param>
        /// <param name="mes">操作信息</param>
        /// <param name="result">结果</param>
        /// <param name="type">类型</param>
        /// <param name="module">操作模块</param>
        public static void WriteServiceLog(string oper, string mes, string result, string type, string module)
        {
            SysConfigModel siteConfig = new SysConfigBLL().loadConfig(Utils.GetXmlMapPath("Configpath"));
            //后台管理日志开启
            if (siteConfig.logstatus == 1)
            {
                ValidationErrors errors = new ValidationErrors();
                SysLog entity = new SysLog();
                entity.Id = ResultHelper.NewId;
                entity.Operator = oper;
                entity.Message = mes;
                entity.Result = result;
                entity.Type = type;
                entity.Module = module;
                entity.CreateTime = ResultHelper.NowTime;
                using (SysLogRepository logRepository = new SysLogRepository(new DBContainer()))
                {
                    logRepository.Create(entity);
                }
            }
            else
            {
                return;
            }
        }

        public static void WriteWeChatLog(WC_ResponseLogModel model)
        {
            SysConfigModel siteConfig = new SysConfigBLL().loadConfig(Utils.GetXmlMapPath("Configpath"));
            //后台管理日志开启
            if (siteConfig.logstatus == 1)
            {
                WC_ResponseLog entity = new WC_ResponseLog();
                entity.Id = ResultHelper.NewId;
                entity.OpenId = model.OpenId;
                entity.RequestType = model.RequestType;
                entity.RequestContent = model.RequestContent;
                entity.ResponseType = model.ResponseType;
                entity.ResponseContent = model.ResponseContent;
                entity.CreateBy = "";
                entity.CreateTime = ResultHelper.NowTime;
                entity.ModifyBy = "";
                entity.ModifyTime = ResultHelper.NowTime;

                using (WC_ResponseLogRepository logRepository = new WC_ResponseLogRepository(new DBContainer()))
                {
                    logRepository.Create(entity);
                }
            }
            else
            {
                return;
            }
        }


        /// <summary>
        /// 写入Excel导入的日志
        /// </summary>
        /// <param name="table">导入的表名</param>
        /// <param name="fileName">Excel文件名</param>
        /// <param name="fileUrl">Excel文件Url</param>
        /// <param name="status">导入状态</param>
        /// <param name="oper">操作员</param>
        public static void WriteImportExcelLog(string oper, string table, string fileName, string fileUrl, string status)
        {
            SysImportExcelLog entity = new SysImportExcelLog();
            entity.Id = 0;
            entity.ImportTime = DateTime.Now;
            entity.ImportTable = table;
            entity.ImportFileName = fileName;
            entity.ImportFilePathUrl = fileUrl;
            entity.ImportStatus = status;
            entity.CreateBy = oper;
            using (SysImportExcelLogRepository logRepository = new SysImportExcelLogRepository(new DBContainer()))
            {
                logRepository.Create(entity);
            }
        }
    }
}