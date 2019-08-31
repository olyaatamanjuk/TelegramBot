using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using Telegram.Bot.Types.Enums;

namespace BotApp
{
	public class TelegramBot
	{
		BackgroundWorker bw;
		BotAssistent BotAs;

		public void StartBot()
		{
			string key = "622335668:AAEv62L9dRi4BH6JVyW8gfEg-tCmd-qTRXo";
			this.bw = new BackgroundWorker();
			this.bw.DoWork += bw_DoWork;
			this.bw.RunWorkerAsync(key);
			BotAs = new BotAssistent();
		}

		async void GetQuestion(Telegram.Bot.TelegramBotClient Bot, Telegram.Bot.Args.CallbackQueryEventArgs ev, Telegram.Bot.Types.Message message)
		{
			await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);

			if (!BotAs.GameStarted)
			{
				BotAs = new BotAssistent();
			}


			Country country = BotAs.GetRandomCountry();
			string tCountry1 = country.Name;
			string tCapital1 = country.Capital;
			string tFlagPath = country.Flag;

			string tCapital2 = BotAs.GetRandomCapital(tCapital1, "");
			string tCapital3 = BotAs.GetRandomCapital(tCapital1, tCapital2);


			//Визначаємо рандомно порядок правильної відповіді
			Random rnd = new Random();
			int rndValue = rnd.Next(1, 4);

			Dictionary<int, string> dic = new Dictionary<int, string>();
			dic.Add(rndValue, tCapital1);

			if (rndValue == 1)
			{
				dic.Add(2, tCapital2);
				dic.Add(3, tCapital3);
			}
			else if (rndValue == 2)
			{
				dic.Add(1, tCapital2);
				dic.Add(3, tCapital3);
			}
			else
			{
				dic.Add(1, tCapital2);
				dic.Add(2, tCapital3);
			}

			dic = dic.OrderBy(x => x.Key).ToDictionary(y => y.Key, z => z.Value);


			var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(
										new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[][]
										{
															new [] {

																 new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton { Text=dic[1], CallbackData = (rndValue == 1) ? "correctAnswer" + tCountry1 : "wrongAnswer" },
																 new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton { Text=dic[2], CallbackData = (rndValue == 2) ? "correctAnswer" + tCountry1 : "wrongAnswer" },
																 new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton { Text=dic[3], CallbackData = (rndValue == 3) ? "correctAnswer" + tCountry1 : "wrongAnswer" },
															},
										}
									);

			await Bot.SendTextMessageAsync(message.Chat.Id, tCountry1 + " ?", replyMarkup: keyboard);
			await Bot.SendPhotoAsync(message.Chat.Id, tFlagPath, "");
		}

		async void bw_DoWork(object sender, DoWorkEventArgs e)
		{
			var worker = sender as BackgroundWorker;
			string key = "622335668:AAEv62L9dRi4BH6JVyW8gfEg-tCmd-qTRXo";
			try
			{
				var Bot = new Telegram.Bot.TelegramBotClient(key);
				await Bot.SetWebhookAsync("");

				// Callback'и от кнопок
				Bot.OnCallbackQuery += async (object sc, Telegram.Bot.Args.CallbackQueryEventArgs ev) =>
				{
					var message = ev.CallbackQuery.Message;
					if (ev.CallbackQuery.Data == "callback0")
					{
						await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id, "You hav choosen " + ev.CallbackQuery.Data, true);
					}

					else if (ev.CallbackQuery.Data.Contains("correctAnswer"))
					{
						await Bot.SendTextMessageAsync(message.Chat.Id, "Вірно!", replyToMessageId: message.MessageId);
						await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);// видаляє очікування (годинник)
						GetQuestion(Bot, ev, message);
						string tCountry = ev.CallbackQuery.Data.Replace("correctAnswer", "");
						BotAs.RightAnswer(tCountry);

					}

					else if (ev.CallbackQuery.Data == "wrongAnswer")
					{
						await Bot.SendTextMessageAsync(message.Chat.Id, "Емм..Ні. Йдем далі...", replyToMessageId: message.MessageId);
						await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);// видаляє очікування (годинник)
						GetQuestion(Bot, ev, message);
						BotAs.WrongAnswer();
					}

					else
					if (ev.CallbackQuery.Data.Contains("Continent:"))
					{
						string tContinent = ev.CallbackQuery.Data.Replace("Continent:", "");
						await Bot.SendTextMessageAsync(message.Chat.Id, "Хмм, сміливо!", replyToMessageId: message.MessageId);
						BotAs.ChooseContinent(tContinent);
						GetQuestion(Bot, ev, message);
					}
				};

				Bot.OnUpdate += async (object su, Telegram.Bot.Args.UpdateEventArgs evu) =>
				{
					if (evu.Update.CallbackQuery != null || evu.Update.InlineQuery != null) return;
					var update = evu.Update;
					var message = update.Message;
					if (message == null) return;
					if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
					{
						if (message.Text == "/hello")
						{
							await Bot.SendTextMessageAsync(message.Chat.Id, "Хелова", replyToMessageId: message.MessageId);
						}
						if (message.Text == "/getimage")
						{
							await Bot.SendPhotoAsync(message.Chat.Id, "https://static.365info.kz/uploads/2019/03/a346a3729504594579883eeb12a38d80.jpg", "Та й таке!");
						}

						if (message.Text == "/stopgame")
						{
							await Bot.SendTextMessageAsync(message.Chat.Id, "Гру завершено! Правильних: " + BotAs.RightAnswers + " з " + (BotAs.WrongAnswers + BotAs.RightAnswers), replyToMessageId: message.MessageId);
							BotAs.OverGame();
						}

						if (message.Text == "/capitals")
						{
							BotAs.StartGame();
							List<string> continents = BotAs.GetContinents();
							continents.Add("Все");

							Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[] Keys = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[continents.Count];

							int i = 0;

							foreach (string continent in continents)
							{
								Keys[i] = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton { Text = continent, CallbackData = "Continent:" + continent };
								i++;
							}

							var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(
																		new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[][]
																		{
																			Keys
																		}
																		);

							await Bot.SendTextMessageAsync(message.Chat.Id, "З чого розпочнемо гру?", replyMarkup: keyboard);
						}
					}
				};

				Bot.StartReceiving();
			}
			catch (Telegram.Bot.Exceptions.ApiRequestException ex)
			{
				Console.WriteLine(ex.Message);
			}

		}
	}

}
