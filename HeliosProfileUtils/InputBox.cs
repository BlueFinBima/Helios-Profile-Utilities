using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeliosProfileUtils
{
    public static partial class InputBox
    {
        public static string ShowDialog(string title, string requestText, string message, string leftButtonText, string rightButtonText)
        {
            Form inputBox = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterScreen,
                AutoScaleMode = AutoScaleMode.Font,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                SizeGripStyle = SizeGripStyle.Hide
            };
            Label textLabel = new Label() { Left = 12, Top = 12, Width = 360, Text = requestText};
            TextBox textBox = new TextBox() { Left = 12, Top = 32, Width = 360, Height = 50, TabStop = false, BorderStyle = BorderStyle.FixedSingle, TabIndex = 0, Multiline = false, ReadOnly = false,
                Text = message};
            textBox.KeyDown += (sender, e) => { if (e.KeyData == Keys.Enter) { inputBox.DialogResult = DialogResult.OK; inputBox.Close(); } };
            Button continueButton = new Button() { Text = rightButtonText, Left = 200, Width = 100, Top = 70, TabIndex = 2, DialogResult = DialogResult.OK };
            continueButton.Click += (sender, e) => { inputBox.Close(); };
            Button cancelButton = new Button() { Text = leftButtonText, Left = 50, Width = 100, Top = 70, TabIndex = 1, DialogResult = DialogResult.Cancel };
            cancelButton.Click += (sender, e) => { inputBox.Close(); };

            inputBox.Controls.Add(textLabel);
            inputBox.Controls.Add(textBox);
            inputBox.Controls.Add(continueButton);
            inputBox.Controls.Add(cancelButton);
            inputBox.AcceptButton = continueButton;

            return inputBox.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
