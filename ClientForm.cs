using System;
using System.Windows.Forms;
using WembleyManagementSystem;

namespace WembleyManagementSystem
{
    public partial class ClientForm : Form
    {
        public ClientForm()
        {
            InitializeComponent();

            // Add AIChatBox
            var aiChatBox = new AIChatBox("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"); 
            aiChatBox.Dock = DockStyle.Right;
            this.Controls.Add(aiChatBox);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ClientForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "ClientForm";
            this.Text = "Client Portal";
            this.ResumeLayout(false);
        }
    }
}