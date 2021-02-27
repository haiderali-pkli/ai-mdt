using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace BDCS.Models
{
    public class SMSHelper
    {
        public static string SendSmsMessage(string message, string number, string mask)
        {
            string uri = "http://www.smspoint.pk/api/smsapi/";
            string res;
            string myParameters =
                $"username={"haider_sb"}&password={"haider_sb_2020"}&clientid={"Haider"}&mask={mask}&msg={HttpUtility.UrlEncode(message)}&to={FormatMobileNumber(number)}&language={"English"}";
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                res = wc.UploadString(uri, "POST", myParameters);
            }
            return res;
        }

        //comment
        private static string FormatMobileNumber(string mobile)
        {
            string number = "";

            if (string.IsNullOrEmpty(mobile) || mobile.Length < 10)
                return number;
            else
            {
                number = mobile.Replace("-", "").Replace(" ", "").Replace(".", "");
                if (number.StartsWith("0"))
                    number = "92" + number.Substring(1);
                else
                    number = "92" + number;
            }
            return number;
        }
    }
}