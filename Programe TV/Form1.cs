using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Programe_TV
{
    public partial class MainForm : Form
    {
        private DataTable dataTable;
        private XmlDocument document;
        private int draggedRowIndex = -1;
        private DataView filteredView; // Added DataView for filtering
        public MainForm()
        {
            InitializeComponent();



            dataTable = new DataTable();
            dataTable.Columns.Add("Nume Program");
            dataTable.Columns.Add("Pozitie", typeof(int));
            //dataGridView.DataSource = dataTable;

            filteredView = new DataView(dataTable); // Initialize the DataView
            filteredView.Sort = "Pozitie ASC"; // Set initial sort by "Num" column

            dataGridView.DataSource = filteredView;
            dataTable = filteredView.ToTable();

            // Enable drag and drop in the DataGridView
            dataGridView.AllowDrop = true;


            // Hide the row selector column
            dataGridView.RowHeadersVisible = false;

            this.Resize += MainForm_Resize;

            this.DragEnter += MainForm_DragEnter;
            this.DragDrop += MainForm_DragDrop;
            dataGridView.DragEnter += MainForm_DragEnter;
            dataGridView.DragDrop += MainForm_DragDrop;
            dataGridView.CellBeginEdit += dataGridView_CellBeginEdit;

        }

        private void dataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // Check if the first column is being edited
            if (e.ColumnIndex == 0)
            {
                // Cancel the edit operation for the first column
                e.Cancel = true;
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                string filePath = files[0];

                // Load the XML file and populate the DataGridView
                loadXml(filePath);
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Adjust the column widths based on percentage
            int serviceNameColumnWidth = (int)(dataGridView.Width * 0.76);
            int numColumnWidth = (int)(dataGridView.Width * 0.2);

            dataGridView.Columns["Nume Program"].Width = serviceNameColumnWidth;
            dataGridView.Columns["Pozitie"].Width = numColumnWidth;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int serviceNameColumnWidth = (int)(dataGridView.Width * 0.76);
            int numColumnWidth = (int)(dataGridView.Width * 0.2);

            dataGridView.Columns["Nume Program"].Width = serviceNameColumnWidth;
            dataGridView.Columns["Pozitie"].Width = numColumnWidth;


        }


        private void saveButton_Click_1(object sender, EventArgs e)
        {
            if (document != null)
            {


                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    // Get the service name and new num value from the DataGridView
                    DataRow dataRow = dataTable.Rows[i];
                    string serviceName = dataRow["Nume Program"].ToString();
                    int newNumValue = Convert.ToInt32(dataRow["Pozitie"]);

                    // Search for the XML element based on the service name attribute value
                    XmlNodeList serviceNodes = document.SelectNodes($"//service[@name='{serviceName}']");
                    if (serviceNodes.Count > 0)
                    {
                        XmlNode serviceNode = serviceNodes[0];

                        // Update the num attribute value
                        XmlAttribute numAttribute = serviceNode.Attributes["num"];
                        if (numAttribute != null)
                        {
                            numAttribute.Value = newNumValue.ToString();
                        }
                        else
                        {
                            // If the num attribute doesn't exist, create it
                            numAttribute = document.CreateAttribute("num");
                            numAttribute.Value = newNumValue.ToString();
                            serviceNode.Attributes.Append(numAttribute);
                        }
                    }
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "XML Files|*.xml";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    document.Save(saveFileDialog.FileName);
                    MessageBox.Show("Modified XML saved successfully.");
                }
            }
        }

        private void loadButton_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files|*.xml";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;

                loadXml(fileName);
            }
        }

        private void loadXml(string fileName)
        {
            document = new XmlDocument();

            document.Load(fileName);

            XmlNodeList serviceNodes = document.SelectNodes("//service[./@typ='1']");

            dataTable.Rows.Clear();

            foreach (XmlNode serviceNode in serviceNodes)
            {
                XmlElement serviceElement = (XmlElement)serviceNode;
                string serviceName = serviceElement.GetAttribute("name");
                int num = int.Parse(serviceElement.GetAttribute("num"));

                dataTable.Rows.Add(serviceName, num);
            }


            filteredView = new DataView(dataTable); // Initialize the DataView
            filteredView.Sort = "Pozitie ASC";

            dataGridView.DataSource = filteredView;


            dataTable = filteredView.ToTable();

            if (dataGridView.Rows.Count > 0)
            {
                dataGridView.Rows[0].Selected = true;
            }
        }

        private void DataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                draggedRowIndex = dataGridView.HitTest(e.X, e.Y).RowIndex;
                //label1.Text = draggedRowIndex.ToString();
                if (draggedRowIndex >= 0)
                {
                    dataGridView.DoDragDrop(dataGridView.Rows[draggedRowIndex], DragDropEffects.Move);
                }
            }
        }

        private void DataGridView_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left && draggedRowIndex >= 0)
            {
                dataGridView.DoDragDrop(dataGridView.Rows[draggedRowIndex], DragDropEffects.Move);
            }
        }

        private void DataGridView_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void DataGridView_DragDrop(object sender, DragEventArgs e)
        {
            //label1.Text = draggedRowIndex.ToString();
            if (draggedRowIndex >= 0)
            {
                Point clientPoint = dataGridView.PointToClient(new Point(e.X, e.Y));
                int targetRowIndex = dataGridView.HitTest(clientPoint.X, clientPoint.Y).RowIndex;
                //label2.Text = targetRowIndex.ToString();
                if (targetRowIndex >= 0 && targetRowIndex != draggedRowIndex)
                {
                    DataRow draggedRow = dataTable.Rows[draggedRowIndex];
                    DataRow newRow = dataTable.NewRow();
                    newRow["Nume Program"] = draggedRow["Nume Program"];
                    newRow["Pozitie"] = draggedRow["Pozitie"];

                    //          DataRow draggedRow = ((DataRowView)e.Data.GetData(typeof(DataRowView))).Row;


                    dataTable.Rows.RemoveAt(draggedRowIndex);
                    dataTable.Rows.InsertAt(newRow, targetRowIndex);

                    
                    // Update the "Num" column in the DataTable
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        dataTable.Rows[i]["Pozitie"] = (i + 1).ToString();
                    }

                    filteredView = new DataView(dataTable); // Initialize the DataView
                    dataGridView.DataSource = filteredView;
                    dataGridView.Refresh();

                }
            }
            dataGridView.ClearSelection();

            draggedRowIndex = -1;
        }


        private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0) // Check if the "Num" column is edited
            {
                string newValue = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                int currentRowPosition = e.RowIndex + 1;

                // Check if the new value is numeric
                if (int.TryParse(newValue, out int newRowPosition))
                {

                    if (newRowPosition >= 1 && newRowPosition <= dataGridView.RowCount)
                    {
                        if (newRowPosition != currentRowPosition)
                        {
                            DataRow draggedRow = dataTable.Rows[e.RowIndex];

                            DataRow newRow = dataTable.NewRow();
                            newRow["Nume Program"] = draggedRow["Nume Program"];
                            newRow["Pozitie"] = draggedRow["Pozitie"];

                            dataTable.Rows.Remove(draggedRow);
                            dataTable.Rows.InsertAt(newRow, newRowPosition - 1);


                            // Update the "Num" column in the DataTable
                            for (int i = 0; i < dataTable.Rows.Count; i++)
                            {
                                dataTable.Rows[i]["Pozitie"] = (i + 1).ToString();
                            }

                            filteredView = new DataView(dataTable); // Initialize the DataView
                            dataGridView.DataSource = filteredView;
                            dataGridView.Refresh();
                        }
                    }
                    else
                    {
                        // Reset the value if it's out of range
                        dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = currentRowPosition;
                    }
                }
                else
                {
                    // Reset the value if it's not numeric
                    dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = currentRowPosition;
                }
            }
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void sortButton_Click(object sender, EventArgs e)
        {
            filteredView = new DataView(dataTable); // Initialize the DataView
            filteredView.Sort = "Nume Program ASC";

            dataTable = filteredView.ToTable();

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                dataTable.Rows[i]["Pozitie"] = (i + 1).ToString();
            }

            dataGridView.DataSource = new DataView(dataTable);
            dataGridView.Refresh();

            /*
                for (int i = 0; i < dataGridView.Rows.Count; i++)
                {
                    if (dataGridView.Rows[i].Cells[0].Value == null)
                        continue;
                    string numeProgram = dataGridView.Rows[i].Cells[0].Value.ToString();

                    for (int j=0; j< dataTable.Rows.Count; j++)
                    {
                        if (dataTable.Rows[j]["Nume Program"] == numeProgram)
                        {
                            dataTable.Rows[j]["Pozitie"] = i+1;
                            break;
                        }
                    }
                }
            */



            /*
            // Sort the rows in the DataGridView by the "Service Name" column alphabetically
            dataGridView.Sort(dataGridView.Columns["Nume Program"], ListSortDirection.Ascending);

            // Update the "Num" values starting from 1 for the sorted rows
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                dataGridView.Rows[i].Cells["Pozitie"].Value = (i + 1).ToString();
            }
            dataGridView.Refresh();
*/
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            dataGridView.ClearSelection();
            string text = textBox1.Text;
            int rowIndex = -1;

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                string programName = dataTable.Rows[i]["Nume Program"].ToString();
                if (programName.ToLower().StartsWith(text.ToLower()))
                {
                    rowIndex = i;
                    break;
                }
            }
            if(rowIndex == -1)
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    string programName = dataTable.Rows[i]["Nume Program"].ToString();
                    if (programName.ToLower().Contains(text.ToLower()))
                    {
                        rowIndex = i;
                        break;
                    }
                }
            }
            if (rowIndex != -1 && dataGridView.Rows.Count > 0)
            {
                dataGridView.Rows[rowIndex].Selected = true;
                dataGridView.FirstDisplayedScrollingRowIndex = rowIndex;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            
        }
    }
}
