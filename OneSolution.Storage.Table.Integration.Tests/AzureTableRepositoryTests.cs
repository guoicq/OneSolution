using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos.Table;

namespace OneSolution.Storage.Table.Integration.Tests
{
    public class AzureTableRepositoryTests
    {
        private AzureTableSetting setting;
        private AzureTableRepository<TestEntity> repository;

        public AzureTableRepositoryTests()
        {
            setting = new AzureTableSetting
            {
                AccountName = "nlogiccaeaststoragedev",
                AccountKey = "22j3Wz0WWeBwubee6CFIm6l37X7FNy2yriU6ZSw8ikIq4rO5IQQO3omVXDzXJccl6yy6IfFYDJXDBlLyRwXuOA==",
                TableName = "Test1",
            };
            repository = new AzureTableRepository<TestEntity>(setting);
        }


        [Fact]
        public async Task Should_insert_entity()
        {
            var id = Guid.NewGuid().ToString();
            var entity = new TestEntity { Name = id, PartitionKey = "P1", RowKey = id};
            await repository.InsertOrMerge(entity) ;
            var result = await repository.Get("P1", id);
            result.Should().NotBeNull();
            result.Name.Should().Be(id);
            await repository.Delete(entity);
        }


        [Fact]
        public async Task Should_only_return_attribute_in_list()
        {
            var id = Guid.NewGuid().ToString();
            var entity = new TestEntity { Name = id, PartitionKey = "P1", RowKey = id };
            await repository.InsertOrMerge(entity);
            var result = await repository.Get("P1", id, new List<string>{ "PartitionKey", "RowKey" });
            result.Should().NotBeNull();
            result.RowKey.Should().NotBeNull();
            result.Name.Should().BeNull();
            await repository.Delete(entity);
        }


        [Fact]
        public async Task Should_insert_10_entity()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 0, 10);
            await repository.InsertOrMerge(list);
            var result = await repository.Query(partition);
            result.Should().NotBeNull();
            result.Should().HaveCount(10);
            await repository.Delete(list);
        }

        [Fact]
        public async Task Should_delete_entity()
        {
            var id = Guid.NewGuid().ToString();
            var entity = new TestEntity { Name = id, PartitionKey = "P1", RowKey = id };
            await repository.InsertOrMerge(entity);
            await repository.Delete(entity);
            var result = await repository.Get("P1", id);
            result.Should().BeNull();
        }

        [Fact]
        public async Task Should_delete_list()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 1000, 3000);
            await repository.InsertOrMerge(list);
            await repository.Delete(list);
            var result = await repository.Get(partition, "2999");
            result.Should().BeNull();
        }

        [Fact]
        public async Task Should_delete_filtered_list()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 1000, 3000);
            await repository.InsertOrMerge(list);
            await repository.Delete(partition);
            var result = await repository.Get(partition, "2999");
            result.Should().BeNull();
        }

        [Fact]
        public async Task Should_delete_filtered_list_only()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 1000, 3001);
            await repository.InsertOrMerge(list);
            await repository.Delete(partition, "1000", "3000");
            var result = await repository.Get(partition, "2999");
            result.Should().BeNull();
            var result2 = await repository.Get(partition, "3000");
            result2.Should().NotBeNull(); 
            await repository.Delete(list[2000]);
        }

        [Fact]
        public async Task Should_return_filtered_entities_between_3_and_6()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 0, 9);
            await repository.InsertOrMerge(list);
            var result = await repository.Query(partition, "3","6");
            result.Should().NotBeNull();
            result.Should().HaveCount(3)
                .And.Contain(p => p.RowKey == "3")
                .And.Contain(p => p.RowKey == "4")
                .And.Contain(p => p.RowKey == "5");
            await repository.Delete(list);
        }

        [Fact]
        public async Task Should_return_all_entities_more_than_1000_if_not_passing_count()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 0, 1001);
            await repository.InsertOrMerge(list);
            var result = await repository.Query(partition, "0", "99999");
            result.Should().NotBeNull();
            result.Should().HaveCount(1001);
            await repository.Delete(list);
        }


        [Fact]
        public async Task Should_return_specified_number_of_entities_if_passing_count_less_than_1000()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 0, 1001);
            await repository.InsertOrMerge(list);
            var result = await repository.Query(partition, "0", "99999", 900);
            result.Should().NotBeNull();
            result.Should().HaveCount(900);
            await repository.Delete(list);
        }

        [Fact]
        public async Task Should_return_continuationToken_if_ther_are_more_filtered_items()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 0, 1001);
            await repository.InsertOrMerge(list);
            var result = await repository.QuerySegmented(partition, "0", "99999", 900);
            result.Should().NotBeNull();
            result.Item1.Should().NotBeNull();
            result.Item1.Should().HaveCount(900);
            result.Item2.Should().NotBeNull();

            await repository.Delete(list);
        }

        [Fact]
        public async Task Should_return_continuationToken_null_if_all_filtered_items_returned()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 1000, 3000);
            await repository.InsertOrMerge(list);
            var result = await repository.QuerySegmented(partition, "1000", "2000", 2000);
            result.Should().NotBeNull();
            result.Item1.Should().HaveCount(1000);
            result.Item2.Should().BeNull();

            await repository.Delete(list);
        }

        [Fact]
        public async Task Should_return_continuationToken_if_all_specified_quantity_items_returned_but_there_are_more_filtered_items()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 1000, 3000);
            await repository.InsertOrMerge(list);
            var result = await repository.QuerySegmented(partition, "1000", "3000", 1000);
            result.Should().NotBeNull();
            result.Item1.Should().HaveCount(1000);
            result.Item2.Should().NotBeNull();

            await repository.Delete(list);
        }


        [Fact]
        public async Task Should_return_following_items_if_passing_continuationToken_with_count_1000()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 1000, 2001);
            await repository.InsertOrMerge(list);
            var result = await repository.QuerySegmented(partition, "0", "99999", 1000);
            result.Should().NotBeNull();
            result.Item1.Should().NotBeNull();
            result.Item1.Should().HaveCount(1000);
            result.Item2.Should().NotBeNull();

            result = await repository.QuerySegmented(partition, "0", "99999", 1000, null, result.Item2);
            result.Item1.Should().NotBeNull();
            result.Item1.Should().HaveCount(1)
                .And.ContainSingle(p => p.RowKey == "2000");
            await repository.Delete(list);
        }

        [Fact]
        public async Task Should_return_following_items_if_passing_continuationToken_with_count_less_than_1000()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 1000, 2001);
            await repository.InsertOrMerge(list);
            var result = await repository.QuerySegmented(partition, "0", "99999", 500);
            result.Should().NotBeNull();
            result.Item1.Should().NotBeNull();
            result.Item1.Should().HaveCount(500);
            result.Item2.Should().NotBeNull();

            result = await repository.QuerySegmented(partition, "0", "99999", 500, null, result.Item2);
            result.Item1.Should().NotBeNull();
            result.Item1.Should().HaveCount(500)
                .And.ContainSingle(p => p.RowKey == "1500");
            await repository.Delete(list);
        }



        [Fact]
        public async Task Should_return_following_items_if_passing_continuationToken_with_count_more_than_1000()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 1000, 5000);
            await repository.InsertOrMerge(list);
            var result = await repository.QuerySegmented(partition, "0", "99999", 1100);
            result.Should().NotBeNull();
            result.Item1.Should().NotBeNull();
            result.Item1.Should().HaveCount(1100);
            result.Item2.Should().NotBeNull();

            result = await repository.QuerySegmented(partition, "0", "99999", 1100, null, result.Item2);
            result.Item1.Should().NotBeNull();
            result.Item1.Should().HaveCount(1100)
                .And.ContainSingle(p => p.RowKey == "2100");
            await repository.Delete(list);
        }

        [Fact]
        public async Task Should_return_following_items_if_passing_continuationToken_with_count_more_than_2000()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 1000, 6000);
            await repository.InsertOrMerge(list);
            var result = await repository.QuerySegmented(partition, "0", "99999", 2100);
            result.Should().NotBeNull();
            result.Item1.Should().NotBeNull();
            result.Item1.Should().HaveCount(2100);
            result.Item2.Should().NotBeNull();

            result = await repository.QuerySegmented(partition, "0", "99999", 2100, null, result.Item2);
            result.Item1.Should().NotBeNull();
            result.Item1.Should().HaveCount(2100)
                .And.ContainSingle(p => p.RowKey == "3100");
            await repository.Delete(list);
        }

        [Fact]
        public async Task Should_return_new_filtered_items_if_passing_continuationToken_and_query_changed()
        {
            var partition = Guid.NewGuid().ToString();
            var list = await CreateList(partition, 1000, 4000);
            await repository.InsertOrMerge(list);
            var result = await repository.QuerySegmented(partition, "1000", "2500", 1000);
            result.Should().NotBeNull();
            result.Item1.Should().NotBeNull();
            result.Item1.Should().HaveCount(1000);
            result.Item2.Should().NotBeNull();

            result = await repository.QuerySegmented(partition, "3000", "4000", null, null, result.Item2);
            result.Item1.Should().NotBeNull();
            result.Item1.Should().HaveCount(1000)
                .And.Contain(p => p.RowKey == "3000");
            result.Item2.Should().BeNull();
            await repository.Delete(list);
        }


        private async Task<List<TestEntity>> CreateList(string partition, int start, int end)
        {
            var list = new List<TestEntity>();
            for (var i = start; i < end; i++)
            {
                var id = i.ToString();
                var entity = new TestEntity { Name = id, PartitionKey = partition, RowKey = id };
                list.Add(entity);
            }
            await repository.InsertOrMerge(list);
            return list;
        }

    }

}
