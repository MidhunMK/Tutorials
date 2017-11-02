using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SearchGrid
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //CustomSearchGrid.SearchGrid showGrid = new CustomSearchGrid.SearchGrid(1000);
            CustomSearchGrid.SearchGrid showGrid = new CustomSearchGrid.SearchGrid(1000, @"Data Source=.\SQLEXPRESS;Initial Catalog=InventoryDB;Integrated Security=True");
            showGrid.ShowDialog(this);
        }
    }
}
