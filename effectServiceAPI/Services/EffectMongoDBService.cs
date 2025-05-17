using effectServiceAPI.Model;
using MongoDB.Driver;

namespace auctionServiceAPI.Services
{
    public interface IEffectService
    {
        Task<IEnumerable<Effect>> GetAllEffectsAsync();
        Task<Effect> GetEffectAsync(Guid id);
        Task<Guid> CreateEffectAsync(Effect effect);
        Task<bool> UpdateEffectAsync(Effect effect);
        Task<bool> DeleteEffectAsync(Guid id);
        Task<IEnumerable<Effect>> GetEffectsByStatusAsync(EffectStatus status);
        Task<IEnumerable<Effect>> GetEffectsBySellerAsync(Guid sellerId);
        Task<bool> TransferToAuctionAsync(Guid effectId);
        Task<bool> MarkAsSoldAsync(Guid effectId, Guid buyerId, decimal soldFor);
    }

    public class EffectMongoDBService : IEffectService
    {
        private readonly ILogger<EffectMongoDBService> _logger;
        private readonly IMongoCollection<Effect> _collection;

        public EffectMongoDBService(ILogger<EffectMongoDBService> logger, MongoDBContext dbContext)
        {
            _logger = logger;
            _collection = dbContext.Collection;
        }

        public async Task<IEnumerable<Effect>> GetAllEffectsAsync()
        {
            _logger.LogInformation("Getting all effects from database");
            var filter = Builders<Effect>.Filter.Empty;

            try
            {
                _logger.LogInformation($"Database: {_collection.Database.DatabaseNamespace.DatabaseName}");
                _logger.LogInformation($"Collection: {_collection.CollectionNamespace.CollectionName}");

                var count = await _collection.CountDocumentsAsync(filter);
                _logger.LogInformation($"Found {count} documents in collection");

                var effects = await _collection.Find(filter).ToListAsync();
                _logger.LogInformation($"Retrieved {effects.Count} effects");
                return effects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve effects");
                return new List<Effect>();
            }
        }

        public async Task<Effect> GetEffectAsync(Guid id)
        {
            var filter = Builders<Effect>.Filter.Eq(x => x.EffectId, id);

            try
            {
                return await _collection.Find(filter).SingleOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve effect with ID {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<Guid> CreateEffectAsync(Effect effect)
        {
            try
            {
                if (effect.EffectId == Guid.Empty)
                {
                    effect.EffectId = Guid.NewGuid();
                }

                effect.EffectStatus = EffectStatus.InStock;
                await _collection.InsertOneAsync(effect);
                _logger.LogInformation($"Created effect with ID {effect.EffectId}");
                return effect.EffectId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create effect: {ex.Message}");
                return Guid.Empty;
            }
        }

        public async Task<bool> UpdateEffectAsync(Effect effect)
        {
            var filter = Builders<Effect>.Filter.Eq(x => x.EffectId, effect.EffectId);

            try
            {
                var result = await _collection.ReplaceOneAsync(filter, effect);
                _logger.LogInformation($"Updated effect with ID {effect.EffectId}. Modified: {result.ModifiedCount}");
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update effect with ID {effect.EffectId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteEffectAsync(Guid id)
        {
            var filter = Builders<Effect>.Filter.Eq(x => x.EffectId, id);

            try
            {
                var result = await _collection.DeleteOneAsync(filter);
                _logger.LogInformation($"Deleted effect with ID {id}. Deleted: {result.DeletedCount}");
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete effect with ID {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<Effect>> GetEffectsByStatusAsync(EffectStatus status)
        {
            var filter = Builders<Effect>.Filter.Eq(x => x.EffectStatus, status);

            try
            {
                return await _collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve effects with status {status}: {ex.Message}");
                return new List<Effect>();
            }
        }

        public async Task<IEnumerable<Effect>> GetEffectsBySellerAsync(Guid sellerId)
        {
            var filter = Builders<Effect>.Filter.Eq(x => x.Seller, sellerId);

            try
            {
                return await _collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve effects for seller {sellerId}: {ex.Message}");
                return new List<Effect>();
            }
        }

        public async Task<bool> TransferToAuctionAsync(Guid effectId)
        {
            var filter = Builders<Effect>.Filter.Eq(x => x.EffectId, effectId);
            var update = Builders<Effect>.Update.Set(x => x.EffectStatus, EffectStatus.OnAuction);

            try
            {
                var result = await _collection.UpdateOneAsync(filter, update);
                _logger.LogInformation($"Transferred effect {effectId} to auction. Modified: {result.ModifiedCount}");
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to transfer effect {effectId} to auction: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MarkAsSoldAsync(Guid effectId, Guid buyerId, decimal soldFor)
        {
            var filter = Builders<Effect>.Filter.Eq(x => x.EffectId, effectId);
            var update = Builders<Effect>.Update
                .Set(x => x.EffectStatus, EffectStatus.Sold)
                .Set(x => x.Buyer, buyerId)
                .Set(x => x.SoldFor, soldFor);

            try
            {
                var result = await _collection.UpdateOneAsync(filter, update);
                _logger.LogInformation($"Marked effect {effectId} as sold to buyer {buyerId} for {soldFor}. Modified: {result.ModifiedCount}");
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to mark effect {effectId} as sold: {ex.Message}");
                return false;
            }
        }
    }
}