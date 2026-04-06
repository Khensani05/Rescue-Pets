using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Web;

namespace u23641925_HW02.Models
{
	public class Pet
	{
		public int Pet_ID { get; set; }
		public string Pet_Name { get; set; }
		public string Pet_Age { get; set; }
		public string Pet_Weight { get; set; }
		public string Gender_Name { get; set; }
        public int Breed_ID { get; set; }
        public string Breed_Name { get; set; }
        public int Location_ID { get; set; }
        public string Location_Name { get; set; }
		public string Short_Descript { get; set; }
		public string Long_Descript { get; set; }
		public string Availabilty { get; set; }
		public int	Type_ID { get; set; }
		public string Type_Name { get; set; }
        public String ImageRaw { get; set; }
		public string User_Name { get; set; }
		public int User_PNumber { get; set; }
		public int Post_ID { get; set; }
    }
}