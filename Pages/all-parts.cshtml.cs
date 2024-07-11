using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using static Cart_Inventory.Pages.new_inventModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Cart_Inventory.Pages
{
    public class all_cartsModel : PageModel
    {
        private readonly IConfiguration Configuration;

        public all_cartsModel(IConfiguration configuration)
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
        public class main_table_model
        {
            public string? id { get; set; }
            public string? model { get; set; }
            public string? barcode { get; set; }
        }

        public class new_model
        { 
            public string? name { get; set; }
            public string? barcode { get; set; }
        }

        public class delete_module
        {
            public string? id { get; set; }
        }

        public List<main_table_model>? main_table { get; set; }

        public void OnGet()
        {
            LoadMainTable();
        }

        private void LoadMainTable() //ЗАГРУЗКА ТАБЛИЦЫ С КАРТРИДЖАМИ И МОДУЛЯМИ
        {
            using var connection = new MySqlConnection(sql_connection());
            connection.Open();

            using var command = new MySqlCommand("SELECT * FROM cartridges", connection);

            using var reader_main = command.ExecuteReader(); // ПОЛУЧЕНИЕ ТАБЛИЦЫ ИНВЕНТАРИЗАЦИЙ
            if (reader_main.HasRows)
            {
                DataTable dt = new DataTable();
                dt.Load(reader_main);

                string serializeObject = JsonConvert.SerializeObject(dt);
                var dataTableObjectInPOCO = JsonConvert.DeserializeObject<List<main_table_model>>(serializeObject);
                main_table = dataTableObjectInPOCO;
            }
        }

        public IActionResult OnPostAdd_new_model([FromForm] new_model model) //ОБРАБОТКА ПРИ ДОБАВЛЕНИИ МОДЕЛИ
        {
            try
            {
                string sqlExpression = "INSERT INTO cartridges (model, barcode) VALUES (?model, ?barcode)";

                using (var connection = new MySqlConnection(sql_connection()))
                {
                    connection.Open();

                    using var command = new MySqlCommand(sqlExpression, connection);

                    command.Prepare();

                    int error = 0;
                    //--------------------------------НАИМЕНОВАНИЕ КАРТРИДЖА------------------
                    if (model.name != null && model.name != "" && model.name.Contains("/") && model.name.Contains(",")) 
                    {
                        command.Parameters.AddWithValue("?model", model.name);
                    }
                    else error++;
                    //---------------------------------------------------

                    //--------------------------------ШТРИХ-КОД------------------
                    if (model.barcode != null && model.barcode != "")
                    {
                        command.Parameters.AddWithValue("?barcode", model.barcode);
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
            return Page();
        }

        public IActionResult OnPostDelete_module([FromForm] delete_module model) //ОБРАБОТКА ПРИ УДАЛЕНИИ МОДЕЛИ
        {
            try
            {
                string sqlExpression = "DELETE FROM cartridges WHERE (ID = ?id)";

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
            return Page();
        }
    }
}
