using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Globalization;
using Microsoft.AspNetCore.Antiforgery;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MySqlConnector;
using Newtonsoft.Json;

namespace Cart_Inventory.Pages
{
    public class new_inventModel : PageModel
    {
        private readonly IConfiguration Configuration;

        public new_inventModel(IConfiguration configuration)
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
            LoadPage();
        }

        private void LoadPage()
        {
            string dateTime = DateTime.Now.ToString("d", new CultureInfo("en-US"));
            ViewData["TimeStamp"] = dateTime;
        }

        //--------ТЕХНИЧЕСКИЕ БЛОКИ--------

        public class Return_Data
        {
            public string? name { get; set; }

        }

        public class InputModel
        {
            public string? Text { get; set; }
        }

        public class InventoryFormModel
        {
            public string? CategoryId { get; set; }
            public string? raw_table { get; set; }
        }

        public class TableItem
        {
            public string? Name { get; set; }
            public int Count { get; set; }
        }
    
        //--------------------------------

        public IActionResult OnPostOnTextInputChanged([FromBody] InputModel data) //ОБРАБОТКА ПРИ ВВОДЕ В ТЕКСТОВЫЙ БЛОК
        {
            string barcode = data.Text;

            // Логика для получения данных на основе inputText
            string cartridge_name = get_cartridge(barcode);
            if (cartridge_name == "0") { return new JsonResult(""); }
            var resultData = GetDataBasedOnInput(cartridge_name);

            return new JsonResult(resultData);
        }

        private List<Return_Data> GetDataBasedOnInput(string inputText) //ОБРАБОТКА ПРИ ВВОДЕ В ТЕКСТОВЫЙ БЛОК
        {
            // Пример данных для демонстрации
            return new List<Return_Data>
            {
                new Return_Data { name = inputText }
            };
        }

        public IActionResult OnPostSubmitInventorization([FromForm] InventoryFormModel model) //ЗАПИСЬ В БАЗУ ДАННЫХ
        {
            try
            {
                string sqlExpression = "INSERT INTO invent (date, invent, invent_table) VALUES (?date, ?invent, ?invent_table)";

                using (var connection = new MySqlConnection(sql_connection()))
                {
                    connection.Open();

                    using var command = new MySqlCommand(sqlExpression, connection);

                    command.Prepare();

                    //--------------------------------ДАТА------------------
                    command.Parameters.AddWithValue("?date", DateTime.Now.ToString("d", new CultureInfo("en-US")));
                    //---------------------------------------------------

                    //--------------------------------ТАБЛИЦА------------------
                    int invent_table = 0;
                    if (model.CategoryId == "Cartridges") invent_table = 0;
                    else invent_table = 1;
                    command.Parameters.AddWithValue("?invent_table", invent_table);
                    //---------------------------------------------------

                    //--------------------------------КАРТРИДЖИ------------------
                    command.Parameters.AddWithValue("?invent", model.raw_table);
                    //---------------------------------------------------

                    //---------ЗАПИСЬ И ВЫХОД------------
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            LoadPage();
            return Page();
        }

        private string get_cartridge(string barcode) //ПОЛУЧЕНИЕ МОДЕЛИ КАРТРИДЖА ПО ШТРИХКОДУ
        {
            try
            {
                string sqlExpression = "SELECT barcode, model FROM cartridges";

                using (var connection = new MySqlConnection(sql_connection()))
                {
                    connection.Open();

                    using var command = new MySqlCommand(sqlExpression, connection);

                    using var reader = command.ExecuteReader();
                    {
                        if (reader.HasRows) // если есть данные
                        {
                            while (reader.Read())   // построчно считываем данные
                            {
                                string[] tmp = reader.GetString(0).Split(",");
                                foreach (string s in tmp)
                                {
                                    if (s == barcode) return reader.GetString(1);
                                }
                            }
                            return "0";
                        }
                        else return "0";
                    }
                }
            }
            catch (Exception ex)
            {
                return "0";
            }
        }
    }
}
