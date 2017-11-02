using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomSearchGrid
{
    public partial class SearchGrid : Form
    {
        public int GridID { get; set; }
        public string ConString { get; set; }
        public List<Control> Filters { get; set; }
        private Button btnSearch = new Button()
        {
            Text = "Search"
        };

        public SearchGrid(int id)
        {
            InitializeComponent();
            this.GridID = id;
            this.Filters = Engine.ShowGrid(this.GridID, this.dgv, new DataTable());
        }
        public SearchGrid(int id, string conString)
        {
            InitializeComponent();
            this.GridID = id;
            this.ConString = conString;
            this.Filters = Engine.ShowGrid(this.GridID, this.dgv, this.ConString);
        }

        private void SearchGrid_Load(object sender, EventArgs e)
        {
            LoadFilters();
            btnSearch.Click += new System.EventHandler(btnSearch_Click);
        }

        private void LoadFilters()
        {
            if (this.Filters != null && this.Filters.Count > 0)
            {
                this.Filters.ForEach(ctrl => { flpFilters.Controls.Add(ctrl); });
                Control dummy = GetDummyLabel();
                flpFilters.Controls.Add(dummy);
                flpFilters.SetFlowBreak(dummy, true);
                flpFilters.Controls.Add(btnSearch);
            }
            else
            {
                splitContainer1.Panel1.Height = 1;
                splitContainer1.SplitterDistance = 1;
            }
        }


        private void btnSearch_Click(object sender, EventArgs e)
        {

        }

        private Label GetDummyLabel()
        {
            return new Label()
        {
            Height = 0,
            Width = 0,
            Margin = new Padding(0, 0, 0, 0)
        };
        }
    }


}
