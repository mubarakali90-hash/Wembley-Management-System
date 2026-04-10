using System;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using WembleyManagementSystem;
using AdminUser;

public class AIChatBox : UserControl
{
    private Panel aiPanel = new Panel();
    private Label lblTitle = new Label();
    private TextBox txtChatHistory = new TextBox();
    private TextBox txtInput = new TextBox();
    private Button btnSend = new Button();

    private string aiApiKey;

    // Added properties to allow the AI to manipulate data
    public EventManagementSystem EventSystem { get; set; }
    public User CurrentUser { get; set; }

    public AIChatBox(string apiKey)
    {
        aiApiKey = apiKey;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        aiPanel.Dock = DockStyle.Fill;
        aiPanel.BackColor = Color.FromArgb(240, 240, 240);
        this.Controls.Add(aiPanel);

        lblTitle.Text = "AI Assistant";
        lblTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        lblTitle.Location = new Point(10, 10);
        lblTitle.Size = new Size(280, 20);
        aiPanel.Controls.Add(lblTitle);

        txtChatHistory.Location = new Point(10, 40);
        txtChatHistory.Size = new Size(280, 280);
        txtChatHistory.Multiline = true;
        txtChatHistory.ScrollBars = ScrollBars.Vertical;
        txtChatHistory.ReadOnly = true;
        txtChatHistory.BackColor = Color.White;
        aiPanel.Controls.Add(txtChatHistory);

        txtInput.Location = new Point(10, 330);
        txtInput.Size = new Size(200, 26);
        aiPanel.Controls.Add(txtInput);

        btnSend.Text = "Send";
        btnSend.Location = new Point(215, 329);
        btnSend.Size = new Size(75, 28);
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

            // Intercept JSON commands from the AI
            int jsonStartIndex = aiResponse.IndexOf("```json");
            if (jsonStartIndex >= 0)
            {
                int jsonEndIndex = aiResponse.IndexOf("```", jsonStartIndex + 7);
                if (jsonEndIndex > jsonStartIndex)
                {
                    string jsonCommand = aiResponse.Substring(jsonStartIndex + 7, jsonEndIndex - (jsonStartIndex + 7)).Trim();

                    // Execute the data manipulation
                    ExecuteAIActions(jsonCommand);

                    // Clean the response for the user
                    aiResponse = aiResponse.Substring(0, jsonStartIndex).Trim() + "\n✅ Actions executed successfully!";
                }
            }

            txtChatHistory.AppendText("AI: " + aiResponse + Environment.NewLine + Environment.NewLine);
        }
        catch (Exception ex)
        {
            txtChatHistory.AppendText("Error: " + ex.Message + Environment.NewLine);
        }

        txtChatHistory.SelectionStart = txtChatHistory.Text.Length;
        txtChatHistory.ScrollToCaret();
        btnSend.Enabled = true;
    }

    private void ExecuteAIActions(string json)
    {
        if (EventSystem == null || CurrentUser == null) return;

        try
        {
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                if (doc.RootElement.TryGetProperty("actions", out JsonElement actions))
                {
                    foreach (JsonElement action in actions.EnumerateArray())
                    {
                        string type = action.GetProperty("type").GetString();

                        if (type == "CREATE")
                        {
                            WembleyEvent newEvent = new WembleyEvent(
                                0,
                                CurrentUser.UserID,
                                action.GetProperty("eventName").GetString(),
                                DateTime.Parse(action.GetProperty("eventDate").GetString()),
                                action.GetProperty("eventType").GetString(),
                                0,
                                action.GetProperty("eventPrice").GetInt32()
                            );
                            EventSystem.AddEvent(newEvent);
                        }
                        else if (type == "UPDATE")
                        {
                            int eventId = action.GetProperty("eventId").GetInt32();
                            WembleyEvent updatedEvent = EventSystem.GetEvent(eventId);

                            // Security: Business can only edit their own
                            if (updatedEvent != null && (CurrentUser.UserRole == "Admin" || updatedEvent.BusinessID == CurrentUser.UserID))
                            {
                                if (action.TryGetProperty("eventName", out JsonElement val)) updatedEvent.EventName = val.GetString();
                                if (action.TryGetProperty("eventDate", out JsonElement dVal)) updatedEvent.EventDate = DateTime.Parse(dVal.GetString());
                                if (action.TryGetProperty("eventType", out JsonElement tVal)) updatedEvent.EventType = tVal.GetString();
                                if (action.TryGetProperty("eventPrice", out JsonElement pVal)) updatedEvent.EventPrice = pVal.GetInt32();

                                EventSystem.UpdateEvent(eventId, updatedEvent);
                            }
                        }
                        else if (type == "DELETE")
                        {
                            int eventId = action.GetProperty("eventId").GetInt32();
                            WembleyEvent eventToDelete = EventSystem.GetEvent(eventId);

                            // Admin can delete any, Business can only delete their own
                            if (eventToDelete != null && (CurrentUser.UserRole == "Admin" || eventToDelete.BusinessID == CurrentUser.UserID))
                            {
                                EventSystem.DeleteEvent(eventId);
                            }
                        }
                    }
                }
            }

            // Force the AdminBusinessForm grid to refresh instantly
            if (this.Parent is AdminBusinessForm adminForm)
            {
                adminForm.RefreshGrid();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to parse AI action: " + ex.Message);
        }
    }

    private async Task<string> SendMessageToAIAsync(string userMessage)
    {
        if (string.IsNullOrEmpty(aiApiKey)) return "Simulated AI response (no API key).";

        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("x-api-key", aiApiKey);
            httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            string eventContext = "";
            bool isBusinessMode = (CurrentUser != null && (CurrentUser.UserRole == "Admin" || CurrentUser.UserRole == "Verified_Business"));

            // Gather event context securely
            if (EventSystem != null)
            {
                foreach (var ev in EventSystem.GetAllEvents())
                {
                    if (ev != null)
                    {
                        // Security check: If user is a business (not an Admin), skip events they don't own
                        if (CurrentUser != null && CurrentUser.UserRole == "Verified_Business" && ev.BusinessID != CurrentUser.UserID)
                        {
                            continue; // Do not feed this event to the AI
                        }

                        // In business mode, give the AI the EventID so it knows what to delete/update
                        if (isBusinessMode)
                            eventContext += $"- [ID:{ev.EventID}] {ev.EventName} | {ev.EventDate:yyyy-MM-dd} | {ev.EventType} | £{ev.EventPrice} | Att: {ev.Attendance}\n";
                        else
                            eventContext += $"- {ev.EventName} | {ev.EventDate:dd/MM/yyyy} | {ev.EventType} | £{ev.EventPrice}\n";
                    }
                }
            }

            string systemPrompt = "";

            if (isBusinessMode)
            {
                // Prompt Engineering for the Business Agent
                systemPrompt = $@"You are a Wembley Stadium Business AI Assistant. 
Current events database:
{eventContext}

If the user asks to CREATE, UPDATE, or DELETE events, you MUST output a JSON array of actions wrapped in ```json blocks. 
If the user's request is ambiguous, misses details (Name, Date, Type, Price for creation), or spelling is wrong, DO NOT output JSON. Instead, ask them for clarification in plain text.
Supported Event Types: Football, Concert, Comedy, Other.

Format exactly like this example if actions are requested:
```json
{{
  ""actions"": [
    {{ ""type"": ""CREATE"", ""eventName"": ""Coldplay"", ""eventDate"": ""2026-08-10T19:00:00"", ""eventType"": ""Concert"", ""eventPrice"": 150 }},
    {{ ""type"": ""UPDATE"", ""eventId"": 123, ""eventPrice"": 100 }},
    {{ ""type"": ""DELETE"", ""eventId"": 124 }}
  ]
}}
You can execute multiple actions in one JSON block. Translate date range deletions into individual DELETE commands for specific matching eventIds based on the database.";
            }
            else
            {
                // Standard Prompt for Clients
                systemPrompt = $"You are a helpful assistant for Wembley Stadium events. Only answer questions about these events:\n{eventContext}\nPolitely redirect unrelated questions.";
            }

            var requestBody = new
            {
                model = "claude-sonnet-4-20250514",
                max_tokens = 500, // Increased slightly to accommodate Business Agent
                system = systemPrompt,
                messages = new[] { new { role = "user", content = userMessage } }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var jsonContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://api.anthropic.com/v1/messages", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(responseJson);

                // Traverse the JSON to prevent crashes if the API format shifts
                if (doc.RootElement.TryGetProperty("content", out JsonElement contentArray) && contentArray.GetArrayLength() > 0)
                {
                    if (contentArray[0].TryGetProperty("text", out JsonElement textElement))
                    {
                        return textElement.GetString() ?? "No response.";
                    }
                }
                return "AI returned an unexpected format.";
            }
            else
            {
                // This guarantees a string is returned even if the API call fails
                string errorBody = await response.Content.ReadAsStringAsync();
                return $"Error {response.StatusCode}: {errorBody}";
            }
        }
    }

    private void AIChatBox_Load(object sender, EventArgs e) { }
}