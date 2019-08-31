using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var Bot = new TelegramBot();
			Bot.StartBot();
			Console.ReadKey();
		}
	}
}
