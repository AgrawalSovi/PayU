using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PayURefund
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                bool isSuccess = false;
                string PayUMoneyURL = "https://test.payu.in/merchant/postservice.php?form=2"; //Configurations.AppSettings.PayU_URL + "refundPayment";
                RefundRQ req = new RefundRQ();
                req.merchantKey = "gtKFFx";
                req.paymentId = "40399371550994501023456789012334hytrder";
                req.refundAmount = 500M;
                RefundTicketPost refundTicketPost = GetRefundParameters(req);
                byte[] dataToPost = GetPostBytes(refundTicketPost);
                HttpWebRequest postRequest = (HttpWebRequest)WebRequest.Create(PayUMoneyURL);
                postRequest.Method = "POST";
                postRequest.ContentType = "application/json";
                postRequest.ContentLength = dataToPost.Length;
                Stream st = postRequest.GetRequestStream();
                st.Write(dataToPost, 0, dataToPost.Length);
                st.Close();
                HttpWebResponse postResponse = (HttpWebResponse)postRequest.GetResponse();

                if (postResponse.StatusCode == HttpStatusCode.OK)
                    isSuccess = true;
                else
                    isSuccess = false;
                // For testing only
                Stream stream1 = postResponse.GetResponseStream();
                StreamReader sr = new StreamReader(stream1);
                string strsb = sr.ReadToEnd();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public class RefundTicketPost
        {
            public string key { get; set; }
            public string hash { get; set; }
            public string var1 { get; set; }
            public string var2 { get; set; }
            public string var3 { get; set; }
            public string command { get; set; }
        }

        private static RefundTicketPost GetRefundParameters(RefundRQ req)
        {
            RefundTicketPost refundTicketPost = new RefundTicketPost();
            refundTicketPost.command = "cancel_refund_transaction";
            string salt = "eCwWELxi";
            refundTicketPost.var1 = req.paymentId;
            refundTicketPost.var3 = req.refundAmount.ToString();
            refundTicketPost.key = req.merchantKey;
            string hash = GetHashString(req.merchantKey, refundTicketPost.command, refundTicketPost.var1, salt);
            refundTicketPost.hash = Generatehash512(hash).ToLower();
            refundTicketPost.var2 = refundTicketPost.hash.Substring(0, 20);
            return refundTicketPost;
        }

        private static string GetHashString(string merchantkey, string Command, string paymentId, string salt)
        {
            var hash_string = string.Empty;
            hash_string += merchantkey + "|" + Command + "|" + paymentId + "|" + salt;
            return hash_string;
        }

        public static string Generatehash512(string text)
        {

            byte[] message = Encoding.UTF8.GetBytes(text);

            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            SHA512Managed hashString = new SHA512Managed();
            string hex = "";
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

        public class RefundRQ
        {
            public string merchantKey { get; set; }
            public string paymentId { get; set; }
            public decimal refundAmount { get; set; }
        }

        public static byte[] GetPostBytes(object obj)
        {

            MemoryStream mStream = new MemoryStream();
            if (obj != null)
            {
                var json = JsonConvert.SerializeObject(obj);
                var stringbyte = Encoding.UTF8.GetBytes(json);
                mStream.Write(stringbyte, 0, stringbyte.Length);
            }
            return mStream.ToArray();
        }
    }
}
