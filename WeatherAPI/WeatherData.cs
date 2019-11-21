using System;
using System.Collections.Generic;

namespace WeatherAPI
{
	class WeatherData
	{
		public string City { get; set; }

		public WeatherData(string city)
		{
			City = city;
		}

		public List<DayTemperature> GetWeather()
		{
			WeatherAPIService dataAPI = new WeatherAPIService(City);
			return dataAPI.GetTemp();
		}
	}
}