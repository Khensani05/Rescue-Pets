using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace u23641925_HW02.Models
{
    public class DefaultDataService
    {
        //variables for connection string and lists/tables in the DB
        private String ConnectionString;
        public static List<Pet> Pets = null;


        public DefaultDataService() 
        {
            // Try get the connection string safely
            var connSetting = ConfigurationManager.ConnectionStrings["DefaultConnection"];

            if (connSetting == null)
            {
                throw new InvalidOperationException(
                    "❌ Connection string 'DefaultConnection' was not found in Web.config. " +
                    "Make sure <connectionStrings> is defined directly under <configuration>."
                );
            }

            ConnectionString = connSetting.ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "❌ Could not connect to DB3 with the provided connection string. " +
                    "Check server name, instance, and DB name.", ex);

            }
        }

        //select statements
        //method to get all Adoptions from the adopt table in SQL
        public List<Adopted> GetAllAdoptions()
        {
            List<Adopted> adopted = new List<Adopted>();

            //open sql connection to get the items 
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                    string query = @"
                SELECT u.User_Name, p.Pet_Name
                FROM Adopted a
                INNER JOIN [User] u ON a.User_ID = u.User_ID
                INNER JOIN Pet p ON a.Pet_ID = p.Pet_ID";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    adopted.Add(new Adopted
                    {
                        User_Name = reader.GetString(0),
                        Pet_Name = reader.GetString(1)
                    });
                }

                return adopted;
            }
        }

        //get the count of how many adoptions made
        public int GetCountAdopted()
        {
            int count = 0;

            //open sql connection to read from the Adopted table
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string sql = "SELECT COUNT(*) FROM Adopted";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                count = (int)cmd.ExecuteScalar();
            }

            return count;
        }

        //method to all the pets in Pet table in sql
        public List<Pet> GetAllPets()
        {

            List<Pet> pets = new List<Pet>();

            //open sql connection to get details from pet table
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                //CASE STATEMENT THAT SAYS IF PET IS IN ADOPT TABLE CHANGE AVAILABILITY
                string query = @"SELECT 
                        pb.Breed_Name, 
                        g.Gender_Name, 
                        l.Location_Name, 
                        p.Pet_Name, p.Pet_Age, 
                        p.Pet_Short_Description, 
                        pi.[B64Image], 
                        p.Pet_Weight,
                        CASE
                            WHEN a.Pet_ID IS NOT NULL THEN 'Adopted'
                            ELSE 'Available'
                        END AS Availability,
                        (SELECT pt.Type_Name
                        FROM Pet_Type pt 
                        INNER JOIN Pet_Breed pb2 ON pt.Type_ID = pb2.Type_ID
                        WHERE pb2.Breed_ID = p.Breed_ID) AS Type_Name,
                        p.Pet_ID,
                        p.Pet_Long_Description,
                        po.Post_ID,
                        u.User_Name,
                        pt.Type_ID,
                        pb.Breed_ID,
                        l.Location_ID
                        FROM Pet p
                        INNER JOIN Pet_Breed pb ON p.Breed_ID  = pb.Breed_ID
                        INNER JOIN Pet_Type pt ON pb.Type_ID = pt.Type_ID 
                        INNER JOIN Gender g ON p.Gender_ID = g.Gender_ID
                        INNER JOIN [Location] l ON p.Location_ID  = l.Location_ID
                        INNER JOIN Pet_Image pi ON p.Pet_ID = pi.Pet_ID
                        LEFT JOIN Adopted a ON p.Pet_ID = a.Pet_ID
                        INNER JOIN Post po ON p.Pet_ID = po.Pet_ID
                        INNER JOIN [User] u ON po.User_ID = u.User_ID;";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    pets.Add(new Pet
                    {
                        Breed_Name = reader.IsDBNull(0) ? "" : reader.GetString(0),
                        Gender_Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Location_Name = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Pet_Name = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Pet_Age = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        Short_Descript = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        ImageRaw = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        Pet_Weight = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        Availabilty = reader.IsDBNull(8) ? "Available" : reader.GetString(8),
                        Type_Name = reader.IsDBNull(9) ? "" : reader.GetString(9),
                        Pet_ID = reader.IsDBNull(10) ? 0 : reader.GetInt32(10),
                        Long_Descript = reader.IsDBNull(11) ? "" : reader.GetString(11),
                        Post_ID = reader.IsDBNull(12) ? 0 : reader.GetInt32(12),
                        User_Name = reader.IsDBNull(13) ? "" : reader.GetString(13),
                        Type_ID = reader.IsDBNull(12) ? 0 : reader.GetInt32(14),
                        Breed_ID = reader.IsDBNull(12) ? 0 : reader.GetInt32(15),
                        Location_ID = reader.IsDBNull(12) ? 0 : reader.GetInt32(16)
                    });
                }
            }

            return pets;
        }

        public List<User> GetUsers()
        {
            List<User> user = new List<User>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                //CASE STATEMENT THAT SAYS IF PET IS IN ADOPT TABLE CHANGE AVAILABILITY
                string query = @"SELECT 
                        u.[User_Name], 
                        n.Number,
                        u.User_ID
                        FROM [User] u 
                        INNER JOIN Number n ON u.User_ID = n.User_ID";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    user.Add(new User
                    {
                        User_Name = reader.GetString(0),
                        User_PNumber = reader.GetInt32(1),
                        User_ID = reader.GetInt32(2)
                    });
                }
            }

            return user;
        }

        //dropdown for searching
        public List<Breed> GetBreed()
        {
            List<Breed> breed = new List<Breed>();

            //open sql connection and get breed details
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"SELECT Breed_ID, Breed_Name, Type_ID FROM Pet_Breed";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    breed.Add(new Breed
                    {
                        Breed_ID = reader.GetInt32(0),
                        Breed_Name = reader.GetString(1),
                        Type_ID = reader.GetInt32(2)
                    });
                }
            }
            return breed;
        }

        //to get all pet types in the db
        public List<Type> GetTypes()
        {
            List<Type> types = new List<Type>();

            //open sql connection and get type details
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"SELECT Type_ID, Type_Name FROM Pet_Type";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    types.Add(new Type
                    {
                        Type_ID = reader.GetInt32(0),
                        Type_Name = reader.GetString(1)
                    });
                }
            }
            return types;
        }

        //to get all locations in the db
        public List<Location> GetLocations()
        {
            List<Location> locations = new List<Location>();

            //open sql connection and get type details
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"SELECT Location_ID, Location_Name FROM [Location]";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    locations.Add(new Location
                    {
                        Location_ID = reader.GetInt32(0),
                        Location_Name = reader.GetString(1)
                    });
                }
            }
            return locations;
        }

        //to get all genders in the db
        public List<Gender> GetGender()
        {
            List<Gender> genders = new List<Gender>();

            //open sql connection and get type details
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"SELECT Gender_ID, Gender_Name FROM Gender";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    genders.Add(new Gender
                    {
                        Gender_ID = reader.GetInt32(0),
                        Gender_Name = reader.GetString(1)
                    });
                }
            }
            return genders;
        }

        //FOR DONATEIONS
        public List<Donate> GetDonations()
        {
            List<Donate> donations = new List<Donate>();

            //open sql connection and get type details
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"SELECT Donate_ID, Amount, User_ID FROM Donate";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    donations.Add(new Donate
                    {
                        Donate_ID = reader.GetInt32(0),
                        Amount = reader.GetInt32(1),
                        User_ID = reader.GetInt32(2)
                    });
                }
            }

            return donations;
        }

        public int GetCountDonations()
        {
            int count = 0;

            //open sql connection to read from the Adopted table
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string sql = "SELECT COUNT(*) FROM Donate";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                count = (int)cmd.ExecuteScalar();
            }

            return count;
        }
    }
}
