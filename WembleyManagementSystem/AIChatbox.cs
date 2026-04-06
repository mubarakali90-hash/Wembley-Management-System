using System;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        await Task.Delay(300); // simulate delay
        return "Simulated AI response (no API key provided).";
    }

    using (var client = new HttpClient())
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", aiApiKey);

        var requestBody = new
        {
            model = "claude-2",
            prompt = userMessage,
            max_tokens_to_sample = 200
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.anthropic.com/v1/complete", content);

        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(responseJson);
            var completion = doc.RootElement.GetProperty("completion").GetString();
            return completion ?? "No response from AI.";
        }
        else
        {
            return $"Error: {response.StatusCode}";
        }
    }
}

