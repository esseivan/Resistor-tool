using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EsseivaN.Apps.ResistorTool
{
	public partial class frmPreview : Form
	{
		public frmPreview()
		{
			InitializeComponent();
		}

		public frmPreview(string msg)
		{
			InitializeComponent();
			this.richTextBox1.Text = msg;
		}
	}
}
