using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace CustomSearchGrid
{
    public class Engine
    {
        public static List<Control> ShowGrid(int id, DataGridView dgv, string conString)
        {
            GridMap map = GenerateMapping(id);
            ArrangeColumnsInGrid(map, dgv);
            dgv.DataSource = GetDataFromDB(map, conString);
            return GenerateFilters(map);
        }

        public static List<Control> ShowGrid(int id, DataGridView dgv, DataTable dt)
        {
            GridMap map = GenerateMapping(id);
            ArrangeColumnsInGrid(map, dgv);
            dgv.DataSource = dt;
            return GenerateFilters(map);
        }

        private static GridMap GenerateMapping(int id)
        {
            GridMap map = null;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(MapCollection));

                using (StringReader strReader = new StringReader(Properties.Resources.MappingData))
                using (XmlReader reader = XmlReader.Create(strReader))
                {
                    MapCollection mapcollection = (MapCollection)ser.Deserialize(reader);
                    if (mapcollection != null && mapcollection.GridMap != null && mapcollection.GridMap.Count > 0)
                    {
                        map = mapcollection.GridMap.FirstOrDefault(x => x.Id == id.ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Please correct the mapping xml.", "Exception Occured.");
            }
            return map;
        }

        private static void ArrangeColumnsInGrid(GridMap map, DataGridView dgv)
        {
            if (map != null)
            {
                try
                {
                    if (map.Outputs != null && map.Outputs.Columns != null && map.Outputs.Columns.Count > 0)
                    {
                        map.Outputs.Columns.ForEach(col =>
                        {
                            dgv.Columns.Add(new DataGridViewTextBoxColumn()
                            {
                                DataPropertyName = col.ColumnName,
                                Name = col.Name,
                                HeaderText = col.Name,
                                ReadOnly = true,
                                Visible = "TRUE".Equals(col.Visible.ToUpper()),
                                Width = Convert.ToInt32(col.Width),
                                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = GetAlignment(col.Align), Format = col.Format }
                            });
                        });
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Please correct the column value  [id = {0}].", map.Id), "Exception Occured.");
                }
            }

        }

        private static DataTable GetDataFromDB(GridMap map, string conString)
        {
            DataTable dt = new DataTable();
            if (map != null && !string.IsNullOrEmpty(map.DBInstance))
            {
                try
                {
                    using (SqlDataAdapter sqlAda = new SqlDataAdapter(map.DBInstance, new SqlConnection(conString)))
                    {
                        if (map.Inputs != null && map.Inputs.Param != null && map.Inputs.Param.Count > 0)
                        {
                            map.Inputs.Param.ForEach(prm =>
                            {
                                sqlAda.SelectCommand.Parameters.AddWithValue(prm.Name, prm.DefaultValue);
                            });
                        }
                        sqlAda.Fill(dt);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Excetion occured for ({0}, [{1}])", map.Id, ex.Message), "DB Execution Error");
                }
            }
            return dt;
        }

        private static DataGridViewContentAlignment GetAlignment(string align)
        {
            switch (align.ToUpper())
            {
                case "RIGHT": return DataGridViewContentAlignment.BottomRight;
                case "MIDDLE": return DataGridViewContentAlignment.BottomCenter;
                default: return DataGridViewContentAlignment.BottomLeft;
            }
        }

        private static List<Control> GenerateFilters(GridMap map)
        {
            List<Control> filterCtrls = new List<Control>();
            if (map != null && map.Inputs != null && map.Inputs.Param != null && map.Inputs.Param.Count > 0)
            {
                map.Inputs.Param.ForEach(prm =>
                {
                    if ("TRUE".Equals(prm.Filter.ToUpper()))
                    {
                        filterCtrls.Add(new Label() { Text = prm.FilterName, Margin = new Padding(5) });
                        if ("TEXT".Equals(prm.FilterType.ToUpper()))
                        {
                            filterCtrls.Add(new TextBox() { Name = prm.FilterName, Text = string.Empty, Width = 150 });
                        }
                        else
                        {
                            LookUp lookup = GetLookUp(prm.FilterValue);
                            if (lookup != null)
                            {
                                if ("DROP".Equals(lookup.Type.ToUpper()))
                                {
                                    
                                    if (lookup.LookUpItem != null && lookup.LookUpItem.Count > 0)
                                    {
                                        ComboBox cmb = new ComboBox() { Name = prm.FilterName, Margin = new Padding(5) };
                                        cmb.DisplayMember = "Text";
                                        cmb.ValueMember = "Value";
                                        cmb.DataSource = lookup.LookUpItem;
                                        filterCtrls.Add(cmb);
                                    }
                                }
                                else if ("RADIO".Equals(lookup.Type.ToUpper()))
                                {

                                    if (lookup.LookUpItem != null && lookup.LookUpItem.Count > 0)
                                    {
                                        GroupBox gb = new GroupBox() { Name = prm.FilterName, Margin = new Padding(5), Height=60 };
                                        int posX=-110;
                                        lookup.LookUpItem.ForEach(itm =>
                                        {
                                            gb.Controls.Add(new RadioButton() { Name = string.Format("{0}_{1}", prm.FilterName, itm.Value), Text = itm.Text, Location = new Point(posX+=120, 10) });
                                        });
                                        gb.Width = posX + 120;
                                        filterCtrls.Add(gb);
                                    }

                                }
                                else
                                {
                                    filterCtrls.Add(new TextBox() { Text = string.Empty, Width = 150 });
                                }

                            }
                        }
                    }
                });
            }

            return filterCtrls;
        }

        private static LookUp GetLookUp(string lookupID)
        {
            LookUp lookup = null;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(MapCollection));

                using (StringReader strReader = new StringReader(Properties.Resources.MappingData))
                using (XmlReader reader = XmlReader.Create(strReader))
                {
                    MapCollection mapcollection = (MapCollection)ser.Deserialize(reader);
                    if (mapcollection != null && mapcollection.GridMap != null && mapcollection.GridMap.Count > 0)
                    {
                        lookup = mapcollection.LookUps.LookUp.FirstOrDefault(x => x.Id.ToUpper() == lookupID.ToUpper());
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Please correct the mapping xml.", "Exception Occured.");
            }

            return lookup;
        }
    }

    #region Mapping Deserialize

    [XmlRoot(ElementName = "Param")]
    public class Param
    {
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "DefaultValue")]
        public string DefaultValue { get; set; }
        [XmlAttribute(AttributeName = "Filter")]
        public string Filter { get; set; }
        [XmlAttribute(AttributeName = "FilterType")]
        public string FilterType { get; set; }
        [XmlAttribute(AttributeName = "FilterName")]
        public string FilterName { get; set; }
        [XmlAttribute(AttributeName = "FilterValue")]
        public string FilterValue { get; set; }
    }

    [XmlRoot(ElementName = "Inputs")]
    public class Inputs
    {
        [XmlElement(ElementName = "Param")]
        public List<Param> Param { get; set; }
    }

    [XmlRoot(ElementName = "Columns")]
    public class Columns
    {
        [XmlAttribute(AttributeName = "ColumnName")]
        public string ColumnName { get; set; }
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "Width")]
        public string Width { get; set; }
        [XmlAttribute(AttributeName = "Display")]
        public string Display { get; set; }
        [XmlAttribute(AttributeName = "Visible")]
        public string Visible { get; set; }
        [XmlAttribute(AttributeName = "Align")]
        public string Align { get; set; }
        [XmlAttribute(AttributeName = "Format")]
        public string Format { get; set; }
        [XmlAttribute(AttributeName = "Evaluate")]
        public string Evaluate { get; set; }
    }

    [XmlRoot(ElementName = "Outputs")]
    public class Outputs
    {
        [XmlElement(ElementName = "Columns")]
        public List<Columns> Columns { get; set; }
        [XmlAttribute(AttributeName = "MaximumRecords")]
        public string MaximumRecords { get; set; }
    }

    [XmlRoot(ElementName = "GridMap")]
    public class GridMap
    {
        [XmlElement(ElementName = "Inputs")]
        public Inputs Inputs { get; set; }
        [XmlElement(ElementName = "Outputs")]
        public Outputs Outputs { get; set; }
        [XmlAttribute(AttributeName = "Id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "DBInstance")]
        public string DBInstance { get; set; }
    }

    [XmlRoot(ElementName = "LookUpItem")]
    public class LookUpItem
    {
        [XmlAttribute(AttributeName = "Text")]
        public string Text { get; set; }
        [XmlAttribute(AttributeName = "Value")]
        public string Value { get; set; }
    }

    [XmlRoot(ElementName = "LookUp")]
    public class LookUp
    {
        [XmlElement(ElementName = "LookUpItem")]
        public List<LookUpItem> LookUpItem { get; set; }
        [XmlAttribute(AttributeName = "Id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "Type")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "LookUps")]
    public class LookUps
    {
        [XmlElement(ElementName = "LookUp")]
        public List<LookUp> LookUp { get; set; }
    }

    [XmlRoot(ElementName = "MapCollection")]
    public class MapCollection
    {
        [XmlElement(ElementName = "GridMap")]
        public List<GridMap> GridMap { get; set; }

        [XmlElement(ElementName = "LookUps")]
        public LookUps LookUps { get; set; }
    }

    #endregion
}
