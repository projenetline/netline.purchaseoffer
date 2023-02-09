using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace netline.purchaseoffer.Auth.Controllers
{
    public class ValuesController : ApiController
    {
       

        
        public RedirectResult Get()
        {          
            
           return Redirect("http://localhost:1666/Login/Auth?activeName=" + User.Identity.Name);
        }

    
    }
}
