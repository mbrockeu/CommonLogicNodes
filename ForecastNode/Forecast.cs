using LogicModule.Nodes.Helpers;
using LogicModule.ObjectModel.TypeSystem;
using LogicModule.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Xml.Linq;
using System.Threading;

namespace marcus_brock_mbrock_eu.logic.ForecastNode
{
    public class Forecast : LogicNodeBase
    {
        private StringValueObject errorMessage;

        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://jsonplaceholder.typicode.com")
        };

        [Parameter(DisplayOrder = 1, InitOrder = 1, IsDefaultShown = false,IsRequired = false)]
        public StringValueObject APIKey { get; private set; }

        [Parameter(DisplayOrder = 2, InitOrder = 2, IsDefaultShown = false, IsRequired =true)]
        public StringValueObject Longitude { get; private set; }

        [Parameter(DisplayOrder = 3, InitOrder = 3, IsDefaultShown = false,IsRequired = true)]
        public StringValueObject Latitude { get; private set; }

        [Parameter(DisplayOrder = 4, InitOrder = 4, IsDefaultShown = false, IsRequired = true)]
        public IntValueObject Azimuth { get; private set; }

        [Parameter(DisplayOrder = 5, InitOrder = 5, IsDefaultShown = false, IsRequired = true)]
        public IntValueObject Declination { get; private set; }

        [Parameter(DisplayOrder = 6, InitOrder = 6, IsDefaultShown = false, IsRequired = true)]
        public DoubleValueObject KWP { get; private set; }

        [Input(DisplayOrder = 1, IsInput = true, IsRequired = true)]
        public BoolValueObject Trigger { get; private set; }

        //[Output]
        //public DoubleValueObject currentPVPower { get; private set; }

        [Output]
        public StringValueObject ErrorMessage { get => errorMessage; set => errorMessage = value; }

        public String APIUrl { get; private set; }

        public Forecast(INodeContext context)
        {
            context.ThrowIfNull("context");
            ITypeService typeService = context.GetService<ITypeService>();

            this.Trigger = typeService.CreateBool(PortTypes.Bool, "Trigger",true);
            this.APIKey = typeService.CreateString(PortTypes.String, "APIKey", string.Empty);
            this.Longitude = typeService.CreateString(PortTypes.String, "Longitude", string.Empty);
            this.Latitude = typeService.CreateString(PortTypes.String, "Latitude", string.Empty);
            this.Azimuth = typeService.CreateInt(PortTypes.Integer, "Azimuth", 0);
            this.Declination = typeService.CreateInt(PortTypes.Integer, "Declination", 0);
            this.KWP = typeService.CreateDouble(PortTypes.Number, "kWp", 0.0);

            //this.TimeSpan = typeService.CreateInt(PortTypes.Integer, "Abfrageinterval", 60);

            this.ErrorMessage = typeService.CreateString(PortTypes.String, "RAW / Error");
        }

        public override void Startup()
        {
            //https://api.forecast.solar/estimate/:lat/:lon/:dec/:az/:kwp
            if(APIKey.HasValue)
            {
                APIUrl = string.Format("https://api.forecast.solar/estimate/{0}/{1}/{2}/{3}/{4}/{5}", APIKey.Value,Latitude.Value,Longitude.Value,Declination.Value,Azimuth.Value,KWP.Value);
            } else
            {
                APIUrl = string.Format("https://api.forecast.solar/estimate/{0}/{1}/{2}/{3}/{4}", Latitude.Value, Longitude.Value, Declination.Value, Azimuth.Value, KWP.Value);
            }
        }

        public override void Execute()
        {
            if ((this.Trigger.HasValue && this.Trigger.WasSet && this.Trigger))
            {
                //List<WattHours> watthours;
                //using (var client = new HttpClient())
                //{
                //    client.BaseAddress = new Uri("https://api.forecast.solar/");
                //    client.DefaultRequestHeaders.Add("User-Agent", "Anything");
                //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //    var response = client.GetAsync(APIUrl).Result;
                //    response.EnsureSuccessStatusCode();
                //    watthours = response.Content.<List<WattHours>>().Result;
                //}
                //// Do something
            }
        }
        //public async Task<WattHours> GetMyObjectAsync(CancellationToken cts = default)
        //{
        //    // http get request to a rest api address
        //    var httpResponse = await _httpClient.GetAsync(APIUrl, cts);

        //    if (!httpResponse.IsSuccessStatusCode)
        //        throw new Exception("Oops... Something went wrong");

        //    // deserialize content stream into MyObject
        //    return await httpResponse.Content.ReadFromJsonAsync<WattHours>(cts);
        //}

        public  class WattHours
        {
            public DateTime Day { get; set; }
            public int Watts { get; set; }

            public override string ToString()
            {
                return $"{Day.ToString()}: {Watts} W";
            }
        }
    }
}
