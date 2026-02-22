namespace InsureX.Domain.Entities
{
    public class Claim : BaseEntity  // Inherit from BaseEntity
    {
        public string Description { get; set; } = string.Empty;
        // other properties
    }
}