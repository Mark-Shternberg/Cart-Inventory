using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using Newtonsoft.Json;
using static Cart_Inventory.Pages.all_cartsModel;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace Cart_Inventory.Pages
{
    public class printersModel : PageModel
    {
        private readonly IConfiguration Configuration;

        public printersModel(IConfiguration configuration)
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

        public void OnGet()
        {
            LoadMainTable();
            loadCartridges();
        }

        public class main_table_model
        {
            public string? id { get; set; }
            public string? name { get; set; }
            public string? cartridges { get; set; }
        }

        public class cartridges_list
        {
            public string? name { get; set; }
        }

        public class new_printer
        {
            public string? name { get; set; }
            public string? raw_table { get; set; }
        }
        public class delete_printer
        {
            public string? id { get; set; }
        }


        public List<main_table_model>? main_table { get; set; }

        public List<string>? all_cartridges { get; set; } = new List<string>();

        private void LoadMainTable() //ЗАГРУЗКА ТАБЛИЦЫ С ПРИНТЕРАМИ
        {
            using var connection = new MySqlConnection(sql_connection());
            connection.Open();

            using var command = new MySqlCommand("SELECT * FROM printers", connection);

            using var reader_main = command.ExecuteReader(); // ПОЛУЧЕНИЕ ТАБЛИЦЫ ИНВЕНТАРИЗАЦИЙ
            if (reader_main.HasRows)
            {
                DataTable dt = new DataTable();
                dt.Load(reader_main);

                string serializeObject = JsonConvert.SerializeObject(dt);
                var dataTableObjectInPOCO = JsonConvert.DeserializeObject<List<main_table_model>>(serializeObject);
                main_table = dataTableObjectInPOCO;
            }
            else main_table = null;
        }

        private void loadCartridges() //ЗАГРУЗКА ВСЕХ МОДУЛЕЙ ПРИНТЕРОВ
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
                            all_cartridges.Add(reader.GetString(0));
                        }
                    }
                }
            }
        }

        public IActionResult OnPostAdd_new_printer([FromForm] new_printer model) //ОБРАБОТКА ПРИ ДОБАВЛЕНИИ МОДЕЛИ
        {
            try
            {
                string sqlExpression = "INSERT INTO printers (name, cartridges) VALUES (?name, ?cartridges)";

                using (var connection = new MySqlConnection(sql_connection()))
                {
                    connection.Open();

                    using var command = new MySqlCommand(sqlExpression, connection);

                    command.Prepare();

                    int error = 0;
                    //--------------------------------НАИМЕНОВАНИЕ ПРИНТЕРА------------------
                    if (model.name != null && model.name != "")
                    {
                        string model_name = model.name;
                        if (model_name.EndsWith(" "))
                        {
                            model_name = model_name.TrimEnd(' '); // Удаление пробела в конце при его наличии
                        }
                        model_name = Regex.Replace(model_name, @"\s+", " "); // Замена множества пробелов на один

                        command.Parameters.AddWithValue("?name", model.name);
                    }
                    else error++;
                    //---------------------------------------------------

                    //--------------------------------СВЯЗАННЫЕ МОДУЛИ------------------
                    if (model.raw_table != null && model.raw_table != "")
                    {
                        command.Parameters.AddWithValue("?cartridges", model.raw_table);
                    }
                    else error++;
                    //---------------------------------------------------

                    //---------ЗАПИСЬ И ВЫХОД------------
                    if (error == 0)
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            LoadMainTable();
            loadCartridges();
            return Page();
        }

        public IActionResult OnPostDelete_printer([FromForm] delete_printer model) //ОБРАБОТКА ПРИ УДАЛЕНИИ МОДУЛЯ
        {
            try
            {
                string sqlExpression = "DELETE FROM printers WHERE (ID = ?id)";

                using (var connection = new MySqlConnection(sql_connection()))
                {
                    connection.Open();

                    using var command = new MySqlCommand(sqlExpression, connection);

                    command.Prepare();

                    int error = 0;
                    //--------------------------------ТАБЛИЦА------------------
                    if (model.id != null && model.id != "")
                    {
                        command.Parameters.AddWithValue("?id", model.id);
                    }
                    else error++;
                    //---------------------------------------------------

                    //---------ЗАПИСЬ И ВЫХОД------------
                    if (error == 0)
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            LoadMainTable();
            loadCartridges();
            return Page();
        }
    }
}
