using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tesseract;

namespace FindeTextInImage
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private RichTextBox richText;
        private string pattern = ".*";
        private void ClearHighlighting()
        {
            // Reset the RichTextBox's selection and background color for all text
            richText.SelectionStart = 0;
            richText.SelectionLength = richText.TextLength;
            richText.SelectionBackColor = Color.White;
        }
        TextBox txtReplaceCharFor;
        MatchCollection matches;
        private void btnConvert_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image File";
                dlg.Filter = "Image Files (*.bmp;*.jpg;*.jpeg,*.png)|*.BMP;*.JPG;*.JPEG;*.PNG";

                if (dlg.ShowDialog() == DialogResult.OK)
                {

                    var ocr = new TesseractEngine("./tessdata", "eng");

                    //ocr.SetVariable("tessedit_pageseg_mode", "1"); // 6 means PageSegMode.SingleLine
                    //ocr.SetVariable("tessedit_create_hocr", "1"); // Output HTML format
                    //using (var image = new Bitmap(dlg.FileName))
                    using (var image = Pix.LoadFromFile(dlg.FileName))
                    {
                        using (var page = ocr.Process(image))
                        {
                            var outputText = page.GetText();
                            //var outputFile = Path.Combine(Path.GetDirectoryName(dlg.FileName), "OCRResult.txt");
                            //File.WriteAllText(outputFile, outputText);

                            // Create a new message box with the output text and a Copy button
                            var msgBox = new Form()
                            {
                                Text = "OCR Output",
                                Size = new Size(400, 480),
                                ControlBox = false,
                                FormBorderStyle = FormBorderStyle.FixedSingle
                            };
                            var label = new Label()
                            {
                                Text = "Text from the image:",
                                Location = new Point(20, 20),
                                AutoSize = true
                            };
                            richText = new RichTextBox()
                            {
                                Text = outputText,
                                Location = new Point(17, 40),
                                Size = new Size(350, 310),
                                Multiline = true,
                                ReadOnly = false
                            };
                            //Replace char
                            var labelChar = new Label()
                            {
                                Text = "Char:",
                                Location = new Point(18, 363),
                                AutoSize = true
                            };
                            var txtReplaceChar = new TextBox()
                            {
                                Text = "",
                                Location = new Point(60, 360),
                                Size = new Size(55, 25),
                                Multiline = true,
                                ReadOnly = false
                            };
                            //Replace char For
                            var labelCharFor = new Label()
                            {
                                Text = "For:",
                                Location = new Point(115, 363),
                                AutoSize = true
                            };
                            txtReplaceCharFor = new TextBox()
                            {
                                Text = "",
                                Location = new Point(150, 360),
                                Size = new Size(55, 25),
                                Multiline = true,
                                ReadOnly = false
                            };
                            //Button to replace char
                            var btnReplace = new Button()
                            {
                                Text = "Replace",
                                Location = new Point(215, 360),
                                Size = new Size(75, 30),
                            };
                            txtReplaceChar.TextChanged += (s, a) =>
                            {
                                ClearHighlighting();
                                // Reset the RichTextBox's selection and scroll position
                                richText.SelectionStart = 0;
                                richText.SelectionLength = 0;
                                richText.ScrollToCaret();
                                // Check if the input text ends with .*
                                if (txtReplaceChar.Text.EndsWith(".*"))
                                {
                                    // Create a regular expression pattern using the input text
                                    string pattern = $"{Regex.Escape(txtReplaceChar.Text.Substring(0, txtReplaceChar.Text.Length - 2))}.*$";
                                    // Find all occurrences of the pattern in the RichTextBox
                                    matches = Regex.Matches(richText.Text, pattern, RegexOptions.Multiline);
                                    foreach (Match match in matches)
                                    {
                                        int startIndex = match.Index;
                                        int endIndex = richText.Text.IndexOf('\n', startIndex);
                                        if (endIndex < 0) endIndex = richText.TextLength;
                                        int length = endIndex - startIndex;
                                        richText.Select(startIndex, length);
                                        richText.SelectionBackColor = Color.Yellow;
                                    }
                                }
                                else
                                {
                                    ClearHighlighting();
                                    // Reset the RichTextBox's selection and scroll position
                                    richText.SelectionStart = 0;
                                    richText.SelectionLength = 0;
                                    richText.ScrollToCaret();
                                    //SetRegexValidation(richText, pattern);
                                    // Find all occurrences of the search text in the RichTextBox
                                    int index = 0;
                                    while (index < richText.TextLength)
                                    {
                                        index = richText.Find(txtReplaceChar.Text, index, RichTextBoxFinds.None);
                                        if (index != -1)
                                        {
                                            richText.SelectionStart = index;
                                            richText.SelectionLength = txtReplaceChar.Text.Length;
                                            richText.SelectionBackColor = Color.Yellow;
                                            index += txtReplaceChar.Text.Length;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            };
                            btnReplace.Click += (s, a) =>
                            {
                                msgBox.Closing += (sender, e) => e.Cancel = false;
                                // Iterate through the highlighted ranges and replace the text within those ranges
                                if (txtReplaceChar.Text.EndsWith(".*"))
                                {
                                    for (int i = matches.Count - 1; i >= 0; i--)
                                    {
                                        Match match = matches[i];
                                        int startIndex = match.Index;
                                        int endIndex = richText.Text.IndexOf('\n', startIndex);
                                        if (endIndex < 0) endIndex = richText.TextLength;
                                        int length = endIndex - startIndex;
                                        richText.Select(startIndex, length);
                                        richText.SelectedText = txtReplaceCharFor.Text;
                                    }
                                }
                                else
                                {
                                    msgBox.Closing += (sender, e) => e.Cancel = false;
                                    // Replace all occurrences of the search text with the replace character
                                    richText.Text = richText.Text.Replace(txtReplaceChar.Text, txtReplaceCharFor.Text);
                                }


                                // Clear the highlighting
                                ClearHighlighting();
                            };


                            var btnClose = new Button()
                            {
                                Text = "Close",
                                Location = new Point(100, 400),
                                Size = new Size(90, 30),
                                DialogResult = DialogResult.OK
                            };
                            btnClose.Click += (s, a) =>
                            {
                                msgBox.Closing += (sender, e) => e.Cancel = false;
                            };
                            var btnCopy = new Button()
                            {
                                Text = "Copy",
                                Location = new Point(200, 400),
                                Size = new Size(90, 30),
                                DialogResult = DialogResult.OK

                            };
                            btnCopy.Click += (s, a) =>
                            {

                                Clipboard.SetText(outputText);
                                richText.Focus(); // set focus to the RichTextBox control
                                richText.SelectAll();
                                msgBox.Closing += (sender, e) => e.Cancel = true;
                            };
                            /*  var btnOpen = new Button()
                              {
                                  Text = "Open",
                                  Location = new Point(250, 400),
                                  Size = new Size(90, 30),
                                  DialogResult = DialogResult.OK
                              };
                              btnOpen.Click += (s, a) =>
                              {
                                  Clipboard.SetText(outputText);
                                  msgBox.Closing += (sender, e) => e.Cancel = false;
                              };*/

                            msgBox.AcceptButton = btnCopy;
                            msgBox.CancelButton = btnClose;
                            msgBox.StartPosition = FormStartPosition.CenterScreen;

                            msgBox.Controls.Add(label);
                            msgBox.Controls.Add(richText);
                            msgBox.Controls.Add(labelChar);
                            msgBox.Controls.Add(labelCharFor);
                            msgBox.Controls.Add(txtReplaceChar);
                            msgBox.Controls.Add(txtReplaceCharFor);
                            msgBox.Controls.Add(btnReplace);
                            msgBox.Controls.Add(btnClose);
                            msgBox.Controls.Add(btnCopy);
                            //msgBox.Controls.Add(btnOpen);

                            // Show the message box and wait for the user to close it
                            msgBox.ShowDialog();
                        }
                    }

                }
            }
        }
        private void SetRegexValidation(RichTextBox richTextBox, string pattern, Color? highlightColor = null)
        {
            if (string.IsNullOrEmpty(pattern)) return;

            var regex = new Regex(pattern);
            var matches = regex.Matches(richTextBox.Text);

            foreach (Match match in matches)
            {
                richTextBox.Select(match.Index, match.Length);
                richTextBox.SelectionBackColor = highlightColor ?? Color.Yellow;
            }
        }
    }
}
