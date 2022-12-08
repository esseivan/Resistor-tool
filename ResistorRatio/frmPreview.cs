using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ESN.ResistorCalculator.Forms
{
	public partial class frmPreview : Form
	{
        public string Data
        {
            get
            {
                return richTextBox1.Text;
            }
            set
            {
                this.richTextBox1.Text = value;
            }
        }

        public void AddData(string NewData)
        {
            this.richTextBox1.Text += NewData;
        }

		public frmPreview()
		{
			InitializeComponent();
		}

		public frmPreview(string msg)
		{
			InitializeComponent();
            Data = msg;
		}
	}
}
