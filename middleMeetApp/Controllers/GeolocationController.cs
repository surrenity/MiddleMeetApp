using diffusedreality.middleMeetApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace diffusedreality.middleMeetApp.Controllers
{
    public class GeolocationController : ApiController
    {

      [Route("api/geoLocateByIP")]
      [HttpGet]
      public string geoLocateByIP()
      {
        string ipAddress = "0.0.0.0";
        string city = "Dallas, TX";
        if(this.Request.Properties.ContainsKey("MS_HttpContext"))
        {
          HttpContextWrapper wrapper = (HttpContextWrapper)Request.Properties["MS_HttpContext"];
          ipAddress = wrapper.Request.UserHostAddress;
        }

        using (var client = new HttpClient())
        {
          client.BaseAddress = new Uri("http://api.hostip.info/");
          client.DefaultRequestHeaders.Accept.Clear();
          client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

          HttpResponseMessage response = client.GetAsync(string.Format("get_json.php?ip={0}", ipAddress)).Result;
          if (response.IsSuccessStatusCode)
          {
            GeolocationResult result = response.Content.ReadAsAsync<GeolocationResult>().Result;
            city = result.city;
          }
        }

        return city;
      }
    }
}
