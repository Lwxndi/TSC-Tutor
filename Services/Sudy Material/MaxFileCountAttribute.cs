// Validation/MaxFileCountAttribute.cs
using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.Services
{
    public class MaxFileCountAttribute : ValidationAttribute
    {
        private readonly int _maxCount;

        public MaxFileCountAttribute(int maxCount)
        {
            _maxCount = maxCount;
            ErrorMessage = $"You can upload a maximum of {maxCount} files.";
        }

        public override bool IsValid(object value)
        {
            if (value is List<IFormFile> files)
            {
                return files.Count <= _maxCount;
            }

            // No files, or wrong type entirely — let [Required] handle the empty case
            return true;
        }
    }
}