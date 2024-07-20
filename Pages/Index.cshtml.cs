using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using System.Data;
using System.Drawing;
using Newtonsoft.Json;
using System.Reflection.PortableExecutable;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Cart_Inventory.Pages.IndexModel;

namespace Cart_Inventory.Pages
{
    public class IndexModel : PageModel
    {
        //private readonly ILogger<IndexModel> _logger;

        //public IndexModel(ILogger<IndexModel> logger)
        //{
        //    _logger = logger;
        //}

        private readonly IConfiguration Configuration;

        public IndexModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string sql_connection()
        {

            var server = Configuration["MySQLConnection:server"];
            var user = Configuration["MySQLConnection:user"];
            var password = Configuration["MySQLConnection:password"];
            var database = Configuration["MySQLConnection:database"];

            return "Server=" + server + ";User ID=" + user + ";Password=" + password + ";Database=" + database;
        }

        public DataTable main_table { get; set; } = new DataTable();

        private void CheckDTExist() //ПРОВЕРЯЕТ СУЩЕСТВУЮТ ЛИ ТАБЛИЦЫ В БАЗЕ ДАННЫХ И ПРИ ОТСУТСТВИИ СОЗДАЁТ
        {
            using var connection = new MySqlConnection(sql_connection());
            connection.Open();

            using var check_table1 = new MySqlCommand("SHOW TABLES LIKE 'cartridges'", connection);
            using var check_table2 = new MySqlCommand("SHOW TABLES LIKE 'invent'", connection);
            using var check_table3 = new MySqlCommand("SHOW TABLES LIKE 'printers'", connection);

            using var create_cartridges = new MySqlCommand("CREATE TABLE `cartridges` (" +
                "`id` INT NOT NULL AUTO_INCREMENT," +
                "`model` LONGTEXT NOT NULL," +
                "`barcode` LONGTEXT NOT NULL," +
                "PRIMARY KEY (`id`)," +
                "`yellow_zone` INT NOT NULL DEFAULT 2" +
                "UNIQUE INDEX `id_UNIQUE` (`id` ASC) VISIBLE);", connection);
            using var create_invent = new MySqlCommand("CREATE TABLE `invent` (" +
                "`id` INT NOT NULL AUTO_INCREMENT," +
                "`date` MEDIUMTEXT NOT NULL," +
                "`invent` LONGTEXT NOT NULL," +
                "`invent_table` INT NOT NULL," +
                "PRIMARY KEY (`id`)," +
                "UNIQUE INDEX `id_UNIQUE` (`id` ASC) VISIBLE);", connection);
            using var create_printers = new MySqlCommand("CREATE TABLE `printers` (" +
                "`id` INT NOT NULL AUTO_INCREMENT," +
                "`name` MEDIUMTEXT NOT NULL," +
                "`cartridges` LONGTEXT NOT NULL," +
                "PRIMARY KEY (`id`)," +
                "UNIQUE INDEX `id_UNIQUE` (`id` ASC) VISIBLE);", connection);

            using var reader_main1 = check_table1.ExecuteReader();
            if (!reader_main1.HasRows)
            {
                reader_main1.Close();
                create_cartridges.Prepare();
                create_cartridges.ExecuteNonQuery();
            }
            else reader_main1.Close();

            using var reader_main2 = check_table2.ExecuteReader();
            if (!reader_main2.HasRows)
            {
                reader_main2.Close();
                create_invent.Prepare();
                create_invent.ExecuteNonQuery();
            }
            else reader_main2.Close();

            using var reader_main3 = check_table3.ExecuteReader();
            if (!reader_main3.HasRows)
            {
                reader_main3.Close();
                create_printers.Prepare();
                create_printers.ExecuteNonQuery();
            }
            else reader_main3.Close();
        }

        public void OnGet()
        {
            CheckDTExist();
            //DataTable main_table = new DataTable();

            using var connection = new MySqlConnection(sql_connection());
            connection.Open();

            using var command = new MySqlCommand("SELECT * FROM invent WHERE invent_table=0", connection);
            using var command_printer_names = new MySqlCommand("SELECT name FROM printers", connection);
            using var command_cartridges = new MySqlCommand("SELECT model FROM cartridges", connection);

            using var reader_main = command.ExecuteReader(); //ПОЛУЧЕНИЕ ТАБЛИЦЫ ИНВЕНТАРИЗАЦИЙ
            if (reader_main.HasRows)
            {
                DataTable dt = new DataTable();
                dt.Load(reader_main);
                int numberOfResults = dt.Rows.Count;

                main_table.Columns.Clear();

                main_table.Columns.Add("Cartridge");
                main_table.Columns.Add("Printer");

                for (int i = numberOfResults; i > numberOfResults - 8 && i > 0 ; i--)   // построчно считываем данные
                {

                    string date = dt.Rows[i - 1][1].ToString();

                    string columnName = date;
                    int columnCount = 1;

                    while (main_table.Columns.Contains(columnName))
                    {
                        columnName = $"{columnName} - {columnCount}";
                        columnCount++;
                    }
                    main_table.Columns.Add(columnName);

                    string[] tmp = dt.Rows[i - 1][2].ToString().Split(',');

                    int column_id = main_table.Columns[columnName].Ordinal;
                    foreach (string item in tmp)
                    {
                        string[] tmp2 = item.Split("/");
                        if (check_cart_exist(tmp2[0]) == 1)
                        {

                            int row_id = find_row(get_cartridge(tmp2[0]), tmp2[0]);
                            //main_table.Rows[row_id][column_id] = invent_difference(column_id, row_id, tmp2[1], 0);
                            main_table.Rows[row_id][column_id] = tmp2[1];
                            main_table.Rows[row_id][column_id - 1] = invent_difference(column_id, row_id, tmp2[1]);
                        }
                    }
                }
            }

            string get_cartridge(string id) //ПОЛУЧЕНИЕ НАИМЕНОВАНИЯ КАРТРИДЖА ПО ID
            {
                try
                {
                    string sqlExpression = "SELECT model FROM cartridges WHERE id=?id";

                    using (var connection = new MySqlConnection(sql_connection()))
                    {
                        connection.Open();

                        using var command = new MySqlCommand(sqlExpression, connection);
                        command.Parameters.AddWithValue("?id", id);

                        using var reader = command.ExecuteReader();
                        {
                            if (reader.HasRows) // если есть данные
                            {
                                while (reader.Read())   // построчно считываем данные
                                {
                                    return reader.GetString(0);
                                }
                                return "No data";
                            }
                            else return "No data";
                        }
                    }
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }

            int find_row(string name, string id) //ПОИСК СТРОКИ
            {
                int rowIndex = -1;

                for (int i = 0; i < main_table.Rows.Count; i++)
                {
                    if (main_table.Rows[i].Field<string>(0) != null && main_table.Rows[i].Field<string>(0) == name)
                    {
                        main_table.Rows[i][1] = get_printer_by_cart(id);
                        return i;
                    }
                }

                if (rowIndex == -1)
                {
                    main_table.Rows.Add(name);
                    int row_id = main_table.Rows.Count - 1;
                    main_table.Rows[row_id][1] = get_printer_by_cart(id);
                    return row_id;
                }
                return -1;
            }

            string invent_difference(int column, int row, string count) //РАЗНИЦА МЕЖДУ ИНВЕНТАРИЗАЦИЯМИ
            {
                string str_out = main_table.Rows[row][column - 1].ToString();

                if (column > 2)
                {
                    if (main_table.Rows[row][column - 1] == null || main_table.Rows[row][column - 1].ToString() == "")
                    {
                        main_table.Rows[row][column - 1] = "0";
                    }
                    int old_count = Convert.ToInt32(count);

                    int difference = Convert.ToInt32(main_table.Rows[row][column - 1]) - old_count;
                    string str_difference;
                    if (difference > 0) str_difference = "+" + difference;
                    else if (difference == 0) return str_out;
                    else str_difference = difference.ToString();

                    str_out = main_table.Rows[row][column - 1].ToString() + " (" + str_difference + ")";
                }

                return str_out;
            }

            find_0();
            ViewData["DataTable"] = DataTableToHTML(main_table);
        }

        private int check_cart_exist(string id) //ПРОВЕРКА НА СУЩЕСТВОВАНИЕ КАРТРИДжА
        {
            try
            {
                string sqlExpression = "SELECT model FROM cartridges WHERE id=?id";

                using (var connection = new MySqlConnection(sql_connection()))
                {
                    connection.Open();

                    using var command = new MySqlCommand(sqlExpression, connection);
                    command.Parameters.AddWithValue("?id", id);

                    using var reader = command.ExecuteReader();
                    {
                        if (reader.HasRows) // если есть данные
                        {
                            return 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return -1;
            }
            return 0;
        }

        private string get_printer_by_cart(string cartridge_id) //ПОЛУЧЕНИЕ ПРИНТЕРА ПО КАРТРИДЖУ
        {
            try
            {
                string sqlExpression = "SELECT cartridges,name FROM printers";

                using (var connection = new MySqlConnection(sql_connection()))
                {
                    connection.Open();

                    using var command = new MySqlCommand(sqlExpression, connection);
                    //command.Parameters.AddWithValue("@printer", printer);

                    using var reader = command.ExecuteReader();
                    {
                        string printers = "No data";

                        if (reader.HasRows) // если есть данные
                        {

                            while (reader.Read())   // построчно считываем данные
                            {
                                string tmp = reader.GetString(0);
                                string[] cartridges = tmp.Split(',');
                                string printer = reader.GetString(1);

                                foreach (string cartr in cartridges)
                                {
                                    if (cartr == cartridge_id)
                                    {
                                        if (printers == "No data") printers = printer;
                                        else printers = printers + ", " + printer;
                                    }
                                }
                            }
                        }
                        return printers;
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error, " + ex;
            }
        }

        private void find_0() //ОБРАБОТКА ПУСТЫХ ЯЧЕЕК 
        {
            for (int i = 0; i < main_table.Rows.Count; i++)
            {
                for (int j = 0; j < main_table.Columns.Count; j++)
                {
                    if (main_table.Rows[i][j] == null || main_table.Rows[i][j].ToString() == "")
                    {
                        main_table.Rows[i][j] = "0";
                    }
                }
            }
        }

        public string DataTableToHTML(DataTable dt) //КОВЕРТАЦИЯ ТАбЛИЦЫ В HTML
        {
            string sqlExpression = "SELECT model,yellow_zone FROM cartridges";
            DataTable yellow_zone_table = new DataTable();

            using (var connection = new MySqlConnection(sql_connection()))
            {
                connection.Open();

                using var command = new MySqlCommand(sqlExpression, connection);
                //command.Parameters.AddWithValue("@printer", printer);

                using var reader = command.ExecuteReader();
                {
                    if (reader.HasRows) // если есть данные
                    {
                        yellow_zone_table.Load(reader);
                    }
                }
            }

            string html = "<table class=\"table\">";

            // HEADERS
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                if (i == 2)
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
                            else if (count <= getYellowZone(dt.Rows[i][0].ToString())) html += "<td style=\"min-width: 100px;border-right-width: 4px;border-left-width: 4px; " +
                                    "border-color: dimgray; background-color: khaki;border-bottom-color: #dee2e6;border-top-color: #dee2e6;\">" + dt.Rows[i][j].ToString() + "</td>";
                            else html += "<td style=\"min-width: 100px;border-right-width: 4px;border-left-width: 4px; " +
                                    "border-color: dimgray;border-bottom-color: #dee2e6;border-top-color: #dee2e6;background-color: darkseagreen;\">" + dt.Rows[i][j].ToString() + "</td>";
                        }
                        else
                        {
                            if (count == 0) html += "<td style=\"min-width: 100px; background-color: salmon;\">" + dt.Rows[i][j].ToString() + "</td>";
                            else if (count <= getYellowZone(dt.Rows[i][0].ToString())) html += "<td style=\"min-width: 100px; background-color: khaki;\">" + dt.Rows[i][j].ToString() + "</td>";
                            else html += "<td style=\"min-width: 100px;background-color: darkseagreen;\">" + dt.Rows[i][j].ToString() + "</td>";
                        }
                    }
                    else html += "<td style=\"min-width: 100px;\">" + dt.Rows[i][j].ToString() + "</td>";
                }

                html += "</tr>";
            }
            html += "</table>";
            return html;

            int getYellowZone(string cartridge)
            {
                foreach (DataRow dr in yellow_zone_table.Rows) 
                {
                    if (dr[0].ToString() == cartridge) return Convert.ToInt32(dr[1]);
                }
                return 2;
            }
        }
    }
}