using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WembleyManagementSystem
{
    public class AIChatBox : UserControl
    {
        private Panel panel = new Panel();
        private TextBox txtChat = new TextBox();
        private TextBox txtInput = new TextBox();
        private Button btnSend = new Button();

        private string apiKey;

        public AIChatBox(string openAiKey)
        {
            apiKey = openAiKey;
            InitializeUI();
        }

        private void InitializeUI()
        {
            panel.Dock = DockStyle.Fill;
            this.Controls.Add(panel);

            // Chat history
            txtChat.Multiline = true;
            txtChat.ReadOnly = true;
            txtChat.ScrollBars = ScrollBars.Vertical;
            txtChat.SetBounds(10, 10, 260, 300);
            panel.Controls.Add(txtChat);

            // Input
            txtInput.SetBounds(10, 320, 180, 30);
            panel.Controls.Add(txtInput);

            // Send button
            btnSend.Text = "Send";
            btnSend.SetBounds(200, 320, 70, 30);
            btnSend.Click += BtnSend_Click;
            panel.Controls.Add(btnSend);
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            string userMessage = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(userMessage)) return;

            txtChat.AppendText("You: " + userMessage + Environment.NewLine);
            txtInput.Clear();
            btnSend.Enabled = false;

            try
            {
                string response = await SendToOpenAI(userMessage);
                txtChat.AppendText("AI: " + response + Environment.NewLine);
            }
            catch (Exception ex)
            {
                txtChat.AppendText("Error: " + ex.Message + Environment.NewLine);
            }

            txtChat.SelectionStart = txtChat.Text.Length;
            txtChat.ScrollToCaret();
            btnSend.Enabled = true;
        }

        private async Task<string> SendToOpenAI(string message)
        {
            // If no API key → simulate
            if (string.IsNullOrEmpty(apiKey))
            {
                await Task.Delay(300);
                return "Simulated response (no API key)";
            }

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "user", content = message }
                    },
                    max_tokens = 150
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "https://api.openai.com/v1/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                    return $"Error: {response.StatusCode}";

                var responseJson = await response.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(responseJson);

                string reply = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return reply.Trim();
            }
        }
    }
}