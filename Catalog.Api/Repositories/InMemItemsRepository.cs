using System;
using System.Collections.Generic;
using Catalog.Api.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Api.Repositories
{
   

    public class InMemItemsRepository : IItemsRepository
    {
        private readonly List<Item> items = new()
        {
            new Item { Id = Guid.NewGuid(), Name = "Potion", Price = 10, CreatedDate = DateTimeOffset.UtcNow },
            new Item { Id = Guid.NewGuid(), Name = "CSGO", Price = 9, CreatedDate = DateTimeOffset.UtcNow },
            new Item { Id = Guid.NewGuid(), Name = "Dota 2", Price = 8, CreatedDate = DateTimeOffset.UtcNow }
        };

        public async Task<IEnumerable<Item>> GetItemsAsync()
        {
            return await Task.FromResult(items);
        }

        public  async Task<Item> GetItemAsync(Guid id)
        {
            return await Task.FromResult(items.Where(items => items.Id == id).SingleOrDefault());
        }

        public async Task CreateItemAsync(Item item)
        {
            items.Add(item);
            await Task.CompletedTask;
        }

        public async Task UpdateItemAsync(Item item)
        {
            var index = items.FindIndex(existtingItem => existtingItem.Id == item.Id);
            items[index]= item;
            await Task.CompletedTask;
        }

        public async Task DeleteItemAsync(Guid id)
        {
            var index = items.FindIndex(existingItem =>existingItem.Id == id);
            items.RemoveAt(index);
            await Task.CompletedTask;
        }
    }



}