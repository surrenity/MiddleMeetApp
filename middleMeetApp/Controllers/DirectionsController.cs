using diffusedreality.middleMeetApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace diffusedreality.middleMeetApp.Controllers
{
    public class DirectionsController : ApiController
    {
        [Route("api/getDirections")]
        public LatLng getDirections(string start, string end)
        {
            LatLng origin = ConvertParameterToLatLng(start);
            LatLng destination = ConvertParameterToLatLng(end);
            return callGoogleAPI(origin, destination);
        }

        [Route("api/parseDirections")]
        [HttpPost]
        public LatLng parseDirections(DirectionsResult result)
        {
          LatLng latlng = processDirectionResult(result);
          return latlng;
        }

        private LatLng ConvertParameterToLatLng(string start)
        {
          var param = start.Trim('(').Trim(')').Split(',');
          LatLng latlng = new LatLng();
          double temp = 0;
          double.TryParse(param[0], out temp);
          latlng.lat = temp;
          double.TryParse(param[1], out temp);
          latlng.lng = temp;
          return latlng;
        }

        private LatLng callGoogleAPI(LatLng start, LatLng end)
        {
          LatLng latlng = new LatLng();
          using (var client = new HttpClient())
          {
            client.BaseAddress = new Uri("https://maps.googleapis.com/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync(string.Format("maps/api/directions/json?origin=({0},{1})&destination=({2},{3})&API_KEY=AIzaSyB4vBHSil2NPSXZOurCMwDvRSIEn3zLTaY", start.lat, start.lng, end.lat, end.lng)).Result;
            if (response.IsSuccessStatusCode)
            {
              DirectionsResult result = response.Content.ReadAsAsync<DirectionsResult>().Result;
              latlng = processDirectionResult(result);
            }
          }

          return latlng;
        }

        private LatLng processDirectionResult(DirectionsResult result)
        {

          LatLng latlng = new LatLng();

          //Get the Distance
          double totalDistance = result.routes[0].legs[0].distance.value;

          //Get half the distance
          double halfDistance = totalDistance / 2;

          //iterate the steps 
          int index = 0;
          var steps = result.routes[0].legs[0].steps;
          while (halfDistance - steps[index].distance.value > 0)
          {
            halfDistance = halfDistance - steps[index].distance.value;
            index++;
          }

          double ratio = halfDistance / steps[index].distance.value;

          double newLat = getMidPoint(steps[index].start_location.lat, steps[index].end_location.lat, ratio);
          double newLng = getMidPoint(steps[index].start_location.lng, steps[index].end_location.lng, ratio);
          latlng.lat = newLat;
          latlng.lng = newLng;
          return latlng;
        }

        public double getMidPoint(double start, double end, double ratio)
        {
          double diff = 0.0;
          if (start > end)//moving away from meridian line, east to west
          {
            diff = start - end;
            return start - (diff * ratio);
          }
          else//moving toward meridian line, west to east
          {
            diff = end - start;
            return start + (diff * ratio);
          }
        }
    }
}
