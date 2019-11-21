using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherAPI
{
	public class DayTemperature
	{
		public string Day { get; set; }
		public double Value { get; set; }
		public double Wind { get; set; }
		public string Symbol { get; set; }
		public int Pressure { get; set; }
	}
}
