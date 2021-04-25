using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Telegram.Bot;

namespace TelegramParser
{
    public partial class Settings : Form
    {
        KeyWords keyWords = new KeyWords();
        public void RemoveText(object sender, EventArgs e)
        {
            Control tb = (Control)sender;
            if (tb.Text == (string)tb.Tag)
                tb.Text = "";
        }

        public void AddText(object sender, EventArgs e)
        {
            Control tb = (Control)sender;
            if (string.IsNullOrWhiteSpace(tb.Text))
                tb.Text = (string)tb.Tag;
        }

        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(Properties.Settings.Default.TgToken))
                textBox1.Text = Properties.Settings.Default.TgToken;
            textBox2.Text = Properties.Settings.Default.AdminId.ToString();
            textBox3.Text = Properties.Settings.Default.ChatId.ToString();
            richTextBox1.Lines = JsonConvert.DeserializeObject<string[]>(Properties.Settings.Default.Groups);

            if (String.IsNullOrEmpty(Properties.Settings.Default.KeyWords)) {
                Properties.Settings.Default.KeyWords = JsonConvert.SerializeObject(new KeyWords());
                Properties.Settings.Default.Save();
            }

            keyWords = JsonConvert.DeserializeObject<KeyWords>(Properties.Settings.Default.KeyWords);
            foreach (var keyWord in keyWords.keyWords)
                listBox1.Items.Add(keyWord.value);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox4.Text) || comboBox1.SelectedIndex < 0)
                MessageBox.Show("Заполните все необходимые поля!", "TelegramParser");
            else if (!listBox1.Items.Contains(textBox4.Text))
            {
                KeyWord keyWord = new KeyWord();
                keyWord.value = textBox4.Text;
                keyWord.mode = comboBox1.SelectedIndex == 0 ? KeyWord.Mode.SortWord : KeyWord.Mode.IgnoreWord;
                keyWords.keyWords.Add(keyWord);
                listBox1.Items.Add(textBox4.Text);
                if (comboBox1.SelectedIndex == 1)
                    keyWords.stopWords++;

                Properties.Settings.Default.KeyWords = JsonConvert.SerializeObject(keyWords);
                Properties.Settings.Default.Save();

                textBox4.Clear();
                comboBox1.SelectedIndex = -1;
            }
            else MessageBox.Show("Данный фильтр уже существует!", "TelegramParser");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                if (keyWords.keyWords[listBox1.SelectedIndex].mode == KeyWord.Mode.IgnoreWord)
                    keyWords.stopWords--;
                keyWords.keyWords.RemoveAt(listBox1.SelectedIndex);
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                Properties.Settings.Default.KeyWords = JsonConvert.SerializeObject(keyWords);
                Properties.Settings.Default.Save();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                KeyWord keyWord = new KeyWord();
                keyWord.value = textBox4.Text;
                keyWord.mode = comboBox1.SelectedIndex == 0 ? KeyWord.Mode.SortWord : KeyWord.Mode.IgnoreWord;
                if (keyWords.keyWords[listBox1.SelectedIndex].mode == KeyWord.Mode.IgnoreWord && comboBox1.SelectedIndex == 0)
                    keyWords.stopWords--;
                else if (keyWords.keyWords[listBox1.SelectedIndex].mode != KeyWord.Mode.IgnoreWord && comboBox1.SelectedIndex == 1)
                    keyWords.stopWords++;
                keyWords.keyWords[listBox1.SelectedIndex] = keyWord;
                listBox1.Items[listBox1.SelectedIndex] = textBox4.Text;
                Properties.Settings.Default.KeyWords = JsonConvert.SerializeObject(keyWords);
                Properties.Settings.Default.Save();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                textBox4.Text = keyWords.keyWords[listBox1.SelectedIndex].value;
                comboBox1.SelectedIndex = (int)keyWords.keyWords[listBox1.SelectedIndex].mode;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var client = new TelegramBotClient(textBox1.Text);
                client.GetMeAsync();
                if (textBox2.Text != "0" && Convert.ToInt64(textBox2.Text) != Properties.Settings.Default.AdminId)
                    client.SendTextMessageAsync(Convert.ToInt64(textBox2.Text), "Ваш аккаунт добавлен в список администраторов");
                if (textBox3.Text != "0" && Convert.ToInt64(textBox3.Text) != Properties.Settings.Default.ChatId)
                    client.SendTextMessageAsync(Convert.ToInt64(textBox3.Text), "Ваш чат был выбран основным");
                Properties.Settings.Default.TgToken = textBox1.Text;
                Properties.Settings.Default.AdminId = Convert.ToInt64(textBox2.Text);
                Properties.Settings.Default.ChatId = Convert.ToInt64(textBox3.Text);
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка во время проверки введенных данных: " + ex.Message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            richTextBox1.Lines = richTextBox1.Lines.Where(l => l.Length > 0).ToArray();
            Properties.Settings.Default.Groups = JsonConvert.SerializeObject(richTextBox1.Lines);
            Properties.Settings.Default.Save();
        }
    }
}
