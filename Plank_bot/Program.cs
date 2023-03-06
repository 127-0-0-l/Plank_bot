using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using System.Timers;
using System.Linq;

namespace Plank_bot
{
    class Program
    {
        private const string Token = "2021596586:AAFP0gr331yj6ZiwwZhp099dJcgSmPZGi48";
        private static TelegramBotClient client;
        private static List<(long id, bool isReady)> chatIds = new List<(long id, bool isReady)>();

        static void Main()
        {
            client = new TelegramBotClient(Token);
            client.StartReceiving();
            client.OnMessage += OnMessageHandler;
            Console.ReadLine();
            client.StopReceiving();
        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            if (!chatIds.Select(item => item.id).Contains(e.Message.Chat.Id))
            {
                chatIds.Add((e.Message.Chat.Id, false));
            }

            var msg = e.Message;
            if (msg.Text != null)
            {
                Console.WriteLine($"{msg.From.Username}: {msg.Text}");
                switch (msg.Text)
                {
                    case "/start":
                        await client.SendTextMessageAsync(msg.Chat.Id, "привет :)", replyMarkup: GetButtons());
                        break;
                    case "время":
                        var time = GetTime();
                        await client.SendTextMessageAsync(msg.Chat.Id, "сегодня " + time.ToString("mm\\:ss"), replyMarkup: GetButtons());
                        break;
                    case "начать одному/одной":
                        time = GetTime();
                        Begin(msg.Chat.Id, time);
                        break;
                    case "начать вместе":
                        chatIds[chatIds.IndexOf(chatIds.Where(i => i.id == msg.Chat.Id).FirstOrDefault())] = (msg.Chat.Id, true);
                        if (chatIds.Where(i => i.isReady == true).Count() == 2)
                        {
                            for (int i = 0; i < chatIds.Count; i++)
                            {
                                chatIds[i] = (chatIds[i].id, false);
                            }

                            time = GetTime();
                            foreach (var item in chatIds)
                            {
                                Begin(item.id, time);
                            }
                        }
                        else
                        {
                            await client.SendTextMessageAsync(msg.Chat.Id, "ожидание других человеков", replyMarkup: GetButtons());
                        }
                        break;
                    default:
                        await client.SendTextMessageAsync(msg.Chat.Id, "._.", replyMarkup: GetButtons());
                        break;
                }
            }
        }

        private static async void Begin(long chatId, TimeSpan time)
        {
            Timer timer = new Timer(1000);
            var msg = await client.SendTextMessageAsync(chatId, time.ToString("mm\\:ss"));
            timer.Elapsed += async (s, e) =>
            {
                time = TimeSpan.FromSeconds(time.TotalSeconds - 1);
                await client.EditMessageTextAsync(msg.Chat.Id, msg.MessageId, time.ToString("mm\\:ss"));

                if (time.TotalSeconds <= 0)
                {
                    timer.Stop();
                    await client.SendTextMessageAsync(chatId, "молодец)", replyMarkup: GetButtons());
                }
            };
            timer.Start();
        }

        private static IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{ new KeyboardButton { Text = "время" } },
                    new List<KeyboardButton>{ new KeyboardButton { Text = "начать одному/одной" } },
                    new List<KeyboardButton>{ new KeyboardButton { Text = "начать вместе"}}
                }
            };
        }

        private static TimeSpan GetTime()
        {
            int days = (DateTime.Today - new DateTime(2022, 08, 30)).Days;
            return TimeSpan.FromSeconds(60 + days * 10);
        }
    }
}
