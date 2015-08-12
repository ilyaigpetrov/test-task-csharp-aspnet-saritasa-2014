using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SampleMVCSolution.Models
{
    [CustomValidation(typeof(ExchangeRangeViewModel), "Validate")]
    public class ExchangeRangeViewModel
    {
        private DateTime startDate;
        [Required()]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)] // Works only if get/set are defined.
        public DateTime StartDate {
            get { return startDate; }
            set { startDate = value; }
        }

        private DateTime endDate;
        [Required()]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)] // Works only if get/set are defined.
        public DateTime EndDate {
            get { return endDate; }
            set { endDate = value; }
        }

        [Required()]
        [StringLength(3, ErrorMessage = "Currency code must have length of three characters.")]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be specified by three uppercase letters.")]
        public string TargetCurrencyCode { get; set; }

        public static ValidationResult Validate(ExchangeRangeViewModel exchangeRange, ValidationContext validationContext)
        {
            if (exchangeRange.StartDate > exchangeRange.EndDate)
                return new ValidationResult("StartDate mustn't be greater than EndDate.", new List<string> { "StartDate", "EndDate" });
            return ValidationResult.Success;
        }

        public object ToRouteValues()
        {
            return new {
                startDate = this.StartDate.ToString("yyyy-MM-dd"),
                endDate   = this.EndDate.ToString("yyyy-MM-dd"),
                targetCurrencyCode = this.TargetCurrencyCode
            };
        }
    }
}