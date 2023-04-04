using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Testing;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Testing
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var client = new HttpClient())
            {
                //login into API and get token
                var endpoint = new Uri("http://test-demo.aemenersol.com/api/Account/Login");
                var newPost = new login()
                {
                    username = "user@aemenersol.com",
                    password = "Test@123",
                };
                var test = JsonConvert.SerializeObject(newPost);
                var payload = new StringContent(test, Encoding.UTF8, "application/json");
                var result = client.PostAsync(endpoint, payload).Result.Content.ReadAsStringAsync().Result;

                //Remove first and last character from string
                result = result[1..];
                result = result.Remove(result.Length - 1);
                Console.WriteLine(result);

                //Add Authorization
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + result);

                using (var response = await client.GetAsync("http://test-demo.aemenersol.com/api/PlatformWell/GetPlatformWellActual"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var Fresult = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(Fresult);

                        var platform = JsonSerializer.Deserialize<List<Platform>>(Fresult);

                        Console.WriteLine(platform);

                        SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Testing;Trusted_Connection=True;TrustServerCertificate=True");

                        if (platform != null)
                        {
                            foreach (var Platform in platform) 
                            {
                                string sqlplatform = "update platform set id=@id,uniqueName=@uniqueName,latitude=@latitude,longitude=@longitude," +
                                        "createdAt=@createdAt,updatedAt=@updatedAt where id=@id if @@ROWCOUNT = 0 INSERT INTO platform (id,uniqueName) values(@id,@uniqueName)";
                                conn.Open();
                                SqlCommand cmdplatform = new SqlCommand(sqlplatform, conn);
                                cmdplatform.Parameters.Add("@id", SqlDbType.Int);
                                cmdplatform.Parameters.Add("@uniqueName", SqlDbType.VarChar);
                                cmdplatform.Parameters.Add("@latitude", SqlDbType.Float);
                                cmdplatform.Parameters.Add("@longitude", SqlDbType.Float);
                                cmdplatform.Parameters.Add("@createdAt", SqlDbType.VarChar);
                                cmdplatform.Parameters.Add("@updatedAt", SqlDbType.VarChar);
                                cmdplatform.Parameters["@id"].Value = Platform.id;
                                cmdplatform.Parameters["@uniqueName"].Value = Platform.uniqueName;
                                cmdplatform.Parameters["@latitude"].Value = Platform.latitude;
                                cmdplatform.Parameters["@longitude"].Value = Platform.longitude;
                                cmdplatform.Parameters["@createdAt"].Value = Platform.createdAt;
                                cmdplatform.Parameters["@updatedAt"].Value = Platform.updatedAt;
                                cmdplatform.ExecuteNonQuery();
                                conn.Close();
                                Console.WriteLine($"{Platform.id} {Platform.uniqueName}");
                                //Console.WriteLine($"{Platform.id} {Platform.uniqueName} {Platform.well}");
                                foreach (var well in Platform.well)
                                {
                                    string sql = "update well set id=@id,platformId=@platformId,uniqueName=@uniqueName,latitude=@latitude,longitude=@longitude," +
                                        "createdAt=@createdAt,updatedAt=@updatedAt where id=@id if @@ROWCOUNT = 0 INSERT INTO well (id,platformId,uniqueName) values(@id,@platformId,@uniqueName)";
                                    conn.Open();
                                    SqlCommand cmd = new SqlCommand(sql, conn);
                                    cmd.Parameters.Add("@id", SqlDbType.Int);
                                    cmd.Parameters.Add("@platformId", SqlDbType.Int);
                                    cmd.Parameters.Add("@uniqueName", SqlDbType.VarChar);
                                    cmd.Parameters.Add("@latitude", SqlDbType.Float);
                                    cmd.Parameters.Add("@longitude", SqlDbType.Float);
                                    cmd.Parameters.Add("@createdAt", SqlDbType.VarChar);
                                    cmd.Parameters.Add("@updatedAt", SqlDbType.VarChar);
                                    cmd.Parameters["@id"].Value = well.id;
                                    cmd.Parameters["@platformId"].Value = well.platformId;
                                    cmd.Parameters["@uniqueName"].Value = well.uniqueName;
                                    cmd.Parameters["@latitude"].Value = well.latitude;
                                    cmd.Parameters["@longitude"].Value = well.longitude;
                                    cmd.Parameters["@createdAt"].Value = well.createdAt;
                                    cmd.Parameters["@updatedAt"].Value = well.updatedAt;
                                    cmd.ExecuteNonQuery();
                                    conn.Close();
                                    Console.WriteLine($"{well.id} {well.uniqueName}");
                                }
                            }
                        }
                        //var db = new consoleDatabase();
                        //db.Platform?.AddRange(platform);
                        //db.SaveChanges();
                    }
                    else
                    {
                        Console.WriteLine("Failed to call function. Status code: " + response.StatusCode);
                    }
                }

            }
        }

    }
}
