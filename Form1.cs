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

namespace gReptar
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

            String directory = textBox1.Text;
            String searchString = textBox2.Text;

            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            Pipeline pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript($"$directory = '{directory}'");
            pipeline.Commands.AddScript($"$searchString = '{searchString}'");
            pipeline.Commands.AddScript(@"Get-ChildItem -Path $directory -Recurse -File | Where-Object { $_.Extension -notin '.exe', '.bin', '.dll' } | ForEach-Object { Select-String -Path $_.FullName -Pattern $searchString -SimpleMatch | ForEach-Object { $_.Line }}");
            var results = pipeline.Invoke();
            runspace.Close();
            
            listBox1.Items.Clear();
            foreach (var result in results)
            {
                if (result != null)
                    listBox1.Items.Add(result.ToString());
            }

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
