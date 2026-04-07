using System;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using WembleyManagementSystem;

public class AIChatBox : UserControl
{
    private Panel aiPanel = new Panel();
    private Label lblTitle = new Label();
    private TextBox txtChatHistory = new TextBox();
    private TextBox txtInput = new TextBox();
    private Button btnSend = new Button();

    private string aiApiKey; // AI API key

    public AIChatBox(string apiKey)
    {
        aiApiKey = apiKey;
        InitializeComponents();
    }
    private void InitializeComponents()
    {
        // Panel setup
        aiPanel.Dock = DockStyle.Fill;
        aiPanel.BackColor = Color.FromArgb(240, 240, 240);
        this.Controls.Add(aiPanel);

        // Title label
        lblTitle.Text = "AI Chat";
        lblTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        lblTitle.Location = new Point(10, 10);
        lblTitle.Size = new Size(280, 20);
        aiPanel.Controls.Add(lblTitle);

        // Chat history
        txtChatHistory.Location = new Point(10, 40);
        txtChatHistory.Size = new Size(280, 300);
        txtChatHistory.Multiline = true;
        txtChatHistory.ScrollBars = ScrollBars.Vertical;
        txtChatHistory.ReadOnly = true;
        txtChatHistory.BackColor = Color.White;
        aiPanel.Controls.Add(txtChatHistory);

        // Input box
        txtInput.Location = new Point(10, 350);
        txtInput.Size = new Size(200, 26);
        aiPanel.Controls.Add(txtInput);

        // Send button
        btnSend.Text = "Send";
        btnSend.Location = new Point(215, 350);
        btnSend.Size = new Size(75, 26);
        btnSend.BackColor = Color.FromArgb(0, 55, 115);
        btnSend.ForeColor = Color.White;
        btnSend.FlatStyle = FlatStyle.Flat;
        btnSend.FlatAppearance.BorderSize = 0;
        btnSend.Click += BtnSend_Click;
        aiPanel.Controls.Add(btnSend);
    }
    private async void BtnSend_Click(object sender, EventArgs e)
    {
        string userMessage = txtInput.Text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;

        txtChatHistory.AppendText("You: " + userMessage + Environment.NewLine);
        txtInput.Clear();
        btnSend.Enabled = false;

        try
        {
            string aiResponse = await SendMessageToAIAsync(userMessage);
            txtChatHistory.AppendText("AI: " + aiResponse + Environment.NewLine);
        }
        catch (Exception ex)
        {
            txtChatHistory.AppendText("Error: " + ex.Message + Environment.NewLine);
        }

        txtChatHistory.SelectionStart = txtChatHistory.Text.Length;
        txtChatHistory.ScrollToCaret();
        btnSend.Enabled = true;
    }

    private async Task<string> SendMessageToAIAsync(string userMessage)
    {
        if (string.IsNullOrEmpty(aiApiKey))
        {
            await Task.Delay(300);
            return "Simulated AI response (no API key provided).";
        }

        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("x-api-key", aiApiKey);
            httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            // Get all events to give the AI context
            string eventContext = "";
            if (this.Parent is ClientForm clientForm)
            {
                var events = clientForm.GetEvents();
                foreach (var ev in events)
                {
                    if (ev != null)
                    {
                        eventContext += $"- {ev.EventName} | {ev.EventDate:dd/MM/yyyy} | {ev.EventType} | £{ev.EventPrice} | Attendance: {ev.Attendance}\n";
                    }
                }
            }

            var requestBody = new
            {
                model = "claude-sonnet-4-20250514",
                max_tokens = 200,
                system = $"You are a helpful assistant for Wembley Stadium events. Only answer questions about the events listed below. If someone asks about something unrelated, politely redirect them to event-related topics, dont print all the events available if not requested\n\nCurrent events:\n{eventContext}",
                messages = new[]
                {
            new { role = "user", content = userMessage }
        }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var jsonContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.anthropic.com/v1/messages", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(responseJson);
                var contentArray = doc.RootElement.GetProperty("content");
                if (contentArray.GetArrayLength() > 0)
                {
                    var text = contentArray[0].GetProperty("text").GetString();
                    return text ?? "No response from AI.";
                }
                return "No response from AI.";
            }
            else
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                return $"Error {response.StatusCode}: {errorBody}";
            }
        }
    }
    private void InitializeComponent()
    {
            this.SuspendLayout();
            // 
            // AIChatBox
            // 
            this.Name = "AIChatBox";
            this.Load += new System.EventHandler(this.AIChatBox_Load);
            this.ResumeLayout(false);

    }

    private void AIChatBox_Load(object sender, EventArgs e)
    {

    }
}




