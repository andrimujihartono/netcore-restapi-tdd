using System;
using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Catalog.Api.Repositories;
using Catalog.Api.Entities;
using Catalog.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Catalog.Api.Dtos;
using FluentAssertions;
using System.Linq;
using System.Collections.Generic;


namespace Catalog.UnitTests
{
    public class ItemControllerTests
    {
        private readonly Mock<IItemsRepository> repositoryStub = new();
        private readonly Mock<ILogger<ItemsController>> loggerStub = new();
        private readonly Random rand = new();

        [Fact]
        public async Task GetItemAsync_WithUnexistingItem_ReturnNotFound()
        {
            // Arrange
            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((Item)null);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.GetItemAsync(Guid.NewGuid());

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
            
        }

        [Fact]
        public async Task GetItemAsync_WitheExistingItem_ReturnExpectedItem()
        {
            // Arrange
            var expectedItem = CreateRandomItem();
            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                            .ReturnsAsync(expectedItem);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.GetItemAsync(Guid.NewGuid());

            //Assert
            result.Value.Should().BeEquivalentTo(expectedItem);
        }

        [Fact]
        public async Task GetItemAsync_WitheExistingItem_ReturnsAllItems()
        {
            // Arrange
            var expectedItems = new[] { CreateRandomItem(), CreateRandomItem(), CreateRandomItem() };

            repositoryStub.Setup(repo => repo.GetItemsAsync())
                            .ReturnsAsync(expectedItems);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var actualItems = await controller.GetItemsAsync();


            // Assert
            actualItems.Should().BeEquivalentTo(expectedItems);

        }

        [Fact]
        public async Task GetItemAsync_WitheMatchingItems_ReturnsMatchingItems()
        {
            // Arrange
            var allItems = new[] { 
                new Item(){Name="Potion"},
                new Item(){Name="Ant Man"},
                new Item(){Name="Hi-Potion"}
             };

            var nameToMatch = "Potion";
            repositoryStub.Setup(repo => repo.GetItemsAsync())
                            .ReturnsAsync(allItems);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            IEnumerable<ItemDto> foundsItem = await controller.GetItemsAsync(nameToMatch);


            // Assert
            foundsItem.Should().OnlyContain(
                item => item.Name == allItems[0].Name ||  item.Name == allItems[2].Name
            );

        }

        [Fact]
        public async Task CreateItemAsync_WitheItemToCreate_ReturnsCreatedItems()
        {
            // Arrange
            var itemToCreate = new CreateItemDto(
                Guid.NewGuid().ToString(), 
                Guid.NewGuid().ToString(), 
                rand.Next(1000)
            );

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.CreateItemAsync(itemToCreate);


            // Assert
            var createdItem = (result.Result as CreatedAtActionResult).Value as ItemDto;
            itemToCreate.Should().BeEquivalentTo(
                createdItem,
                options => options.ComparingByMembers<ItemDto>().ExcludingMissingMembers()
            );

            createdItem.Id.Should().NotBeEmpty();
            TimeSpan seconds = new TimeSpan(0,0,1000);
            createdItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, seconds);
        }

        [Fact]
        public async Task UpdateItemAsync_WitheExistingItem_ReturnsNoContents()
        {
            // Arrange
            var existingItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                            .ReturnsAsync(existingItem);

            var itemId = existingItem.Id;
            var itemToUpdate = new UpdateItemDto(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                existingItem.Price + 3
            );
            

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.UpdateItemAsync(itemId, itemToUpdate);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteItemAsync_WitheExistingItem_ReturnsNoContents()
        {
            // Arrange
            var existingItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                            .ReturnsAsync(existingItem);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.DeleteItemAsync(existingItem.Id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        private Item  CreateRandomItem()
        {
            return new(){
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
                Price = rand.Next(1000),
                CreatedDate = DateTimeOffset.UtcNow
            };
        }

       
    }
}