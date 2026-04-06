using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;
using u23641925_HW02.Models;

namespace u23641925_HW02.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //post results in view 
            DefaultDataService service = new DefaultDataService(); 

            ViewBag.TotalAdopted = service.GetCountAdopted();
            var adopted = service.GetAllAdoptions();

            return View(adopted);
        }

        public ActionResult About()
        {

            DefaultDataService service = new DefaultDataService();
            var user = service.GetUsers();
            var type = service.GetTypes();
            var breed = service.GetBreed();
            var location = service.GetLocations();
            var gender = service.GetGender();

            var vm = new PetUserViewModel
            {
                Users = user,
                Types = type,
                Breeds = breed,
                Locations = location,
                Genders = gender
            };

            return View(vm);
        }

        public ActionResult Contact()
        {
            DefaultDataService service = new DefaultDataService();
            var donations = service.GetDonations();
            var users = service.GetUsers();
            decimal total = donations.Sum(d => d.Amount);

            var vm = new PetUserViewModel
            {
                Users = users,
                Donation = donations,
                TotalRaised = total,
                DonationGoal = 30000
            };

            return View(vm);
        }


        public ActionResult ShowPets()
        {
            DefaultDataService service = new DefaultDataService();

            var vm = new PetUserViewModel
            {
                Pets = service.GetAllPets(),
                Types = service.GetTypes(),
                Breeds = service.GetBreed(),
                Locations = service.GetLocations(),
                Users = service.GetUsers()
            };

            return View(vm);
        }

        //show the details of the pet based on what id was clicked
        public ActionResult PetDetails(int? id)
        {
            if (id == null)
            {
                return HttpNotFound(); //if the pet chosen is not in database
            }

            DefaultDataService service = new DefaultDataService(); //sql connection 

            var pets = service.GetAllPets(); // get all pets in the database

            var pet = pets.FirstOrDefault(p => p.Pet_ID == id);

            if (pet == null)
            {
                return HttpNotFound(); //if the pet chosen is not in database
            }

            var users = service.GetUsers();

            // ✅ Pass both to the view
            var vm = new PetUserViewModel
            {
                Pet = pet,
                Users = users
            };

            return View(vm);
        }


        // method to update the sql table abopt
        [HttpPost]
        public ActionResult DoAdoptUpdate(int User_ID, string Pet_Name, int Pet_ID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Insert adoption record
                    string insert = "INSERT INTO Adopted (User_ID, Pet_ID) VALUES (@User_ID, @Pet_ID)";
                    using (SqlCommand cmd = new SqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@User_ID", User_ID);
                        cmd.Parameters.AddWithValue("@Pet_ID", Pet_ID);
                        cmd.ExecuteNonQuery();
                    }

                    // Mark the pet as adopted
                    string update = "UPDATE Pet SET Availabilty = 'Adopted' WHERE Pet_ID = @Pet_ID";
                    using (SqlCommand cmd2 = new SqlCommand(update, conn))
                    {
                        cmd2.Parameters.AddWithValue("@Pet_ID", Pet_ID);
                        cmd2.ExecuteNonQuery();
                    }
                }

                ViewBag.Message = $"✅ {Pet_Name} has been successfully adopted!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "❌ Error: " + ex.Message;
            }

            DefaultDataService service = new DefaultDataService();
            var users = service.GetUsers();
            var pet = service.GetAllPets().FirstOrDefault(p => p.Pet_ID == Pet_ID);

            var vm = new PetUserViewModel
            {
                Pet = pet,
                Users = users
            };

            return View("PetDetails", vm);
        }

        //convert the images to b64
        private string ConvertToBase64(HttpPostedFileBase imageFile)
        {
            if (imageFile == null || imageFile.ContentLength == 0)
                return null;

            using (var binaryReader = new System.IO.BinaryReader(imageFile.InputStream))
            {
                byte[] imageBytes = binaryReader.ReadBytes(imageFile.ContentLength);
                return Convert.ToBase64String(imageBytes);
            }
        }


        //update table pet and image in sql
        [HttpPost]
        public ActionResult DoUpdatePetPost(PetUserViewModel model, HttpPostedFileBase PetImage)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            try
            {
                string base64Image = ConvertToBase64(PetImage);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    string sqlPet = @"INSERT INTO Pet 
                              (Breed_ID, Gender_ID, Location_ID, Pet_Name, Pet_Age, Pet_Weight, 
                               Pet_Long_Description, Pet_Short_Description)
                              VALUES 
                              (@Breed_ID, @Gender_ID, @Location_ID, @Pet_Name, @Pet_Age, 
                               @Pet_Weight, @Pet_Long_Description, @Pet_Short_Description);
                              SELECT SCOPE_IDENTITY();";

                    int newPetId;
                    using (SqlCommand cmdPet = new SqlCommand(sqlPet, conn, transaction))
                    {
                        cmdPet.Parameters.AddWithValue("@Breed_ID", model.Breed_ID);
                        cmdPet.Parameters.AddWithValue("@Gender_ID", model.Gender_ID);
                        cmdPet.Parameters.AddWithValue("@Location_ID", model.Location_ID);
                        cmdPet.Parameters.AddWithValue("@Pet_Name", model.Pet_Name ?? (object)DBNull.Value);
                        cmdPet.Parameters.AddWithValue("@Pet_Age", model.Pet_Age ?? (object)DBNull.Value);
                        cmdPet.Parameters.AddWithValue("@Pet_Weight", model.Pet_Weight ?? (object)DBNull.Value);
                        cmdPet.Parameters.AddWithValue("@Pet_Long_Description", (object)model.Long_Descript ?? DBNull.Value);
                        cmdPet.Parameters.AddWithValue("@Pet_Short_Description", (object)model.Short_Descript ?? DBNull.Value);

                        newPetId = Convert.ToInt32(cmdPet.ExecuteScalar());
                    }

                    if (!string.IsNullOrEmpty(base64Image))
                    {
                        string sqlImage = "INSERT INTO Pet_Image (Pet_ID, B64Image) VALUES (@Pet_ID, @B64Image)";
                        using (SqlCommand cmdImg = new SqlCommand(sqlImage, conn, transaction))
                        {
                            cmdImg.Parameters.AddWithValue("@Pet_ID", newPetId);
                            cmdImg.Parameters.AddWithValue("@B64Image", base64Image);
                            cmdImg.ExecuteNonQuery();
                        }
                    }

                    string sqlPost = "INSERT INTO Post (User_ID, Pet_ID) VALUES (@User_ID, @Pet_ID)";
                    using (SqlCommand cmdPost = new SqlCommand(sqlPost, conn, transaction))
                    {
                        cmdPost.Parameters.AddWithValue("@User_ID", model.User_ID);
                        cmdPost.Parameters.AddWithValue("@Pet_ID", newPetId);
                        cmdPost.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    ViewBag.Message = $"✅ Pet '{model.Pet_Name}' posted successfully!";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "❌ Error: " + ex.Message;
            }

            DefaultDataService service = new DefaultDataService();
            var vm = new PetUserViewModel
            {
                Users = service.GetUsers(),
                Types = service.GetTypes(),
                Breeds = service.GetBreed(),
                Locations = service.GetLocations(),
                Genders = service.GetGender()
            };

            return View("About", vm);
        }


        //get donation details in sql
        [HttpPost]
        public ActionResult AddDonation(int User_ID, decimal Amount)
        {
            if (Amount < 5)
            {
                TempData["Message"] = "Donation must be at least R5.00";
                return RedirectToAction("Contact");
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    conn.Open();
                    string sql = "INSERT INTO Donate (User_ID, Amount) VALUES (@User_ID, @Amount)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@User_ID", User_ID);
                        cmd.Parameters.AddWithValue("@Amount", Amount);
                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["Message"] = $"✅ Donation of R{Amount} added successfully!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "❌ Error: " + ex.Message;
            }

            return RedirectToAction("Contact");
        }

        //calculate amount 
        [HttpGet]
        public JsonResult GetTotalDonations()
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                conn.Open();
                string sql = "SELECT ISNULL(SUM(Amount), 0) FROM Donate";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    var total = Convert.ToDecimal(cmd.ExecuteScalar());
                    return Json(new { total = total }, JsonRequestBehavior.AllowGet);
                }
            }
        }

    }
}
    
