using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain
{
   
    public class ConditionalRequiredAttribute : ValidationAttribute
    {
        public string ConditionalPropertyName { get; }
        public object ExpectedValue { get; }

        public ConditionalRequiredAttribute(string conditionalPropertyName, object expectedValue, string errorMessage = "The field is required")
        {
            ConditionalPropertyName = conditionalPropertyName;
            ExpectedValue = expectedValue;
            ErrorMessage = errorMessage;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var conditionalProperty = validationContext.ObjectType.GetProperty(ConditionalPropertyName);
            if (conditionalProperty == null)
            {
                return new ValidationResult($"Unknown property: {ConditionalPropertyName}");
            }

            var conditionalPropertyValue = conditionalProperty.GetValue(validationContext.ObjectInstance);
            if (conditionalPropertyValue != null && conditionalPropertyValue.Equals(ExpectedValue))
            {
                //if (string.IsNullOrEmpty((string)value))
                //{
                //    return new ValidationResult(ErrorMessage);
                //}

                if (value == null)
                {
                    return new ValidationResult(ErrorMessage);
                }

                if (value is string strValue)
                {
                    if (string.IsNullOrWhiteSpace(strValue))
                    {
                        return new ValidationResult(ErrorMessage);
                    }
                }
                else if (value is DateTime dateValue)
                {
                    if (dateValue == default(DateTime)) // e.g., not set
                    {
                        return new ValidationResult(ErrorMessage);
                    }
                }


            }

            return ValidationResult.Success;
        }
    }

}
