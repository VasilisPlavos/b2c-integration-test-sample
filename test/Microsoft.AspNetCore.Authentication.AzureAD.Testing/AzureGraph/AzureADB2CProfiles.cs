using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.Authentication.AzureAD.Testing
{
    internal class AzureADB2CProfiles
    {
        [Required]
        public string ROPC { get; set; }
    }
}
