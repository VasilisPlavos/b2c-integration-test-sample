using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.Authentication.AzureAD.Testing
{
    public class GraphOptions
    {
        [Required]
        public string Domain { get; set; }

        [Required]
        public string ClientId { get; set; }

        [Required]
        public string ClientSecret { get; set; }
    }
}
