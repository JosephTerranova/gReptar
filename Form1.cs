using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace gReptar
{
    public partial class Form1 : Form
    {
        private ContextMenuStrip listBoxContextMenu;
        private ToolStripMenuItem copyMenuItem;

        public Form1()
        {
            InitializeComponent();            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button3.Enabled = false;            
            listBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBox1.SelectionMode = SelectionMode.MultiExtended;

            listBoxContextMenu = new ContextMenuStrip();
            copyMenuItem = new ToolStripMenuItem("Copy");
            copyMenuItem.Click += copyToolStripMenuItem_Click;
            listBoxContextMenu.Items.Add(copyMenuItem);
            listBox1.ContextMenuStrip = listBoxContextMenu;
            listBox1.DoubleClick += listBox1_DoubleClick;


        }

        private void CheckTextboxes()
        {
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                button3.Enabled = true;
            }
            else
            {
                button3.Enabled = false;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }    

        private async void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;

            await Task.Run(() =>
            {

                String directory = textBox1.Text;
                String searchString = textBox2.Text;

                Runspace runspace = RunspaceFactory.CreateRunspace();
                runspace.Open();
                Pipeline pipeline = runspace.CreatePipeline();
                pipeline.Commands.AddScript($"$directory = '{directory}'");
                pipeline.Commands.AddScript($"$searchString = '{searchString}'");
                pipeline.Commands.AddScript(@"Get-ChildItem -Path $directory -Recurse -File | Where-Object { $_.Extension -notin '.exe', '.bin', '.dll' } | ForEach-Object { Select-String -Path $_.FullName -Pattern $searchString -SimpleMatch}");
                var results = pipeline.Invoke();
                runspace.Close();

                listBox1.Invoke(new MethodInvoker(delegate
                {
                    listBox1.Items.Clear();
                    foreach (var result in results)
                    {
                        if (result != null)
                            listBox1.Items.Add(result.ToString());
                    }
                    listBox1.HorizontalScrollbar = true;

                    int maxWidth = 0;
                    using (Graphics g = listBox1.CreateGraphics())
                    {
                        foreach (var item in listBox1.Items)
                        {
                            int itemWidth = (int)g.MeasureString(item.ToString(), listBox1.Font).Width;
                            if (itemWidth > maxWidth)
                                maxWidth = itemWidth;
                        }
                    }
                    listBox1.HorizontalExtent = maxWidth;

                    button3.Enabled = true;
                }));
            });
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            CheckTextboxes();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select a folder";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dialog.SelectedPath;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            CheckTextboxes();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.C))
            {
                if (listBox1.Focused && listBox1.SelectedItems.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in listBox1.SelectedItems)
                    {
                        sb.AppendLine(item.ToString());
                    }
                    Clipboard.SetText(sb.ToString());
                }
                return true; // key handled
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in listBox1.SelectedItems)
                {
                    sb.AppendLine(item.ToString());
                }
                Clipboard.SetText(sb.ToString());
            }
        }


        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null) return;

            string selectedLine = listBox1.SelectedItem.ToString();

            // Match full Windows path + line number + line content
            // Example: C:\folder\file.txt:12:some line text
            var match = Regex.Match(selectedLine, @"^(.+?):(\d+):");

            if (!match.Success || match.Groups.Count < 3) return;

            string filePath = match.Groups[1].Value;
            if (!int.TryParse(match.Groups[2].Value, out int lineNumber)) return;

            try
            {
                // Try opening in Notepad++ with line number
                string notepadPlusPlusPath = @"C:\Program Files\Notepad++\notepad++.exe";
                if (System.IO.File.Exists(notepadPlusPlusPath))
                {
                    System.Diagnostics.Process.Start(notepadPlusPlusPath, $"-n{lineNumber} \"{filePath}\"");
                }
                else
                {
                    // Fallback to default app
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void tipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("If Notepad++ is installed, double clicking on a line item will open the file AND jump to the correct line!");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/JosephTerranova/gReptar",
                UseShellExecute = true
            });
        }
    }
}
