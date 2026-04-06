using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace u23641925_HW02.Models
{
	public class PetUserViewModel
	{
        public Pet Pet { get; set; }
        public List<Pet> Pets { get; set; }
        public User User { get; set; }
        public List<User> Users { get; set; }
        public Type Type { get; set; }
        public List<Type> Types { get; set; }
        public Location Location { get; set; }
        public List<Location> Locations { get; set; }
        public Breed Breed { get; set; }
        public List<Breed> Breeds { get; set; }
        public Gender Gender { get; set; }
        public List<Gender> Genders { get; set; }

        public int User_ID { get; set; }
        public int Breed_ID { get; set; }
        public int Gender_ID { get; set; }
        public int Location_ID { get; set; }

        public string Pet_Name { get; set; }
        public string Pet_Age { get; set; }
        public string Pet_Weight { get; set; }
        public string Long_Descript { get; set; }
        public string Short_Descript { get; set; }


        public Donate Donate { get; set; }
        public List<Donate> Donation { get; set; }
        public decimal TotalRaised { get; set; }
        public decimal DonationGoal { get; set; } = 30000;

    }
}