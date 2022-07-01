using Authentication.IdentityServer.WebAPI.Db;
using Authentication.IdentityServer.WebAPI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SigningServer_TedaSign.Services
{

    public class UserService
    {
       
        private readonly IMongoCollection<User> users;
        public UserService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            users = database.GetCollection<User>(settings.UserCollectionName);
        }

        public UserService()
        {
        }

        public List<User> Get()
        {
            return users.Find(user => true).ToList();
        }

        public User FindByUser(string username)
        {
            return users.Find(user => user.user == username).FirstOrDefault();
        }

        public void Create(User User)
        {
            users.InsertOne(User);
        }

        public void Update(string id, User User)
        {
            users.ReplaceOne(user => user.Id == id, User);
        }

        public void Remove(User User)
        {
            users.DeleteOne(User => User.Id == User.Id);
        }

        public void Remove(string id)
        {
            users.DeleteOne(User => User.Id == id);
        }

    }
}
