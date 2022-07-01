using Authentication.IdentityServer.WebAPI.Db;
using Authentication.IdentityServer.WebAPI.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.IdentityServer.WebAPI.Services
{
    public class AdminService
    {
        private readonly IMongoCollection<Admin> admins;
        public AdminService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            admins = database.GetCollection<Admin>(settings.AdminCollectionName);
        }

        public AdminService()
        {
        }

        public List<Admin> Get()
        {
            return admins.Find(user => true).ToList();
        }

        public bool FindByAdmin(string username, string password)
        {
            List<Admin> admin_users = null;
            admin_users = admins.Find(user => user.usrName == username).ToList();


            if (admin_users.Count == 0)
            {
                return false;
            }
            else
            {
                foreach (Admin admin_user in admin_users)
                {
                    if(admin_user.passwd == password)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public void Create(Admin admin)
        {
            admins.InsertOne(admin);
        }

        public void Update(string id, Admin admin)
        {
            admins.ReplaceOne(user => user.Id == id, admin);
        }

        public void Remove(Admin admin)
        {
            admins.DeleteOne(Admin => Admin.Id == admin.Id);
        }

        public void Remove(string id)
        {
            admins.DeleteOne(Admin => Admin.Id == id);
        }
    }
}
