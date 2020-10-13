using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneSolution.Storage.Table.Integration.Tests
{
    public class TestEntity:TableEntity
    {
        public string Name { get; set;}
    }
}
