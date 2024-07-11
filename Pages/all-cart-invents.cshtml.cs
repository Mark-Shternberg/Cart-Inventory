using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using System.Data;

namespace Cart_Inventory.Pages
{
    public class all_inventsModel : PageModel
    {
        private readonly IConfiguration Configuration;

        public all_inventsModel(IConfiguration configuration)
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

        public void OnGet()
        {
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

                for (int i = numberOfResults; i > 0; i--)   // построчно считываем данные
                {

                    string date = dt.Rows[i - 1][1].ToString();
                    main_table.Columns.Add(date);

                    string[] tmp = dt.Rows[i - 1][2].ToString().Split(',');

                    int column_id = main_table.Columns[date].Ordinal;
                    foreach (string item in tmp)
                    {
                        string[] tmp2 = item.Split("/");
                        if (check_cart_exist(tmp2[0]) == 1)
                        {

                            int row_id = find_row(tmp2[0]);
                            //main_table.Rows[row_id][column_id] = invent_difference(column_id, row_id, tmp2[1], 0);
                            main_table.Rows[row_id][column_id] = tmp2[1];
                            main_table.Rows[row_id][column_id - 1] = invent_difference(column_id, row_id, tmp2[1]);
                        }
                    }
                }
            }

            int find_row(string name) //ПОИСК СТРОКИ
            {
                int rowIndex = -1;

                for (int i = 0; i < main_table.Rows.Count; i++)
                {
                    if (main_table.Rows[i].Field<string>(0) != null && main_table.Rows[i].Field<string>(0) == name)
                    {
                        main_table.Rows[i][1] = get_printer_by_cart(name);
                        return i;
                    }
                }

                if (rowIndex == -1)
                {
                    main_table.Rows.Add(name);
                    int row_id = main_table.Rows.Count - 1;
                    main_table.Rows[row_id][1] = get_printer_by_cart(name);
                    return row_id;
                }
                return -1;
            }

            string invent_difference(int column, int row, string count) //РАЗНИЦА МЕЖДУ ИНВЕНТАРИЗАЦИЯМИ
            {
                string[] tmp = main_table.Rows[row][column - 1].ToString().Split(" (");
                string str_out = "";


                if (tmp.Count() > 1) str_out = tmp[0];
                else str_out = main_table.Rows[row][column - 1].ToString();

                if (str_out == "") str_out = "0";

                if (column > 2)
                {
                    if (main_table.Rows[row][column - 1] == null || main_table.Rows[row][column - 1].ToString() == "")
                    {
                        main_table.Rows[row][column - 1] = "0";
                    }
                    int old_count = Convert.ToInt32(count);

                    int difference = Convert.ToInt32(str_out) - old_count;
                    string str_difference;
                    if (difference > 0) str_difference = "+" + difference;
                    else if (difference == 0) return str_out;
                    else str_difference = difference.ToString();

                    str_out = main_table.Rows[row][column - 1].ToString() + " (" + str_difference + ")";
                }

                return str_out;
            }

            find_0();
            ViewData["DataTable"] = Functions.DataTableToHTML(main_table);
        }

        private int check_cart_exist(string cart) //ПРОВЕРКА НА СУЩЕСТВОВАНИЕ КАРТРИДжА
        {
            try
            {
                string sqlExpression = "SELECT model FROM cartridges";

                using (var connection = new MySqlConnection(sql_connection()))
                {
                    connection.Open();

                    using var command = new MySqlCommand(sqlExpression, connection);
                    //command.Parameters.AddWithValue("@printer", printer);

                    using var reader = command.ExecuteReader();
                    {
                        if (reader.HasRows) // если есть данные
                        {
                            while (reader.Read())   // построчно считываем данные
                            {
                                if (reader.GetString(0) == cart) return 1;
                            }
                            return 0;
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

        private string get_printer_by_cart(string cart) //ПОЛУЧЕНИЕ ПРИНТЕРА ПО КАРТРИДЖУ
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
                        if (reader.HasRows) // если есть данные
                        {
                            string printers = "No data";

                            while (reader.Read())   // построчно считываем данные
                            {
                                string tmp = reader.GetString(0);
                                string[] cartridges = tmp.Split(',');
                                string printer = reader.GetString(1);

                                foreach (string cartr in cartridges)
                                {
                                    if (cartr == cart)
                                    {
                                        if (printers == "No data") printers = printer;
                                        else printers = printers + ", " + printer;
                                    }
                                }
                            }
                            return printers;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error, " + ex;
            }
            return "Error";
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
    }
}
