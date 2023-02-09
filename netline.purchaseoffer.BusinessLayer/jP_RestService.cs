using netline.purchaseoffer.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Configuration;

namespace netline.purchaseoffer.BusinessLayer
{
    public class jP_RestService
    {
        public static string token="";

        static string hostUrl="";
        static  string port = "";


        public string getBaseURIOfAllRequests()
        {
            hostUrl = WebConfigurationManager.AppSettings["hostUrl"];
            port = WebConfigurationManager.AppSettings["port"];
            return "http://" + hostUrl + ":" + port + "/logo/restservices/rest/v1.0/";
        }

        public string getEncodedAuthToken()
        {
            string  returnValue = "";
            try
            {
                string CLIENT_TOKEN_EN = "1";
                string AUTH_TOKEN_EN = token;
                string USERNAME = WebConfigurationManager.AppSettings["UserName"];
                string temp = CLIENT_TOKEN_EN + ":" + AUTH_TOKEN_EN + ":" + USERNAME;
                returnValue = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(temp));
            }
            catch (Exception ex)
            {
                returnValue = ex.Message;
            }

            return returnValue;
        }

        public string getToken()
        {

            try
            {

                string  FirmNr = WebConfigurationManager.AppSettings["firmNr"];
                hostUrl = WebConfigurationManager.AppSettings["hostUrl"];
                port = WebConfigurationManager.AppSettings["port"];
                string  userName = WebConfigurationManager.AppSettings["UserName"];
                string  Password = WebConfigurationManager.AppSettings["Password"];
                string HEADER =userName+":"+Password+":1:"+Convert.ToInt16(FirmNr).ToString()+":TRTR";
                string basicAuth = "Basic " + System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(HEADER));
                HttpWebRequest webrequest = (HttpWebRequest)System.Net.WebRequest.Create("http://"+hostUrl+":"+port+"/logo/restservices/rest/login");
                webrequest.Method = "POST";
                webrequest.Accept = "application/json";
                webrequest.ContentType = "application/json";
                webrequest.Headers.Add("Authorization", basicAuth);
                var response=   webrequest.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var tokenResponse= JsonConvert.DeserializeObject<net_tokenResponse>(responseString);
                token = tokenResponse.authToken;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return token;
        }

        public string postOrder(string data,string ProjectNo)
        {
            string resp_Reference="";
            try
            {
                string urlPathForRequest = getBaseURIOfAllRequests();
                urlPathForRequest = urlPathForRequest + "orders";
                
                string path = "D:\\PurchaseOffer\\JsonFolder\\Test.txt";
                if (!File.Exists(path))
                {
                    
                    File.WriteAllText(path, data);
                }

                var postData =  Encoding.ASCII.GetBytes(data);
                WebRequest webrequest = WebRequest.Create(urlPathForRequest);
                webrequest.Method = "POST";
                webrequest.ContentType = "application/json";
                webrequest.Headers.Add("auth-token", getEncodedAuthToken());
                var responseString = "";
                try
                {
                    using (var stream = webrequest.GetRequestStream())
                    {
                        stream.Write(postData, 0, postData.Length);
                    }
                    var response = (HttpWebResponse)webrequest.GetResponse();
                    responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    if (response.StatusCode.ToString() == "Created")
                    {
                        jP_Response resp = JsonConvert.DeserializeObject<jP_Response>(responseString);
                        string resP_Href=   resp.data.meta.href;
                        resp_Reference = resP_Href.Substring(resP_Href.LastIndexOf('/') + 1);

                    }
                }
                catch (WebException webEx)
                {
                    var response = ((HttpWebResponse)webEx.Response);
                    StreamReader content = new StreamReader(response.GetResponseStream());
                    return content.ReadToEnd();

                }
                catch (Exception e)
                {
                    return e.Message;
                    //POSTStatusCodeArea.Text = "Error!";
                }
            }
            catch (Exception ex)
            {

                resp_Reference = ex.Message;
            }
            logOut();

            return resp_Reference;

        }
        public string  logOut()
        {
            //string  FirmNr = WebConfigurationManager.AppSettings["firmNr"];
            hostUrl = WebConfigurationManager.AppSettings["hostUrl"];
            port = WebConfigurationManager.AppSettings["port"];

            HttpWebRequest webrequest = (HttpWebRequest)System.Net.WebRequest.Create("http://"+hostUrl+":"+port+"/logo/restservices/rest/logout");
            var responseString = "";
            webrequest.Method = "POST";
            webrequest.ContentType = "application/json";
            webrequest.Headers.Add("auth-token", getEncodedAuthToken());
            try
            {
                
                var response = (HttpWebResponse)webrequest.GetResponse();
                responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            }
            catch (WebException webEx)
            {
                var response = ((HttpWebResponse)webEx.Response);
                StreamReader content = new StreamReader(response.GetResponseStream());
                responseString= content.ReadToEnd();

            }
            return responseString;
        }

        public string postOfferAlternatives(jP_OfferAlternatives offerAlternatives, string reference)
        {
            string resp_Reference="";
            string offerData=JsonConvert.SerializeObject(offerAlternatives);
            //   return offerData;
            string urlPathForRequest = getBaseURIOfAllRequests();
            urlPathForRequest = urlPathForRequest + "loofferalternatives";
            var postData = Encoding.ASCII.GetBytes(offerData);

            WebRequest webrequest = WebRequest.Create(urlPathForRequest);
            webrequest.Method = "POST";
            webrequest.ContentType = "application/json";
            webrequest.Headers.Add("auth-token", getEncodedAuthToken());

            var responseString = "";
            try
            {
                using (var stream = webrequest.GetRequestStream())
                {
                    stream.Write(postData, 0, postData.Length);
                }

                var response = (HttpWebResponse)webrequest.GetResponse();

                responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                if (response.StatusCode.ToString() == "Created")
                {
                    jP_Response resp = JsonConvert.DeserializeObject<jP_Response>(responseString);
                    string resP_Href=   resp.data.meta.href;
                    resp_Reference = resP_Href.Substring(resP_Href.LastIndexOf('/') + 1);

                }
            }
            catch (WebException webEx)
            {
                var response = ((HttpWebResponse)webEx.Response);
                StreamReader content = new StreamReader(response.GetResponseStream());
                return content.ReadToEnd();

                //POSTResponseArea.Text = webEx.Message + "\n" + responseString;
                //POSTStatusCodeArea.Text = "Error!";
            }
            catch (Exception e)
            {
                return e.Message;
                //POSTStatusCodeArea.Text = "Error!";
            }
            return resp_Reference;

        }


        public string postUpdateTrans(string transRef, string reqRef, string uomRef)
        {
            string resp_Reference="";

            string urlPathForRequest = getBaseURIOfAllRequests();
            urlPathForRequest = "http://localhost:8080/logo/restservices/rest/customization/invokeMethod?className={CB036915-BABF-2C9A-BBD8-A66228AC00FE}.LPT.RestCustomWebService&methodName=addItem&parameters=%5B \"" + transRef + "\",\"" + reqRef + "\",\"" + uomRef + "\" %5d";



            WebRequest webrequest = WebRequest.Create(urlPathForRequest);
            webrequest.Method = "POST";
            webrequest.ContentType = "application/json";
            webrequest.Headers.Add("auth-token", getEncodedAuthToken());

            var responseString = "";
            try
            {
                //using (var stream = webrequest.GetRequestStream())
                //{
                //    stream.Write(postData, 0, postData.Length);
                //}

                var response = (HttpWebResponse)webrequest.GetResponse();

                responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                if (response.StatusCode.ToString() == "OK")
                {
                    jP_Response resp = JsonConvert.DeserializeObject<jP_Response>(responseString);
                    string resP_Href=   resp.data.meta.href;
                    resp_Reference = resP_Href.Substring(resP_Href.LastIndexOf('/') + 1);

                }
            }
            catch (WebException webEx)
            {
                var response = ((HttpWebResponse)webEx.Response);
                StreamReader content = new StreamReader(response.GetResponseStream());
                return content.ReadToEnd();

                //POSTResponseArea.Text = webEx.Message + "\n" + responseString;
                //POSTStatusCodeArea.Text = "Error!";
            }
            catch (Exception e)
            {
                return e.Message;
                //POSTStatusCodeArea.Text = "Error!";
            }
            return resp_Reference;

        }


    }


}
