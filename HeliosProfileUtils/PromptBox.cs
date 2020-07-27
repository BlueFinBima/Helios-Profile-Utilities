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
    public static partial class PromptBox
    {
        public static Boolean ShowDialog(string title, string message, string leftButtonText, string rightButtonText)
        {
            Form promptBox = new Form()
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
            TextBox textBox = new TextBox() { Left = 12, Top = 12, Width = 360, Height = 50, TabStop = false, BorderStyle = BorderStyle.None, Multiline = true, ReadOnly = true,
                Text = message};
            Button continueButton = new Button() { Text = rightButtonText, Left = 200, Width = 100, Top = 70, TabIndex = 1, DialogResult = DialogResult.OK };
            continueButton.Click += (sender, e) => { promptBox.Close(); };
            Button cancelButton = new Button() { Text = leftButtonText, Left = 50, Width = 100, Top = 70, TabIndex = 0, DialogResult = DialogResult.Cancel };
            cancelButton.Click += (sender, e) => { promptBox.Close(); };

            promptBox.Controls.Add(textBox);
            promptBox.Controls.Add(continueButton);
            promptBox.Controls.Add(cancelButton);
            promptBox.AcceptButton = continueButton;

            return promptBox.ShowDialog() == DialogResult.OK ? true : false;
        }
    }
}
