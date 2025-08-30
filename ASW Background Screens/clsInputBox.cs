using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASW_Background_Screens
{
    public static class clsInputBox
    {

        public static string OriginalShowMoreHeight(string prompt, string title = "Input", string defaultValue = "")
        {
            int labelHeight = 100;  //was 20 originally
            int spacer = 20;
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = prompt;
            textBox.Text = defaultValue;

            // textBox.Multiline = true;

            label.SetBounds(10, 5, 370, labelHeight);
            textBox.SetBounds(10, label.Top + label.Height + spacer, 370, 20);
            buttonOk.SetBounds(220, label.Height + spacer
                    + textBox.Height + spacer, 75, 23);
            buttonCancel.SetBounds(305, label.Height + spacer
                    + textBox.Height + spacer, 75, 23);

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            form.ClientSize = new System.Drawing.Size(400, label.Top + label.Height
                                + spacer + textBox.Height + spacer
                              + buttonOk.Height + spacer);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.ShowInTaskbar = false;

            return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;

        }
        public static string OriginalShow(string prompt, string title = "Input", string defaultValue = "")
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = prompt;
            textBox.Text = defaultValue;

            label.SetBounds(10, 10, 370, 13);
            textBox.SetBounds(10, 30, 370, 20);
            buttonOk.SetBounds(220, 60, 75, 23);
            buttonCancel.SetBounds(305, 60, 75, 23);

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            form.ClientSize = new System.Drawing.Size(400, 100);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.ShowInTaskbar = false;

            return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }

        public static UserInputResult DateAndTextBoxShow(string prompt, string title = "Input", string defaultDateValue = ""
            , string defaultDescription = "")
        {
            Form form = new Form();
            Label label = new Label();
            Label labelDateRemoved = new Label();
            Label labelDescription = new Label();
            TextBox textBoxDate = new TextBox();
            TextBox textBoxDescription = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            Button buttonJustDelete = new Button();

            bool bJustDeleteFlag = false;

            buttonJustDelete.Click += (sender, e) =>
            {
                bJustDeleteFlag = true;
                form.Close();
            };

            form.Text = title;
            label.Text = prompt;
            labelDateRemoved.Text = "Date Removed";
            labelDescription.Text = "Description";
            textBoxDate.Text = defaultDateValue;
            textBoxDescription.Text = defaultDescription;

            label.SetBounds(10, 10, 370, 85);
            labelDateRemoved.SetBounds(10, label.Top + label.Height + 15, 85, 26);
            textBoxDate.SetBounds(labelDateRemoved.Left + labelDateRemoved.Width + 3, labelDateRemoved.Top, 280, 20);
            labelDescription.SetBounds(10, textBoxDate.Top + textBoxDate.Height + 5, 85, 26);
            textBoxDescription.SetBounds(labelDescription.Left + labelDescription.Width + 3, labelDescription.Top, 280, 20);
            buttonOk.SetBounds(10, textBoxDescription.Top + textBoxDescription.Height + 10, 75, 23);
            buttonCancel.SetBounds(buttonOk.Left + buttonOk.Width + 5, buttonOk.Top, 75, 23);
            buttonJustDelete.SetBounds(buttonCancel.Left + buttonCancel.Width + 5, buttonCancel.Top, 75, 23);

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonJustDelete.Text = "Just Delete it.";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;


            form.ClientSize = new System.Drawing.Size(425, 200);
            form.Controls.AddRange(new Control[] { label, labelDateRemoved, labelDescription,
            textBoxDate, textBoxDescription, buttonOk,
            buttonCancel, buttonJustDelete });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;


            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.ShowInTaskbar = false;

            var dialogResult = form.ShowDialog();

            // return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
            return new UserInputResult
            {
                DialogResult = dialogResult,
                JustDeleteIt = bJustDeleteFlag,
                TextBoxDate = textBoxDate.Text,
                TextBoxDescription = textBoxDescription.Text,
            };
        }
        public static string LargeBoxShow(string prompt, string title = "Input", string defaultValue = "")
        {
            int formHeight = 400;
            int formWidth = 500;
            int textboxHeight = 300;
            int textboxWidth = 450;

            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            textBox.Multiline = true;
            textBox.Font = new Font(textBox.Font.FontFamily, 16); ;

            form.Text = title;
            label.Text = prompt;
            textBox.Text = defaultValue;

            form.ClientSize = new System.Drawing.Size(formWidth, formHeight);

            // Label near top, full width minus padding
            label.SetBounds(10, 10, formWidth - 10, 20);

            // TextBox just below label, using textboxHeight/Width
            textBox.SetBounds(10, 40, textboxWidth, textboxHeight);

            int buttonY = 50 + textboxHeight;

            buttonOk.SetBounds(formWidth - 200, textBox.Top + textBox.Height + 10, 75, 30);
            buttonCancel.SetBounds(buttonOk.Left + buttonOk.Width + 5, buttonY, 75, 30);

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.ShowInTaskbar = false;

            return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }
    }
}
