﻿using Apps.Common;
using Apps.IBLL.Spl;
using Apps.Models.Spl;
using Unity.Attributes;
using Senparc.Weixin;
using Senparc.Weixin.BrowserUtility;
using Senparc.Weixin.Helpers;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.TenPayLibV3;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using ZXing;
using ZXing.Common;

namespace Apps.Web.Areas.WC.Controllers
{
    public class TenPayV3Controller : Controller
    {
        [Dependency]
        public ISpl_ProductBLL m_BLL { get; set; }
    
        ValidationErrors errors = new ValidationErrors();

        private static TenPayV3Info _tenPayV3Info;

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;
            return false;
        }


        public static TenPayV3Info TenPayV3Info
        {
            get
            {
                if (_tenPayV3Info == null)
                {
                    _tenPayV3Info =
                        TenPayV3InfoCollection.Data[System.Configuration.ConfigurationManager.AppSettings["TenPayV3_MchId"]];
                }
                return _tenPayV3Info;
            }
        }

        /// <summary>
        /// 获取用户的OpenId
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string productId, int hc = 0)
        {
            var returnUrl = string.Format("/wc/TenPayV3/JsAPI");
            var state = string.Format("{0}|{1}", productId, hc);
            var url = OAuthApi.GetAuthorizeUrl(TenPayV3Info.AppId, returnUrl, state, OAuthScope.snsapi_userinfo);

            return Redirect(url);
        }

        #region JsApi支付

        public ActionResult JsApi(string code, string state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Content("您拒绝了授权！");
            }

            if (!state.Contains("|"))
            {
                //这里的state其实是会暴露给客户端的，验证能力很弱，这里只是演示一下
                //实际上可以存任何想传递的数据，比如用户ID，并且需要结合例如下面的Session["OAuthAccessToken"]进行验证
                return Content("验证失败！请从正规途径进入！1001");
            }

            //获取产品信息
            var stateData = state.Split('|');
            //商品
            string Id = stateData[0];
            Spl_ProductModel product = m_BLL.GetById(Id);
            if (product == null)
            {
                return Content("商品信息不存在，或非法进入！1002");
            }
            //todo 这里需要为用户保存订单
            //todo 如果用户的订单已经存在，那么不需要再次添加，激活此订单



            ViewData["product"] = product;



            //通过，用code换取access_token
            //-------------------开起来支付
            var openIdResult = OAuthApi.GetAccessToken(TenPayV3Info.AppId, TenPayV3Info.AppSecret, code);
            if (openIdResult.errcode != ReturnCode.请求成功)
            {
                return Content("错误：" + openIdResult.errmsg);
            }

            string timeStamp = "";
            string nonceStr = "";
            string paySign = "";
            //把系统ID当成订单号
            string sp_billno = product.Id;
                      
              

            //创建支付应答对象
            RequestHandler packageReqHandler = new RequestHandler(null);
            //初始化
            packageReqHandler.Init();

            timeStamp = TenPayV3Util.GetTimestamp();
            nonceStr = TenPayV3Util.GetNoncestr();

            //设置package订单参数
            packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);		  //公众账号ID
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);		  //商户号
            packageReqHandler.SetParameter("nonce_str", nonceStr);                    //随机字符串
            packageReqHandler.SetParameter("body", product.Name+" "+product.Code);    //商品信息
            packageReqHandler.SetParameter("out_trade_no", sp_billno);		//商家订单号
            decimal price = product.Price ;

            packageReqHandler.SetParameter("total_fee", price.ToString());	//////////////////////商品金额,以分为单位(money * 100).ToString()
            packageReqHandler.SetParameter("spbill_create_ip", Request.UserHostAddress);   //用户的公网ip，不是商户服务器IP
            packageReqHandler.SetParameter("notify_url", TenPayV3Info.TenPayV3Notify);	   //接收财付通通知的URL
            packageReqHandler.SetParameter("trade_type", TenPayV3Type.JSAPI.ToString());  //交易类型
            packageReqHandler.SetParameter("openid", openIdResult.openid);	                    //用户的openId




            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign);	                    //签名

            string data = packageReqHandler.ParseXML();

            var result = TenPayV3.Unifiedorder(data);
            var res = XDocument.Parse(result);
            string prepayId = res.Element("xml").Element("prepay_id").Value;

            //设置支付参数
            RequestHandler paySignReqHandler = new RequestHandler(null);
            paySignReqHandler.SetParameter("appId", TenPayV3Info.AppId);
            paySignReqHandler.SetParameter("timeStamp", timeStamp);
            paySignReqHandler.SetParameter("nonceStr", nonceStr);
            paySignReqHandler.SetParameter("package", string.Format("prepay_id={0}", prepayId));
            paySignReqHandler.SetParameter("signType", "MD5");
            paySign = paySignReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);

            ViewData["appId"] = TenPayV3Info.AppId;
            ViewData["timeStamp"] = timeStamp;
            ViewData["nonceStr"] = nonceStr;
            ViewData["package"] = string.Format("prepay_id={0}", prepayId);
            ViewData["paySign"] = paySign;

            return View();
        }

        /// <summary>
        /// 原生支付 模式一
        /// </summary>
        /// <returns></returns>
        public ActionResult Native()
        {
            RequestHandler nativeHandler = new RequestHandler(null);
            string timeStamp = TenPayV3Util.GetTimestamp();
            string nonceStr = TenPayV3Util.GetNoncestr();

            //商品Id，用户自行定义
            string productId = DateTime.Now.ToString("yyyyMMddHHmmss");

            nativeHandler.SetParameter("appid", TenPayV3Info.AppId);
            nativeHandler.SetParameter("mch_id", TenPayV3Info.MchId);
            nativeHandler.SetParameter("time_stamp", timeStamp);
            nativeHandler.SetParameter("nonce_str", nonceStr);
            nativeHandler.SetParameter("product_id", productId);
            string sign = nativeHandler.CreateMd5Sign("key", TenPayV3Info.Key);

            var url = TenPayV3.NativePay(TenPayV3Info.AppId, timeStamp, TenPayV3Info.MchId, nonceStr, productId, sign);

            BitMatrix bitMatrix;
            bitMatrix = new MultiFormatWriter().encode(url, BarcodeFormat.QR_CODE, 600, 600);
            BarcodeWriter bw = new BarcodeWriter();

            var ms = new MemoryStream();
            var bitmap = bw.Write(bitMatrix);
            bitmap.Save(ms, ImageFormat.Png);
            //return File(ms, "image/png");
            ms.WriteTo(Response.OutputStream);
            Response.ContentType = "image/png";
            return null;
        }

        public ActionResult NativeNotifyUrl()
        {
            ResponseHandler resHandler = new ResponseHandler(null);

            //返回给微信的请求
            RequestHandler res = new RequestHandler(null);

            string openId = resHandler.GetParameter("openid");
            string productId = resHandler.GetParameter("product_id");

            if (openId == null || productId == null)
            {
                res.SetParameter("return_code", "FAIL");
                res.SetParameter("return_msg", "回调数据异常");
            }

            //创建支付应答对象
            RequestHandler packageReqHandler = new RequestHandler(null);

            var sp_billno = DateTime.Now.ToString("HHmmss") + TenPayV3Util.BuildRandomStr(28);
            var nonceStr = TenPayV3Util.GetNoncestr();

            //创建请求统一订单接口参数
            packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);
            packageReqHandler.SetParameter("nonce_str", nonceStr);
            packageReqHandler.SetParameter("body", "test");
            packageReqHandler.SetParameter("out_trade_no", sp_billno);
            packageReqHandler.SetParameter("total_fee", "1");
            packageReqHandler.SetParameter("spbill_create_ip", Request.UserHostAddress);
            packageReqHandler.SetParameter("notify_url", TenPayV3Info.TenPayV3Notify);
            packageReqHandler.SetParameter("trade_type", TenPayV3Type.NATIVE.ToString());
            packageReqHandler.SetParameter("openid", openId);
            packageReqHandler.SetParameter("product_id", productId);

            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign);

            string data = packageReqHandler.ParseXML();

            try
            {
                //调用统一订单接口
                var result = TenPayV3.Unifiedorder(data);
                var unifiedorderRes = XDocument.Parse(result);
                string prepayId = unifiedorderRes.Element("xml").Element("prepay_id").Value;

                //创建应答信息返回给微信
                res.SetParameter("return_code", "SUCCESS");
                res.SetParameter("return_msg", "OK");
                res.SetParameter("appid", TenPayV3Info.AppId);
                res.SetParameter("mch_id", TenPayV3Info.MchId);
                res.SetParameter("nonce_str", nonceStr);
                res.SetParameter("prepay_id", prepayId);
                res.SetParameter("result_code", "SUCCESS");
                res.SetParameter("err_code_des", "OK");

                string nativeReqSign = res.CreateMd5Sign("key", TenPayV3Info.Key);
                res.SetParameter("sign", nativeReqSign);
            }
            catch (Exception)
            {
                res.SetParameter("return_code", "FAIL");
                res.SetParameter("return_msg", "统一下单失败");
            }

            return Content(res.ParseXML());
        }

        /// <summary>
        /// 原生支付 模式二
        /// 根据统一订单返回的code_url生成支付二维码。该模式链接较短，生成的二维码打印到结账小票上的识别率较高。
        /// 注意：code_url有效期为2小时，过期后扫码不能再发起支付
        /// </summary>
        /// <returns></returns>
        public ActionResult NativeByCodeUrl()
        {
            //创建支付应答对象
            RequestHandler packageReqHandler = new RequestHandler(null);

            var sp_billno = DateTime.Now.ToString("HHmmss") + TenPayV3Util.BuildRandomStr(28);
            var nonceStr = TenPayV3Util.GetNoncestr();

            //商品Id，用户自行定义
            string productId = DateTime.Now.ToString("yyyyMMddHHmmss");

            //创建请求统一订单接口参数
            packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);
            packageReqHandler.SetParameter("nonce_str", nonceStr);
            packageReqHandler.SetParameter("body", "test");
            packageReqHandler.SetParameter("out_trade_no", sp_billno);
            packageReqHandler.SetParameter("total_fee", "1");
            packageReqHandler.SetParameter("spbill_create_ip", Request.UserHostAddress);
            packageReqHandler.SetParameter("notify_url", TenPayV3Info.TenPayV3Notify);
            packageReqHandler.SetParameter("trade_type", TenPayV3Type.NATIVE.ToString());
            packageReqHandler.SetParameter("product_id", productId);

            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign);

            string data = packageReqHandler.ParseXML();

            //调用统一订单接口
            var result = TenPayV3.Unifiedorder(data);
            var unifiedorderRes = XDocument.Parse(result);
            string codeUrl = unifiedorderRes.Element("xml").Element("code_url").Value;

            BitMatrix bitMatrix;
            bitMatrix = new MultiFormatWriter().encode(codeUrl, BarcodeFormat.QR_CODE, 600, 600);
            BarcodeWriter bw = new BarcodeWriter();

            var ms = new MemoryStream();
            var bitmap = bw.Write(bitMatrix);
            bitmap.Save(ms, ImageFormat.Png);
            //return File(ms, "image/png");
            ms.WriteTo(Response.OutputStream);
            Response.ContentType = "image/png";
            return null;
        }

        /// <summary>
        /// 刷卡支付
        /// </summary>
        /// <param name="authCode">扫码设备获取到的微信用户刷卡授权码</param>
        /// <returns></returns>
        public ActionResult MicroPay(string authCode)
        {
            RequestHandler payHandler = new RequestHandler(null);

            var sp_billno = DateTime.Now.ToString("HHmmss") + TenPayV3Util.BuildRandomStr(28);
            var nonceStr = TenPayV3Util.GetNoncestr();

            payHandler.SetParameter("auth_code", authCode);//授权码
            payHandler.SetParameter("body", "test");//商品描述
            payHandler.SetParameter("total_fee", "1");//总金额
            payHandler.SetParameter("out_trade_no", sp_billno);//产生随机的商户订单号
            payHandler.SetParameter("spbill_create_ip", Request.UserHostAddress);//终端ip
            payHandler.SetParameter("appid", TenPayV3Info.AppId);//公众账号ID
            payHandler.SetParameter("mch_id", TenPayV3Info.MchId);//商户号
            payHandler.SetParameter("nonce_str", nonceStr);//随机字符串

            string sign = payHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            payHandler.SetParameter("sign", sign);//签名

            var result = TenPayV3.MicroPay(payHandler.ParseXML());

            //此处只是完成最简单的支付功能，实际情况还需要考虑各种出错的情况，并处理错误，最后返回结果通知用户。

            return Content(result);
        }

        public ActionResult PayNotifyUrl()
        {
            ResponseHandler resHandler = new ResponseHandler(null);

            string return_code = resHandler.GetParameter("return_code");
            string return_msg = resHandler.GetParameter("return_msg");

            string res = null;

            resHandler.SetKey(TenPayV3Info.Key);
            //验证请求是否从微信发过来（安全）
            if (resHandler.IsTenpaySign())
            {
                res = "success";

                //正确的订单处理
            }
            else
            {
                res = "wrong";

                //错误的订单处理
            }

            var fileStream = System.IO.File.OpenWrite(Server.MapPath("~/1.txt"));
            fileStream.Write(Encoding.Default.GetBytes(res), 0, Encoding.Default.GetByteCount(res));
            fileStream.Close();

            string xml = string.Format(@"<xml>
   <return_code><![CDATA[{0}]]></return_code>
   <return_msg><![CDATA[{1}]]></return_msg>
</xml>", return_code, return_msg);

            return Content(xml, "text/xml");
        }

        #endregion

        #region 订单及退款

        /// <summary>
        /// 订单查询
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderQuery()
        {
            string nonceStr = TenPayV3Util.GetNoncestr();
            RequestHandler packageReqHandler = new RequestHandler(null);

            //设置package订单参数
            packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);		  //公众账号ID
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);		  //商户号
            packageReqHandler.SetParameter("transaction_id", "");       //填入微信订单号 
            packageReqHandler.SetParameter("out_trade_no", "");         //填入商家订单号
            packageReqHandler.SetParameter("nonce_str", nonceStr);             //随机字符串
            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign);	                    //签名

            string data = packageReqHandler.ParseXML();

            var result = TenPayV3.OrderQuery(data);
            var res = XDocument.Parse(result);
            string openid = res.Element("xml").Element("sign").Value;

            return Content(openid);
        }

        /// <summary>
        /// 关闭订单接口
        /// </summary>
        /// <returns></returns>
        public ActionResult CloseOrder()
        {
            string nonceStr = TenPayV3Util.GetNoncestr();
            RequestHandler packageReqHandler = new RequestHandler(null);

            //设置package订单参数
            packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);		  //公众账号ID
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);		  //商户号
            packageReqHandler.SetParameter("out_trade_no", "");                 //填入商家订单号
            packageReqHandler.SetParameter("nonce_str", nonceStr);              //随机字符串
            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign);	                    //签名

            string data = packageReqHandler.ParseXML();

            var result = TenPayV3.CloseOrder(data);
            var res = XDocument.Parse(result);
            string openid = res.Element("xml").Element("openid").Value;

            return Content(openid);
        }

        /// <summary>
        /// 退款申请接口
        /// </summary>
        /// <returns></returns>
        public ActionResult Refund()
        {
            string nonceStr = TenPayV3Util.GetNoncestr();
            RequestHandler packageReqHandler = new RequestHandler(null);

            //设置package订单参数
            packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);		  //公众账号ID
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);		  //商户号
            packageReqHandler.SetParameter("out_trade_no", "");                 //填入商家订单号
            packageReqHandler.SetParameter("out_refund_no", "");                //填入退款订单号
            packageReqHandler.SetParameter("total_fee", "");               //填入总金额
            packageReqHandler.SetParameter("refund_fee", "");               //填入退款金额
            packageReqHandler.SetParameter("op_user_id", TenPayV3Info.MchId);   //操作员Id，默认就是商户号
            packageReqHandler.SetParameter("nonce_str", nonceStr);              //随机字符串
            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign);	                    //签名
            //退款需要post的数据
            string data = packageReqHandler.ParseXML();

            //退款接口地址
            string url = "https://api.mch.weixin.qq.com/secapi/pay/refund";
            //本地或者服务器的证书位置（证书在微信支付申请成功发来的通知邮件中）
            string cert = @"F:\apiclient_cert.p12";
            //私钥（在安装证书时设置）
            string password = "";
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            //调用证书
            X509Certificate2 cer = new X509Certificate2(cert, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);

            #region 发起post请求
            HttpWebRequest webrequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webrequest.ClientCertificates.Add(cer);
            webrequest.Method = "post";

            byte[] postdatabyte = Encoding.UTF8.GetBytes(data);
            webrequest.ContentLength = postdatabyte.Length;
            Stream stream;
            stream = webrequest.GetRequestStream();
            stream.Write(postdatabyte, 0, postdatabyte.Length);
            stream.Close();

            HttpWebResponse httpWebResponse = (HttpWebResponse)webrequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            string responseContent = streamReader.ReadToEnd();
            #endregion

            var res = XDocument.Parse(responseContent);
            string openid = res.Element("xml").Element("out_refund_no").Value;

            return Content(openid);
        }
        #endregion

        #region 红包

        /// <summary>
        /// 目前支持向指定微信用户的openid发放指定金额红包
        /// 注意total_amount、min_value、max_value值相同
        /// total_num=1固定
        /// 单个红包金额介于[1.00元，200.00元]之间
        /// </summary>
        /// <returns></returns>
        //public ActionResult SendRedPack()
        //{
        //    string nonceStr;//随机字符串
        //    string paySign;//签名
        //    var sendNormalRedPackResult = RedPackApi.SendNormalRedPack(
        //        TenPayV3Info.AppId, TenPayV3Info.MchId, TenPayV3Info.Key,
        //        @"F:\apiclient_cert.p12",     //证书物理地址
        //        "接受收红包的用户的openId",   //接受收红包的用户的openId
        //        "红包发送者名称",             //红包发送者名称
        //        Request.UserHostAddress,      //IP
        //        100,                          //付款金额，单位分
        //        "红包祝福语",                 //红包祝福语
        //        "活动名称",                   //活动名称
        //        "备注信息",                   //备注信息
        //        out nonceStr,
        //        out paySign,
        //        null,                         //场景id（非必填）
        //        null,                         //活动信息（非必填）
        //        null                          //资金授权商户号，服务商替特约商户发放时使用（非必填）
        //        );

        //    SerializerHelper serializerHelper = new SerializerHelper();
        //    return Content(serializerHelper.GetJsonString(sendNormalRedPackResult));
        //}
        #endregion

        

        

        #region 产品展示

        //public ActionResult ProductList()
        //{
        //    var products = ProductModel.GetFakeProductList();
        //    return View(products);
        //}

        //public ActionResult ProductItem(int productId, int hc)
        //{
        //    var products = ProductModel.GetFakeProductList();
        //    var product = products.FirstOrDefault(z => z.Id == productId);
        //    if (product == null || product.GetHashCode() != hc)
        //    {
        //        return Content("商品信息不存在，或非法进入！2003");
        //    }

        //    //判断是否正在微信端
        //    var userAgent = Request.UserAgent;
        //    if (BroswerUtility.SideInWeixinBroswer(HttpContext))
        //    {
        //        //正在微信端，直接跳转到微信支付页面
        //        return RedirectToAction("Index", new { productId = productId, hc = hc });
        //    }
        //    else
        //    {
        //        //在PC端打开，提供二维码扫描进行支付
        //        return View(product);
        //    }
        //}

        /// <summary>
        /// 显示二维码
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        //public ActionResult ProductPayCode(int productId, int hc)
        //{
        //    var products = ProductModel.GetFakeProductList();
        //    var product = products.FirstOrDefault(z => z.Id == productId);
        //    if (product == null || product.GetHashCode() != hc)
        //    {
        //        return Content("商品信息不存在，或非法进入！2004");
        //    }

        //    var url = string.Format("http://sdk.weixin.senparc.com/TenPayV3?productId={0}&hc={1}&t={2}", productId,
        //        product.GetHashCode(), DateTime.Now.Ticks);

        //    BitMatrix bitMatrix;
        //    bitMatrix = new MultiFormatWriter().encode(url, BarcodeFormat.QR_CODE, 600, 600);
        //    BarcodeWriter bw = new BarcodeWriter();

        //    var ms = new MemoryStream();
        //    var bitmap = bw.Write(bitMatrix);
        //    bitmap.Save(ms, ImageFormat.Png);
        //    //return File(ms, "image/png");
        //    ms.WriteTo(Response.OutputStream);
        //    Response.ContentType = "image/png";
        //    return null;
        //}


        #endregion
    }
}
