using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using Newtonsoft.Json;
using System.Data;
using System.Reflection.PortableExecutable;
using static Cart_Inventory.Pages.new_inventModel;
using static Cart_Inventory.Pages.printersModel;

namespace Cart_Inventory.Pages
{
    public class edit_inventModel : PageModel
    {
        private readonly IConfiguration Configuration;

        public edit_inventModel(IConfiguration configuration)
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
            loadInvents();
        }

        public class InputModelLoad
        {
            public string? text { get; set; }
        }

        public class InputModelUpdate
        {
            public string? text { get; set; }
            public string? id { get; set; }
        }

        public class main_table_model
        {
            public string? name { get; set; }
            public string? count { get; set; }
        }

        public class InputModelDelete
        {
            public string? invent_id { get; set; }
        }

        public List<string>? all_invents { get; set; } = new List<string>();

        public List<main_table_model>? main_table { get; set; }

        private void loadInvents() //ЗАГРУЗКА ВСЕХ МОДУЛЕЙ ПРИНТЕРОВ
        {
            string sqlExpression = "SELECT id, date, invent_table FROM invent";

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
                            string table = "";
                            if (reader.GetInt32(2) == 0) table = "Картриджи";
                            else table = "Модули";
                            all_invents.Add(reader.GetInt32(0) + " - " + reader.GetString(1) + " - " + table);
                        }
                    }
                }
            }
        }

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

        public IActionResult OnPostLoadEditableInvent([FromBody] InputModelLoad model) //ОБРАБОТКА ПРИ ЗАГРУЗКЕ ИНВЕНТАРИЗАЦИИ
        {
            string sqlExpression = "SELECT invent FROM invent WHERE id=?id";
            string[] tmp = model.text.Split(" - ");

            using (var connection = new MySqlConnection(sql_connection()))
            {
                connection.Open();

                using var command = new MySqlCommand(sqlExpression, connection);
                command.Parameters.AddWithValue("?id", tmp[0]);

                using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("name");
                    dt.Columns.Add("count");

                    while (reader.Read())
                    {
                        string raw_string = reader.GetString(0);
                        string[] tmp1 = raw_string.Split(",");
                        foreach (string s in tmp1)
                        {
                            string[] tmp2 = s.Split("/");
                            dt.Rows.Add(tmp2[0], tmp2[1]);
                        }
                    }

                    string serializeObject = JsonConvert.SerializeObject(dt);
                    var dataTableObjectInPOCO = JsonConvert.DeserializeObject<List<main_table_model>>(serializeObject);
                    main_table = dataTableObjectInPOCO;

                    return new JsonResult(main_table);
                }
            }
            return new JsonResult(new List<main_table_model>());
        }

        public IActionResult OnPostSave_invent([FromBody] InputModelUpdate model) //ОБРАБОТКА ПРИ СОХРАНЕНИИ ИНВЕНТАРИЗАЦИИ
        {
            try
            {
                string sqlExpression = "UPDATE invent SET invent = ?invent WHERE id=?id";

                using (var connection = new MySqlConnection(sql_connection()))
                {
                    connection.Open();

                    using var command = new MySqlCommand(sqlExpression, connection);

                    command.Prepare();

                    int error = 0;
                    //--------------------------------СТРОКА ИНВЕНТАРИЗАЦИИ------------------
                    if (model.text != null && model.text != "")
                    {
                        command.Parameters.AddWithValue("?invent", model.text);
                    }
                    else error++;
                    //---------------------------------------------------

                    //--------------------------------ID------------------
                    if (model.text != null && model.text != "")
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
            loadInvents();
            return Page();
        }

        public IActionResult OnPostDeleteInventorization([FromForm] InputModelDelete model) //ОБРАБОТКА ПРИ УДАЛЕНИИ ИНВЕНТАРИЗАЦИИ
        {
            try
            {
                string sqlExpression = "DELETE FROM invent WHERE id=?id";

                using (var connection = new MySqlConnection(sql_connection()))
                {
                    connection.Open();

                    using var command = new MySqlCommand(sqlExpression, connection);

                    command.Prepare();

                    int error = 0;
                    //--------------------------------ID------------------
                    if (model.invent_id != null && model.invent_id != "")
                    {
                        command.Parameters.AddWithValue("?id", model.invent_id);
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
            loadInvents();
            return Page();
        }
    }
}
