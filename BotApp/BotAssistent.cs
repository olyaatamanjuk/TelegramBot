using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BotApp
{
	public class Country
	{
		public string Continent { get; set; }
		public string Name { get; set; }
		public string Capital { get; set; }
		public string Flag { get; set; }

		public Country()
		{
			Name = "";
			Capital = "";
			Flag = "";
			Continent = "";
		}
	}


	public class BotAssistent
	{
		public bool GameStarted { get; set; }
		public List<Country> CountryList { get; set; }

		public int WrongAnswers { get; set; }
		public int RightAnswers { get; set; }

		public BotAssistent()
		{
			GameStarted = true;
			CountryList = new List<Country>();
			WrongAnswers = 0;
			RightAnswers = 0;
		}

		public void StartGame()
		{
			GameStarted = true;
			CountryList = new List<Country>();
			WrongAnswers = 0;
			RightAnswers = 0;
			InitializeCountryList();
		}

		public void InitializeCountryList()
		{
			string address = "https://ru.wikipedia.org/wiki/Список_столиц_мира";
			string html;
			using (var client = new WebClient())
			{
				client.Headers.Add(HttpRequestHeader.UserAgent, ".NET Application");
				client.Encoding = Encoding.UTF8;
				html = client.DownloadString(address);

			}
			var htmlDoc = new HtmlAgilityPack.HtmlDocument();
			htmlDoc.LoadHtml(html);
			var trNodes = htmlDoc.DocumentNode.SelectNodes("//table[@class='wikitable sortable']");

			foreach (var trNode in trNodes)
			{
				List<Country> tCountries = new List<Country>();

				var nodes = trNode.SelectNodes(".//tr");
				int coontOfCountries = nodes.Count;
				if (coontOfCountries > 0)
				{
					foreach (var row in nodes)
					{
						var rows = row.SelectNodes(".//td");
						if (rows == null)
						{
							continue;
						}
						string tCapital = rows[1].InnerText;
						tCapital = tCapital.Replace("\n", string.Empty);

						string tCountry = rows[2].InnerText;
						tCountry = tCountry.Replace("\n", string.Empty);

						string tFlag = "";

						int ioa = tCountry.IndexOf('&');
						if (ioa > -1)
						{
							string inh = rows[2].InnerHtml.ToString();
							string[] arr = inh.Split('<');
							foreach (string s in arr)
							{
								if (s.Contains("srcset="))
								{
									string[] arrins = s.Split('"');
									tFlag = arrins[3];
									tFlag = "https:" + tFlag.Replace("22px", "1000px");
								}
							}
							tCountry = tCountry.Remove(ioa, tCountry.Length - ioa);
							CountryList.Add(new Country { Name = tCountry, Flag = tFlag, Capital = tCapital });
							tCountries.Add(new Country { Name = tCountry, Flag = tFlag, Capital = tCapital });
						}
					}
				}

				string tContinent = "";
				if (tCountries.FirstOrDefault(x => x.Name == "Украина") != null)
				{
					tContinent = "Европа";
				}
				else if (tCountries.FirstOrDefault(x => x.Name == "Китай") != null)
				{
					tContinent = "Азия";
				}
				else if (tCountries.FirstOrDefault(x => x.Name == "Египет") != null)
				{
					tContinent = "Африка";
				}
				else if (tCountries.FirstOrDefault(x => x.Name == "Чили") != null)
				{
					tContinent = "Америка";
				}
				else if (tCountries.FirstOrDefault(x => x.Name == "Австралия") != null)
				{
					tContinent = "Австралия и Океания";
				}

				if (tContinent.Length > 0)
				{
					List<Country> countries = GetCountriesByContinent("");
					foreach (Country tCountry in countries)
					{
						tCountry.Continent = tContinent;
					}
				}

			}

		}
		public string GetRandomCapital(string excluding1, string excluding2)
		{
			string rndCapital = "";

			int maxVal = CountryList.Count;
			Random rnd = new Random();
			int rndValue = rnd.Next(0, maxVal);
			rndCapital = CountryList[rndValue].Capital;

			if (rndCapital == excluding1 || rndCapital == excluding2)
			{
				rndCapital = GetRandomCapital(excluding1, excluding2);
			}

			return rndCapital;
		}

		public Country GetRandomCountry()
		{

			int maxVal = CountryList.Count;
			Random rnd = new Random();
			int rndValue = rnd.Next(0, maxVal - 1);

			return CountryList[rndValue]; ;
		}

		public List<Country> GetCountriesByContinent(string continent)
		{
			List<Country> countries = CountryList.FindAll(x => x.Continent == continent);
			return countries;
		}

		public void ChooseContinent(string continent)
		{
			if (continent != "Все")
			{
				List<Country> countries = GetCountriesByContinent(continent);
				CountryList.Clear();
				CountryList.AddRange(countries);
			}
		}
		public List<string> GetContinents()
		{
			List<Country> countries = CountryList.GroupBy(d => d.Continent)
												 .Select(g => g.OrderByDescending(d => d.Continent).First())
												 .ToList();

			List<string> continents = new List<string>();
			foreach (var country in countries)
			{
				continents.Add(country.Continent);
			}
			return continents;
		}
		public void OverGame()
		{
			GameStarted = false;
			CountryList.Clear();
			WrongAnswers = 0;
			RightAnswers = 0;
		}

		public void RightAnswer(string country)
		{
			int index = CountryList.FindIndex(x => x.Name == country);
			CountryList.Remove(CountryList[index]);
			RightAnswers++;
		}

		public void WrongAnswer()
		{
			WrongAnswers++;
		}

	}


}
