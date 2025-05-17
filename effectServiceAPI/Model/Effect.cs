using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace effectServiceAPI.Model

{
    public class Effect
    {
        [BsonId]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.String)]
        public Guid EffectId { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public Guid Seller { get; set; }
        public decimal MinimumPrice { get; set; }
        public EffectStatus EffectStatus { get; set; } = EffectStatus.InStock;
        public Guid AppraisalId { get; set; }
        public Guid? Buyer { get; set; }
        public decimal? SoldFor { get; set; }
    }

    public enum EffectStatus
    {
        InStock,
        OnAuction,
        Sold
    }
}