using System.ComponentModel.DataAnnotations;

namespace BaGet.DropBox
{
    public class DropBoxOptions
    {
        [Required]
        public string AccessToken { get; set; }
    }
}