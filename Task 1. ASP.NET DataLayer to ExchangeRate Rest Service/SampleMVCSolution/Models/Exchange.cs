using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SampleMVCSolution.Models
{
    public class Exchange
    {
        [Column(Order = 1), Key] 
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Column(Order = 2), Key]
        [StringLength(3, ErrorMessage = "Currency must be specified by three letters.")]
        [RegularExpression(@"^[A-Z]{3}$")]
        public string CurrencyCode { get; set; }

        public double Rate { get; set; }
    }
}