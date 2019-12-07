using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace app.Models
{
    [DynamoDBTable("Users")]
    public class User : IDbEntity
    {
        [DynamoDBHashKey]
        public string Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
    }
}
