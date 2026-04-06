using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WembleyManagementSystem
{
    public class AIChatBox : UserControl
    {
        // Wembley brand colours
        private static readonly Color NavyDark = Color.FromArgb(0, 40, 90);
        private static readonly Color NavyMid = Color.FromArgb(0, 55, 115);
        private static readonly Color Gold = Color.FromArgb(255, 190, 0);
        private static readonly Color BgPanel = Color.FromArgb(245, 247, 252);
        private static readonly Color BgUserMsg = Color.FromArgb(0, 55, 115);
        private static readonly Color BgBotMsg = Color.FromArgb(228, 235, 255);
        private static readonly Color BorderCol = Color.FromArgb(210, 220, 240);

        // Controls
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Panel pnlChat;
        private FlowLayoutPanel flowMessages;
        private Panel pnlInput;
        private TextBox txtInput;
        private Button btnSend;
        private Label lblTyping;

        // State
        private readonly string _apiKey;
        private static readonly HttpClient _http = new HttpClient();
        private readonly List<object> _history = new List<object>();

        private const string PLACEHOLDER = "Ask something...";

        private const string SYSTEM_PROMPT =
            "You are a helpful assistant for the Wembley Events portal. " +
            "Help users find events, understand ticket prices, and answer questions " +
            "about Wembley Stadium. Keep answers short and friendly.";

        public AIChatBox(string anthropicApiKey)
        {
            _apiKey = anthropicApiKey;
            this.Width = 300;
            this.BackColor = BgPanel;
            BuildUI();
            AppendBotBubble("Hi! I'm your Wembley assistant. Ask me anything about events or tickets.");
        }

        // UI
        private void BuildUI()
        {
            // Header
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = NavyMid
            };

            Panel accent = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = Gold
            };
            pnlHeader.Controls.Add(accent);

            lblTitle = new Label
            {
                Text = "AI Assistant",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 10),
                Size = new Size(200, 18),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblTitle);

            lblSubtitle = new Label
            {
                Text = "Powered by Claude",
                Font = new Font("Segoe UI", 7),
                ForeColor = Color.FromArgb(160, 195, 255),
                Location = new Point(12, 30),
                Size = new Size(200, 14),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblSubtitle);

            // Input panel
            pnlInput = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 78,
                BackColor = Color.White,
                Padding = new Padding(8)
            };
            pnlInput.Paint += PnlInput_Paint;

            // Simulated placeholder textbox
            txtInput = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.None,
                Location = new Point(8, 8),
                Size = new Size(this.Width - 24, 36),
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle,
                ForeColor = Color.Gray,
                Text = PLACEHOLDER,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            txtInput.GotFocus += TxtInput_GotFocus;
            txtInput.LostFocus += TxtInput_LostFocus;
            txtInput.KeyDown += TxtInput_KeyDown;
            pnlInput.Controls.Add(txtInput);

            btnSend = new Button
            {
                Text = "Send",
                Location = new Point(this.Width - 90, 50),
                Size = new Size(74, 22),
                BackColor = NavyMid,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.FlatAppearance.MouseOverBackColor = Gold;
            btnSend.Click += (s, e) => SendMessage();
            pnlInput.Controls.Add(btnSend);

            lblTyping = new Label
            {
                Text = "Chat is thinking...",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.FromArgb(120, 140, 180),
                Location = new Point(8, 54),
                Size = new Size(180, 18),
                Visible = false
            };
            pnlInput.Controls.Add(lblTyping);

            // Scrollable chat area
            pnlChat = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgPanel,
                AutoScroll = true
            };

            flowMessages = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = BgPanel,
                Padding = new Padding(8)
            };
            flowMessages.Layout += (s, e) =>
                pnlChat.AutoScrollPosition = new Point(0, flowMessages.Height);

            pnlChat.Controls.Add(flowMessages);

            // Fill must be added before Top/Bottom
            this.Controls.Add(pnlChat);
            this.Controls.Add(pnlInput);
            this.Controls.Add(pnlHeader);

            this.Paint += This_Paint;
        }

        //Event handlers 

        private void PnlInput_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(BorderCol, 1))
            {
                e.Graphics.DrawLine(pen, 0, 0, pnlInput.Width, 0);
            }
        }

        private void This_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(BorderCol, 1))
            {
                e.Graphics.DrawLine(pen, 0, 0, 0, this.Height);
            }
        }

        private void TxtInput_GotFocus(object sender, EventArgs e)
        {
            if (txtInput.Text == PLACEHOLDER)
            {
                txtInput.Text = "";
                txtInput.ForeColor = Color.Black;
            }
        }

        private void TxtInput_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                txtInput.Text = PLACEHOLDER;
                txtInput.ForeColor = Color.Gray;
            }
        }

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                SendMessage();
            }
        }

        //  Bubbles 

        private void AppendUserBubble(string text) { AppendBubble(text, true); }
        private void AppendBotBubble(string text) { AppendBubble(text, false); }

        private void AppendBubble(string text, bool isUser)
        {
            int innerW = Math.Max(
                flowMessages.ClientSize.Width - flowMessages.Padding.Horizontal - 16, 80);
            int maxBubW = (int)(innerW * 0.85);

            Font font = new Font("Segoe UI", 9);

            Size measured = TextRenderer.MeasureText(
                text, font,
                new Size(maxBubW - 16, int.MaxValue),
                TextFormatFlags.WordBreak);

            int bubW = Math.Min(measured.Width + 20, maxBubW);
            int bubH = measured.Height + 14;

            Panel row = new Panel
            {
                Width = innerW,
                Height = bubH + 4,
                BackColor = BgPanel,
                Margin = new Padding(0, 3, 0, 0)
            };

            Label bub = new Label
            {
                Width = bubW,
                Height = bubH,
                BackColor = isUser ? BgUserMsg : BgBotMsg,
                ForeColor = isUser ? Color.White : Color.FromArgb(20, 30, 60),
                Font = font,
                Tag = text
            };
            bub.Paint += Bubble_Paint;

            bub.Location = isUser
                ? new Point(innerW - bubW, 0)
                : new Point(0, 0);

            row.Controls.Add(bub);

            if (flowMessages.InvokeRequired)
                flowMessages.Invoke((Action)(() => flowMessages.Controls.Add(row)));
            else
                flowMessages.Controls.Add(row);
        }

        private void Bubble_Paint(object sender, PaintEventArgs pe)
        {
            Label lbl = (Label)sender;
            Rectangle rc = new Rectangle(0, 0, lbl.Width, lbl.Height);

            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = RoundedRect(rc, 9))
            {
                using (SolidBrush brush = new SolidBrush(lbl.BackColor))
                {
                    pe.Graphics.FillPath(brush, path);
                }
            }

            TextRenderer.DrawText(
                pe.Graphics,
                (string)lbl.Tag,
                lbl.Font,
                new Rectangle(8, 6, lbl.Width - 16, lbl.Height - 12),
                lbl.ForeColor,
                TextFormatFlags.WordBreak);
        }

        //Send

        private async void SendMessage()
        {
            string text = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(text) || text == PLACEHOLDER) return;

            txtInput.Text = PLACEHOLDER;
            txtInput.ForeColor = Color.Gray;
            btnSend.Enabled = false;
            lblTyping.Visible = true;

            AppendUserBubble(text);
            _history.Add(new { role = "user", content = text });

            try
            {
                string reply = await CallClaudeAsync();
                AppendBotBubble(reply);
                _history.Add(new { role = "assistant", content = reply });
            }
            catch (Exception ex)
            {
                AppendBotBubble("Sorry, I couldn't reach the AI. (" + ex.Message + ")");
            }
            finally
            {
                btnSend.Enabled = true;
                lblTyping.Visible = false;
            }
        }

        //API

        private async Task<string> CallClaudeAsync()
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                await Task.Delay(300);
                return "Simulated response (no API key provided).";
            }

            var body = new
            {
                model = "claude-sonnet-4-20250514",
                max_tokens = 512,
                system = SYSTEM_PROMPT,
                messages = _history
            };

            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _http.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception("API " + response.StatusCode + ": " + responseBody);

            StringBuilder sb = new StringBuilder();

            using (JsonDocument doc = JsonDocument.Parse(responseBody))
            {
                foreach (JsonElement block in doc.RootElement
                             .GetProperty("content").EnumerateArray())
                {
                    JsonElement t;
                    if (block.TryGetProperty("type", out t) && t.GetString() == "text")
                    {
                        sb.Append(block.GetProperty("text").GetString());
                    }
                }
            }

            return sb.ToString().Trim();
        }

        // Helpers

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
