using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.IdentityServer.WebAPI.Db
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("admin")]
        [Required]
        public string admin { get; set; }

        [BsonElement("user")]
        [Required]
        public string user { get; set; }

        [BsonElement("activation_code")]
        [Required]
        public string activation_code { get; set; }

        [BsonElement("status")]
        [Required]
        public string status { get; set; }

        [BsonElement("create_date")]
        [Required]
        public long create_date { get; set; }

        [BsonElement("expire_date")]
        [Required]
        public long expire_date { get; set; }

        public User()
        {

        }
    }
}
