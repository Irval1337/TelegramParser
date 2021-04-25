using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Newtonsoft.Json;
using Telegram.Bot;

namespace TelegramParser
{
    public partial class Form1 : Form
    {
        KeyWords keyWords = new KeyWords();
        public static bool needClose = false;
        string[] Groups;

        public Form1()
        {
            InitializeComponent();
        }

        private void авторToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://irval.host");
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
            Application.Exit();
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button1.Text = "Начать работу";
            needClose = true;

            var settings = new Settings();
            settings.FormClosed += Form1_Load;
            settings.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(Properties.Settings.Default.KeyWords))
            {
                Properties.Settings.Default.KeyWords = JsonConvert.SerializeObject(new KeyWords());
                Properties.Settings.Default.Save();
            }
            keyWords = JsonConvert.DeserializeObject<KeyWords>(Properties.Settings.Default.KeyWords);
            Groups = JsonConvert.DeserializeObject<string[]>(Properties.Settings.Default.Groups);
            label1.Text = $"Количество чатов: {Groups.Length}";
            label2.Text = $"Собрано сообщений: {Properties.Settings.Default.MsgCount}";
            label4.Text = $"Фильтры для поиска: {keyWords.keyWords.Count - keyWords.stopWords}";
            label5.Text = $"Стоп-слова: {keyWords.stopWords}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Начать работу")
            {
                button1.Text = "Остановить бота";
                needClose = false;
                Task.Factory.StartNew(() => { BotThread(this); } );
            }
            else
            {
                button1.Text = "Начать работу";
                needClose = true;
            }
        }

        void AddLog(string text)
        {
            this.Invoke(new MethodInvoker(() => { richTextBox1.Text += $"[{DateTime.Now.ToShortTimeString()}] {text}\n"; }));
        }

        int session = 0;

        private void BotThread(object obj)
        {
            Form1 main = (Form1)obj;
            try
            {
                var client = new TelegramBotClient(Properties.Settings.Default.TgToken);
                AddLog("Авторизация прошла успешно! Имя бота: " + client.GetMeAsync().Result.Username);
                client.StartReceiving();
                client.OnMessage += (async (sender, evu) =>
                {
                    try
                    {
                        if (evu.Message.Text.StartsWith("/Info"))
                            client.SendTextMessageAsync(evu.Message.Chat.Id, "Уникальный Id вашего чата: " + evu.Message.Chat.Id);
                        if (evu.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group || evu.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup)
                        {
                            if (evu.Message.Text.StartsWith("/AddGroup"))
                            {
                                if (!Groups.Contains(evu.Message.Chat.Id.ToString()))
                                {
                                    var tmp = Groups.ToList();
                                    tmp.Add(evu.Message.Chat.Id.ToString());
                                    Groups = tmp.ToArray();
                                    Properties.Settings.Default.Groups = JsonConvert.SerializeObject(Groups);
                                    client.SendTextMessageAsync(evu.Message.Chat.Id, "Новая группа успешно добавлена");
                                    AddLog("Добавлена новая группа для прослушки: " + evu.Message.Chat.Id.ToString());
                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        label1.Text = $"Количество чатов: {Groups.Length}";
                                    }));
                                }
                            }
                            else if (evu.Message.Text.StartsWith("/RemoveGroup"))
                            {
                                if (Groups.Contains(evu.Message.Chat.Id.ToString()))
                                {
                                    var tmp = Groups.ToList();
                                    tmp.Remove(evu.Message.Chat.Id.ToString());
                                    Groups = tmp.ToArray();
                                    Properties.Settings.Default.Groups = JsonConvert.SerializeObject(Groups);
                                    client.SendTextMessageAsync(evu.Message.Chat.Id, "Ваша группа успешно удалена");
                                    AddLog("Удалена группа: " + evu.Message.Chat.Id.ToString());
                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        label1.Text = $"Количество чатов: {Groups.Length}";
                                    }));
                                }
                            }
                            else if (Groups.Contains(evu.Message.Chat.Id.ToString()))
                            {
                                bool needCheck = false;
                                for (int i = 0; i < keyWords.keyWords.Count; i++)
                                {
                                    if (evu.Message.Text.Contains(keyWords.keyWords[i].value))
                                    {
                                        if (keyWords.keyWords[i].mode == KeyWord.Mode.IgnoreWord)
                                        {
                                            needCheck = false;
                                            break;
                                        }
                                        else
                                            needCheck = true;
                                    }
                                }
                                if (needCheck)
                                {
                                    if (Properties.Settings.Default.AdminId != 0)
                                        client.ForwardMessageAsync(Properties.Settings.Default.AdminId, evu.Message.Chat.Id, evu.Message.MessageId);
                                    if (Properties.Settings.Default.ChatId != 0)
                                        client.ForwardMessageAsync(Properties.Settings.Default.ChatId, evu.Message.Chat.Id, evu.Message.MessageId);
                                    AddLog("Отфильтровано новое сообщение");
                                    Properties.Settings.Default.MsgCount++;
                                    session++;
                                    this.Invoke(new MethodInvoker(() =>
                                    {
                                        label2.Text = $"Собрано сообщений: {Properties.Settings.Default.MsgCount}";
                                        label3.Text = $"Сообщений за сессию: {session}";
                                    }));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AddLog("Во время работы возникла ошибка: " + ex.Message);
                    }
                });
                AddLog("Бот успешно запущен!");
                while (!needClose) { };
                client.StopReceiving();
                Properties.Settings.Default.Save();
                AddLog("Работа бота остановлена!");

            }
            catch (Exception ex)
            {
                AddLog("Во время работы возникла ошибка: " + ex.Message);
            }
        }
    }
}
