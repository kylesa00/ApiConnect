namespace IO.Swagger.Models
{
    /// <summary>A quantity-multiple rule from [dbo].[ItemMoqPairCache].</summary>
    public class ItemMoqPairCacheEntry
    {
        public string ItemNo { get; set; }
        public byte Pair { get; set; }
        public int CentralMoq { get; set; }
    }
}
