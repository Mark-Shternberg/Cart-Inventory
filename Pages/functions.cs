using System.Data;
using System.Diagnostics.Metrics;
using System.Drawing;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Cart_Inventory.Pages
{
    public class Functions
    {
        public static string DataTableToHTML(DataTable dt) //КОВЕРТАЦИЯ ТАбЛИЦЫ В HTML
        {
            string html = "<table class=\"table\">";
            
            // HEADERS
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                if (i==2)
                {
                    html += "<td style=\"min-width: 100px;border-top-style: solid;border-width: 4px;border-bottom-width: 1px; border-color: dimgray; border-bottom-color: #dee2e6;\">" + dt.Columns[i].ColumnName + "</td>";
                }
                else html += "<td style=\"min-width: 100px;\">" + dt.Columns[i].ColumnName + "</td>";
            html += "</tr>";
            
            // ROWS
            string cell;
            string[] tmp;
            int count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    if (j > 1)
                    {
                        cell = dt.Rows[i][j].ToString();
                        tmp = cell.Split(" (");
                        count = Convert.ToInt32(tmp[0]);
                        if (j == 2) // FIRST INVENTORY COLUMN
                        {
                            if (count == 0) html += "<td style=\"min-width: 100px;border-right-width: 4px;border-left-width: 4px; " +
                                    "border-color: dimgray; background-color: salmon;border-bottom-color: #dee2e6;border-top-color: #dee2e6;\">" + dt.Rows[i][j].ToString() + "</td>";
                            else if (count < 3) html += "<td style=\"min-width: 100px;border-right-width: 4px;border-left-width: 4px; " +
                                    "border-color: dimgray; background-color: khaki;border-bottom-color: #dee2e6;border-top-color: #dee2e6;\">" + dt.Rows[i][j].ToString() + "</td>";
                            else html += "<td style=\"min-width: 100px;border-right-width: 4px;border-left-width: 4px; " +
                                    "border-color: dimgray;border-bottom-color: #dee2e6;border-top-color: #dee2e6;background-color: darkseagreen;\">" + dt.Rows[i][j].ToString() + "</td>";
                        }
                        else
                        {
                            if (count == 0) html += "<td style=\"min-width: 100px; background-color: salmon;\">" + dt.Rows[i][j].ToString() + "</td>";
                            else if (count < 3) html += "<td style=\"min-width: 100px; background-color: khaki;\">" + dt.Rows[i][j].ToString() + "</td>";
                            else html += "<td style=\"min-width: 100px;background-color: darkseagreen;\">" + dt.Rows[i][j].ToString() + "</td>";
                        }
                    }
                    else html += "<td style=\"min-width: 100px;\">" + dt.Rows[i][j].ToString() + "</td>";
                }

                html += "</tr>";
            }
            html += "</table>";
            return html;
        }
    }
}


