using Amazon.DynamoDBv2.DataModel;

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
