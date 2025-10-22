using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public List<string> ValidationErrors { get; }

        public ValidationException(List<string> errors) : base("Validation Failed")
        {
            ValidationErrors = errors;
        }
        public ValidationException(string error)
        {
            ValidationErrors = new List<string> { error };
        }
    }
}
