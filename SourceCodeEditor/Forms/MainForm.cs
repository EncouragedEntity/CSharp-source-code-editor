using FastColoredTextBoxNS;
using SourceCodeEditor.AppearenceConfig;
using SourceCodeEditor.Enums;
using SourceCodeEditor.Forms;
namespace SourceCodeEditor
{
    delegate DialogResult DialogRes(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);

    /// <summary>
    /// Main form of Application
    /// </summary>
    public partial class MainForm : Form
    {
        #region Fields
        private string _applicationName { get; set; } = "Salamanca";

        public Theme CurrentTheme { get; set; }
        private Theme DefaultTheme { get; set; } = Theme.Black;
        public CurrentTheme theme = new CurrentTheme();
        public Font DefaultTextFont { get; set; } = new Font(new FontFamily("Courier New"), 12);

        public WindowState StateOfWindow { get; set; } = Enums.WindowState.Windowed;
        public int DefaultZoom { get; set; } = 100;

        /// <summary>
        /// Current opened file
        /// </summary>
        private string _currentFile { get; set; } = String.Empty;
        /// <summary>
        /// Is file created on disk
        /// </summary>
        private bool _isFileCreated { get; set; } = false;
        /// <summary>
        /// Is file saved on disk
        /// </summary>
        private bool _isFileSaved { get; set; } = false;
        #endregion

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //Set toolStripMenuItem dropdown items from "MainTextField.Languages" on load
            new LanguageConfig(this).LoadLanguages();

            //Load hotkeys config from file on form load
            new HotKeysConfig(MainHeader).LoadHotkeysConfig();

            //Get default theme from file and apply it on load
            theme = ThemeSerializer.Deserialize<CurrentTheme>("Themes/BlackTheme.theme")!;
            theme.syntaxColors = ThemeSerializer.Deserialize<SyntaxColors>("SyntaxColors/BlackSyntax.syn");
            CurrentTheme = DefaultTheme;
            new ThemeChanger(this).ChangeTheme(CurrentTheme);
            DeleteUnnecessaryLabels();
        }

        #region Methods

        #region File

        private async Task ReadTextFromFile()
        {
            var astr = await File.ReadAllTextAsync(_currentFile);
            MainTextField.Text = astr;
        }

        /// <summary>
        /// Display opened file content to FastColoredTextBox
        /// </summary>
        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var op = openFileDialog1;
            op.FileName = "untitled.txt";
            if (op.ShowDialog() != DialogResult.OK) return;

            _isFileCreated = true;
            _currentFile = op.FileName;
            FileNameToFormText(_currentFile);

            await ReadTextFromFile();
        }       
        /// <summary>
        /// Save RichTextBox content to current file
        /// </summary>
        private void SaveFile()
        {
            File.WriteAllLines(_currentFile, MainTextField.Lines);
            _isFileSaved = true;
            _isFileCreated = true;
            SwitchFileSavedMark();
        }
        /// <summary>
        /// Save RichTextBox content to file
        /// </summary>
        /// <param name="FileName">Name of file to save to</param>
        private void SaveFile(string FileName)
        {
            File.WriteAllLines(FileName, MainTextField.Lines);
            _isFileCreated = true;
            _isFileSaved = true;
            SwitchFileSavedMark();
        }
        /// <summary>
        /// Save FastColoredTextBox content to current file
        /// </summary>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_isFileCreated == true)
            {
                SaveFile();
                return;
            }
            newToolStripMenuItem_Click(sender, e);
        }
        /// <summary>
        /// Save FastColoredTextBox content to new file
        /// </summary>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sv = saveFileDialog1;
            sv.FileName = _currentFile;
            if (sv.ShowDialog() != DialogResult.OK) return;

            SaveFile(sv.FileName);
            FileNameToFormText(sv.FileName);
        }
        /// <summary>
        /// Create new file and save FastColoredTextBox contents to it
        /// </summary>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sv = saveFileDialog1;
            sv.FileName = "untitled.txt";
            if (sv.ShowDialog() != DialogResult.OK) return;
            _currentFile = sv.FileName;
            FileNameToFormText(_currentFile);
            SaveFile();
        }
        #endregion
        #region Edit
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainTextField.Undo();
        }
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainTextField.Redo();
        }
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainTextField.Cut();
        }
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainTextField.Copy();
        }
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainTextField.Paste();
        }
        #endregion
        #region Theme
        /// <summary>
        /// Change theme of application to black
        /// </summary>
        public void blackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theme.syntaxColors.SyntaxPath = "SyntaxColors/BlackSyntax.syn";
            CurrentTheme = Theme.Black;
            new ThemeChanger(this).ChangeTheme();

            whiteToolStripMenuItem.Checked = false;
            blackToolStripMenuItem.Checked = true;
        }
        /// <summary>
        /// Change theme of application to white
        /// </summary>
        public void whiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theme.syntaxColors.SyntaxPath = "SyntaxColors/WhiteSyntax.syn";
            CurrentTheme = Theme.White;
            new ThemeChanger(this).ChangeTheme();

            whiteToolStripMenuItem.Checked = true;
            blackToolStripMenuItem.Checked = false;
        }
        #endregion


        /// <summary>
        ///  Deletes not default labels
        /// </summary>
        private void DeleteUnnecessaryLabels()
        {
            DeleteLineLabel();
            DeleteSymbolLabel();
            DeleteFileStatusLabel();
        }

        /// <summary>
        /// Switches file save mark ("*") in form text
        /// </summary>
        private void SwitchFileSavedMark()
        {
            switch (_isFileSaved)
            {
                case true:
                    {
                        MarkFileAsSaved();
                    } break;
                case false:
                    {
                        MarkFileAsUnsaved();
                    } break;
            }
        }
        /// <summary>
        /// Marks current opened file as unsaved
        /// </summary>
        private void MarkFileAsUnsaved()
        {
            this.Text += this.Text.Last() != '*' ? "*" : String.Empty;
            IsSavedLabel.Text = "File status: Unsaved";
        }
        /// <summary>
        /// Marks current opened file as saved
        /// </summary>
        private void MarkFileAsSaved()
        {
            FileNameToFormText(_currentFile);
            IsSavedLabel.Text = "File status: Saved";
        }

        /// <summary>
        /// Sets form text as App name and file opened name
        /// </summary>
        /// <param name="FileName">Name of file to set</param>
        private void FileNameToFormText(string FileName = "*") => this.Text = $"{_applicationName} | {Path.GetFileName(FileName)}";

        /// <summary>
        /// Get all labels from StatusStrip
        /// </summary>
        /// <param name="statusStrip">Control to get labels from</param>
        /// <returns>List of labels</returns>
        private IEnumerable<ToolStripStatusLabel> GetLabelsFromStatusStrip(StatusStrip statusStrip)
        {
            var labels = new List<ToolStripStatusLabel>();
            foreach (var item in statusStrip.Items)
            {
                if (item is ToolStripStatusLabel)
                    labels.Add((ToolStripStatusLabel)item);
            }
            return labels;
        }
        /// <summary>
        /// Get all status labels from form
        /// </summary>
        /// <returns>List of labels</returns>
        public IEnumerable<ToolStripStatusLabel> GetLabelsFromForm()
        {
            var labels = new List<ToolStripStatusLabel>();
            foreach (var control in this.Controls)
            {
                if (control is ToolStripStatusLabel)
                    labels.Add((ToolStripStatusLabel)control);
                if (control is StatusStrip)
                    labels.AddRange((List<ToolStripStatusLabel>)GetLabelsFromStatusStrip((StatusStrip)control));
            }
            return labels;
        }


        /// <summary>
        /// Show OptionsForm dialog
        /// </summary>
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new OptionsForm(this).ShowDialog();
        }

        /// <summary>
        /// Deletes status label from footer and unchecks context menu item
        /// </summary>
        /// <param name="label">label to delete</param>
        /// <param name="item">context menu item to uncheck</param>
        private void DeleteStatusLabel(ToolStripStatusLabel label, ToolStripMenuItem item)
        {
            item.Checked = false;
            MainFooter.Items.Remove(label);
        }
        private void DeleteLineCountLabel()
        {
            DeleteStatusLabel(LineCountLable, linesToolStripMenuItem);
        }
        private void DeleteSymbolLabel()
        {
            DeleteStatusLabel(SymbolCountLable, symbolToolStripMenuItem);
        }
        private void DeleteLineLabel()
        {
            DeleteStatusLabel(CurrentLineLabel, currentLineToolStripMenuItem);
        }
        private void DeleteFileStatusLabel()
        {
            DeleteStatusLabel(IsSavedLabel, fileStatusToolStripMenuItem);
        }
        private void DeleteZoomLabel()
        {
            DeleteStatusLabel(zoomPercentageLabel, zoomToolStripMenuItem);
        }

        /// <summary>
        /// Line count status switching
        /// </summary>
        private void linesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var label = LineCountLable;
            if (item.Checked)
            {
                DeleteLineCountLabel();
                return;
            }
            item.Checked = true;
            if(CurrentTheme == Theme.Black)
                label.ForeColor = Color.White;
            else
                label.ForeColor = Color.Black;
            MainFooter.Items.Add(label);
        }
        /// <summary>
        /// Current symbol status switching
        /// </summary>
        private void symbolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var label = SymbolCountLable;
            if (item.Checked)
            {
                DeleteSymbolLabel();
                return;
            }
            item.Checked = true;
            if (CurrentTheme == Theme.Black)
                label.ForeColor = Color.White;
            else
                label.ForeColor = Color.Black;
            MainFooter.Items.Add(SymbolCountLable);
        }
        /// <summary>
        /// Current line status switching
        /// </summary>
        private void currentLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var label = CurrentLineLabel;
            if (item.Checked)
            {
                DeleteLineLabel();
                return;
            }
            item.Checked = true;
            if (CurrentTheme == Theme.Black)
                label.ForeColor = Color.White;
            else
                label.ForeColor = Color.Black;
            MainFooter.Items.Add(CurrentLineLabel);
        }
        /// <summary>
        /// Current file status label switching
        /// </summary>
        private void fileStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var label = IsSavedLabel;
            if (item.Checked)
            {
                DeleteFileStatusLabel();
                return;
            }
            item.Checked = true;
            if (CurrentTheme == Theme.Black)
                label.ForeColor = Color.White;
            else
                label.ForeColor = Color.Black;
            MainFooter.Items.Add(label);
        }
        /// <summary>
        /// Zoom status label switching
        /// </summary>
        private void zoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var button = zoomPercentageLabel;
            if (item.Checked)
            {
                DeleteZoomLabel();
                return;
            }
            item.Checked = true;
            if (CurrentTheme == Theme.Black)
                button.ForeColor = Color.White;
            else
                button.ForeColor = Color.Black;
            MainFooter.Items.Add(button);
        }

        /// <summary>
        /// Set the selection details into status labels
        /// </summary>
        private void MainTextField_SelectionChanged(object sender, EventArgs e)
        {
            SymbolCountLable.Text = $"Current symbol: {MainTextField.Selection.Start.iChar}";
            CurrentLineLabel.Text = $"Current line: {MainTextField.Selection.Start.iLine + 1}";
        }

        /// <summary>
        /// Occurs when content of MainTextField changes somehow
        /// </summary>
        private void MainTextField_OnContentChanged()
        {
            LineCountLable.Text = $"Lines: {MainTextField.Lines.Count}";
            _isFileSaved = false;
            SwitchFileSavedMark();
        }
        private void MainTextField_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainTextField_OnContentChanged();
        }
        private void MainTextField_LineInserted(object sender, LineInsertedEventArgs e)
        {
            MainTextField_OnContentChanged();
        }
        private void MainTextField_LineRemoved(object sender, LineRemovedEventArgs e)
        {
            MainTextField_OnContentChanged();
        }

        private void MainTextField_ZoomChanged(object sender, EventArgs e)
        {
            var zoom = MainTextField.Zoom;
            zoomPercentageLabel.Text = $"Zoom: {zoom}%";
        }

        private void zoomPercentageLabel_Click(object sender, EventArgs e)
        {
            MainTextField.Zoom = DefaultZoom;
        }

        /// <summary>
        /// Shows message box on form closing if file is unsaved to confirm closing with or without save
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_currentFile != String.Empty)
            {
                if (!_isFileSaved)
                {
                    DialogRes AskToSave = new DialogRes(MessageBox.Show);
                    var result = AskToSave("Do you want to save your file?", "Save?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                    if (result == DialogResult.Yes)
                    {
                        SaveFile();
                        return;
                    }

                    /// If user wants to cancel the operation of closing
                    /// We use "FormClosingEvents" object and set its property "Cancel" to true
                    if(result == DialogResult.Cancel)
                        e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Switch state of window
        /// </summary>
        private void screenModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            
            switch (StateOfWindow)
            {
                case Enums.WindowState.Windowed:
                    {
                        FormBorderStyle = FormBorderStyle.None;
                        WindowState = FormWindowState.Maximized;

                        item.Text = $"Switch to {StateOfWindow}";
                        StateOfWindow = Enums.WindowState.Fullscreen;
                        
                    }
                    break;
                case Enums.WindowState.Fullscreen: 
                    {
                        FormBorderStyle = FormBorderStyle.Sizable;
                        WindowState = FormWindowState.Normal;

                        item.Text = $"Switch to {StateOfWindow}";

                        StateOfWindow = Enums.WindowState.Windowed;
                    }
                    break;
            }
        }
        #endregion
    }
}
