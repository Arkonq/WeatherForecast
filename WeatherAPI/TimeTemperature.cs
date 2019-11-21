using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherAPI
{
	public class TimeTemperature
	{
		public DateTime From { get; set; }
		public DateTime To { get; set; }
		public double Value { get; set; }
		public double Wind { get; set; }
		public string SymbolVar { get; set; }
		public int Pressure { get; set; }
	}
}
