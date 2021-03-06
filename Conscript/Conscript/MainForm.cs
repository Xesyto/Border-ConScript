﻿using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Conscript.Borderscript;

namespace Conscript
{
    public partial class MainForm : Form
    {
        private BorderscriptEncoder _encoder;
        private BorderScriptHinter _hinter;
        private BorderscriptDecoder _decoder;

        private RichTextBox _editRichTextBox;

        private int _previousInputTextLengt;
        private bool _isEncoding;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _encoder = new BorderscriptEncoder();
            _decoder = new BorderscriptDecoder();
            _hinter = new BorderScriptHinter();

            _editRichTextBox = new RichTextBox();
            _editRichTextBox.WordWrap = false;

            _isEncoding = true;
            inputBoxLabel.Text = _encoder.inputLabel;
            outputBoxLabel.Text = _encoder.outputLabel;
            
            this.Show();

            inputTextBox.Select();
        }

        private void InputTextChanged(object sender, EventArgs e)
        {
            if (_isEncoding)
            {
                outputTextBox.Text = _encoder.Encode(inputTextBox.Text);
                HighLightText();
            }
            else
            {
                outputTextBox.Text = _decoder.Decode(inputTextBox.Text);
            }                        
        }

        private void HighLightText()
        {
            var selectionStart = inputTextBox.SelectionStart;

            _editRichTextBox.Rtf = inputTextBox.Rtf;
            _editRichTextBox.SelectionStart = selectionStart;

            if (inputTextBox.TextLength - _previousInputTextLengt > 1)
            {
                HighLightAllText();
            }
            else
            {
                HighLightTextLine();
            }

            _previousInputTextLengt = inputTextBox.TextLength;

            inputTextBox.Rtf = _editRichTextBox.Rtf;
            inputTextBox.SelectionStart = selectionStart;
        }


        private void HighLightTextLine(int? line = null)
        {
            var currentLine = line == null 
                ? _editRichTextBox.GetFirstCharIndexOfCurrentLine() 
                : _editRichTextBox.GetFirstCharIndexFromLine(line.Value);
                        
            if (line == null)
            {

                line = _editRichTextBox.GetLineFromCharIndex(currentLine);
            }

            if (_editRichTextBox.Lines.Length <= line.Value)
            {
                return;
            }
            
            var originalCursorLocation = _editRichTextBox.SelectionStart;

            //Color whole line box black
            _editRichTextBox.Select(currentLine, _editRichTextBox.Lines[line.Value].Length);
            _editRichTextBox.SelectionColor = Color.Black;

            var words = Regex.Matches(inputTextBox.Text, @"\w+");
            foreach (Match wordMatch in words)
            {
                var word = wordMatch.ToString();
                if (!_hinter.IsValid(word.ToLower()))
                {
                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        ColorWordInInput(word, currentLine);
                    }

                }
            }

            //Return to defaults
            _editRichTextBox.SelectionStart = originalCursorLocation;
            _editRichTextBox.SelectionLength = 0;
            _editRichTextBox.SelectionColor = Color.Black;
        }

        private void HighLightAllText()
        {
            int i = 0;

            foreach (var line in _editRichTextBox.Lines)
            {
                HighLightTextLine(i);
                i++;
            }
        }

        private void ColorWordInInput(string word,int startIndex = 0)
        {
            var wordIndex = _editRichTextBox.Find(word, startIndex, RichTextBoxFinds.WholeWord);
            var wordLength = word.Length;

            if (wordIndex == -1)
                return;

            _editRichTextBox.Select(wordIndex,wordLength);
            _editRichTextBox.SelectionColor = Color.Red;                        
        }

        private void encodeDecodeSwitch_Click(object sender, EventArgs e)
        {
            if (_isEncoding)
            {
                _isEncoding = false;
                inputBoxLabel.Text = _encoder.outputLabel;
                outputBoxLabel.Text = _encoder.inputLabel;
                outputTextBox.WordWrap = true;
            }
            else
            {
                _isEncoding = true;
                inputBoxLabel.Text = _encoder.inputLabel;
                outputBoxLabel.Text = _encoder.outputLabel;
                outputTextBox.WordWrap = _encoder.outputWordWrap;
            }

            var text = outputTextBox.Text;
            outputTextBox.Text = "";
            inputTextBox.Text = text;
        }
    }
}
