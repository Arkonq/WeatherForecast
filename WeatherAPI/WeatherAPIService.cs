using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows;
using System.Xml;

namespace WeatherAPI
{
	class WeatherAPIService
	{
		private const string APIKEY = "8d9822b63fc95ec65a071696165ac14f";
		private string currentURL;
		public XmlDocument xmlDocument;

		public WeatherAPIService(string city)
		{
			SetCurrentURL(city);
			xmlDocument = GetXML(currentURL);
		}

		public List<DayTemperature> GetTemp()
		{
			CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
			cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";

			List<TimeTemperature> timeTemperatures = new List<TimeTemperature>();

			XmlNodeList timeNodes = xmlDocument.SelectNodes("//time");

			foreach (XmlNode timeNode in timeNodes)
			{
				XmlAttribute fromAttribute = timeNode.Attributes["from"];
				XmlAttribute toAttribute = timeNode.Attributes["to"];

				XmlNode temperatureNode = timeNode.SelectSingleNode("temperature");
				XmlNode symbolNode = timeNode.SelectSingleNode("symbol");
				XmlNode windSpeedNode = timeNode.SelectSingleNode("windSpeed");
				XmlNode PressureNode = timeNode.SelectSingleNode("pressure");

				XmlAttribute temperatureValueAttribute = temperatureNode.Attributes["value"];
				XmlAttribute symbolVarAttribute = symbolNode.Attributes["var"];
				XmlAttribute windSpeedMpsAttribute = windSpeedNode.Attributes["mps"];
				XmlAttribute pressureValueAttribute = PressureNode.Attributes["value"];

				TimeTemperature timeTemperature = new TimeTemperature
				{
					From = DateTime.Parse(fromAttribute.Value),
					To = DateTime.Parse(toAttribute.Value)/*.ToString("g")*/,
					Value = double.Parse(temperatureValueAttribute.Value, NumberStyles.Any, cultureInfo),
					SymbolVar = (symbolVarAttribute.Value.Substring(0, symbolVarAttribute.Value.Length - 1)),   // там выходят символы погоды при дне или ночи, но я решил обрезать конкретику для или ночи для более удобного вычисления общего символа погоды для одного дня
					Wind = double.Parse(windSpeedMpsAttribute.Value, NumberStyles.Any, cultureInfo),
					Pressure = Int32.Parse(pressureValueAttribute.Value)
				};

				timeTemperatures.Add(timeTemperature);
			}
			List<DayTemperature> dayTemperatures = new List<DayTemperature>();

			for (int i = 0; i < 5; i++) // Всего 5 карточек погоды
			{
				double value = 0;
				double wind = 0;
				int pressure = 0;
				int tempCnt = timeTemperatures.FindAll(x => x.From.Day == DateTime.Now.Day + i).Count; // Кол-во температур за 1 день, т.к. в первый день кол-во температур варьируется в зависимости от текущего времени
				List<string> images = new List<string>();
				foreach (var temp in timeTemperatures.FindAll(x => x.From.Day == DateTime.Now.Day + i))
				{
					value += temp.Value;
					wind += temp.Wind;
					pressure += temp.Pressure;
					images.Add(temp.SymbolVar);
				}

				var frequency = from symbol in images
								group symbol by symbol into grouped
								select new Frequency
								{
									Symbol = grouped.Key,
									Value = grouped.Count()
								};
				GetMax(frequency.ToList());

				DayTemperature dayTemperature = new DayTemperature
				{
					Day = DateTime.Now.AddDays(i).DayOfWeek.ToString(),
					Value = Math.Round((value / tempCnt), 1),  // Округление до 1 знака после запятой
					Wind = Math.Round((wind / tempCnt), 1),
					Pressure = pressure / tempCnt,
					Symbol = ("Images/" + GetMax(frequency.ToList()).Symbol + ".png"),
				};
				;

				dayTemperatures.Add(dayTemperature);
			}
			return dayTemperatures;
		}

		public Frequency GetMax(List<Frequency> frequencies)
		{
			Frequency max = frequencies.FirstOrDefault();
			foreach (var frequency in frequencies)
			{
				if (frequency.Value > max.Value)
				{
					max = frequency;
				}
			}
			return max;
		}

		private void SetCurrentURL(string location)
		{
			currentURL = "http://api.openweathermap.org/data/2.5/forecast?q="
				+ location + "&mode=xml&units=metric&appid=" + APIKEY;
		}

		private XmlDocument GetXML(string CurrentURL)
		{
			using (WebClient client = new WebClient())
			{
				try
				{
					string xmlContent = client.DownloadString(CurrentURL);
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(xmlContent);
					return xmlDocument;
				}
				catch
				{
					return null;
				}
			}
		}
	}
}