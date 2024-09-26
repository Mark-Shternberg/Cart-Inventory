using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using static Cart_Inventory.Pages.all_cartsModel;
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
            public string? yellow_zone { get; set; }
        }

        public class new_model
        { 
            public string? name { get; set; }
            public string? barcode { get; set; }
            public string? yellow_zone { get; set; }
        }

        public class update_model
        {
            public string? id { get; set; }
            public string? name { get; set; }
            public string? barcode { get; set; }
            public string? yellow_zone { get; set; }
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
            else main_table = null;
        }

        //public IActionResult OnPostAdd_new_model([FromBody] new_model model) //ОБРАБОТКА ПРИ ДОБАВЛЕНИИ МОДЕЛИ
        public IActionResult OnPostAdd_new_model([FromBody] new_model model) //ОБРАБОТКА ПРИ ДОБАВЛЕНИИ МОДЕЛИ
        {
            try
            {
                string sql = "INSERT INTO cartridges (model, barcode, yellow_zone) VALUES (@model, @barcode, @yellow_zone)";

                using var connection = new MySqlConnection(sql_connection());
                connection.Open();

                using var command = new MySqlCommand(sql, connection);

                if (ValidateModel(model))
                {
                    command.Parameters.AddWithValue("@model", model.name?.Trim());
                    command.Parameters.AddWithValue("@barcode", model.barcode?.Trim());
                    command.Parameters.AddWithValue("@yellow_zone", model.yellow_zone?.Trim());

                    command.ExecuteNonQuery();

                    // Возвращаем новую запись для добавления в таблицу
                    var lastId = command.LastInsertedId;

                    var newEntry = new main_table_model
                    {
                        id = lastId.ToString(),
                        model = model.name,
                        barcode = model.barcode,
                        yellow_zone = model.yellow_zone
                    };

                    LoadMainTable();
                    return new JsonResult(new { success = true, newModel = newEntry });
                }

                return new JsonResult(new { success = false, message = "Validation failed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        private bool ValidateModel(new_model model)
        {
            return !string.IsNullOrWhiteSpace(model.name) &&
                   !model.name.Contains("/") && !model.name.Contains(",") &&
                   !string.IsNullOrWhiteSpace(model.barcode) &&
                   !string.IsNullOrWhiteSpace(model.yellow_zone);
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

        public IActionResult OnPostEdit_module([FromBody] update_model model) //ОБРАБОТКА ПРИ РЕДАКТИРОВАНИИ МОДЕЛИ
        {
            try
            {
                string sqlExpression = "UPDATE cartridges SET model=@model, barcode=@barcode, yellow_zone=@yellow_zone WHERE id=@id";

                using (var connection = new MySqlConnection(sql_connection()))
                {
                    connection.Open();

                    using var command = new MySqlCommand(sqlExpression, connection);

                    int error = 0;

                    //---------------НАИМЕНОВАНИЕ КАРТРИДЖА------------------
                    if (!string.IsNullOrWhiteSpace(model.name) && !model.name.Contains("/") && !model.name.Contains(","))
                    {
                        string model_name = model.name.Trim(); // Удаление пробела в начале и конце
                        model_name = Regex.Replace(model_name, @"\s+", " "); // Замена множества пробелов на один

                        command.Parameters.AddWithValue("@model", model_name);
                    }
                    else
                    {
                        error++;
                    }
                    //---------------------------------------------------

                    //---------------------ШТРИХ-КОД-----------------------
                    if (!string.IsNullOrWhiteSpace(model.barcode))
                    {
                        command.Parameters.AddWithValue("@barcode", model.barcode);
                    }
                    else
                    {
                        error++;
                    }
                    //---------------------------------------------------

                    //--------------------ЖЁЛТАЯ ЗОНА----------------------
                    if (!string.IsNullOrWhiteSpace(model.yellow_zone))
                    {
                        command.Parameters.AddWithValue("@yellow_zone", model.yellow_zone);
                    }
                    else
                    {
                        error++;
                    }
                    //---------------------------------------------------

                    // Добавление id параметра
                    command.Parameters.AddWithValue("@id", model.id);

                    // Проверка ошибок и выполнение команды
                    if (error == 0)
                    {
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = "Validation failed" });
                    }
                }

                LoadMainTable();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
