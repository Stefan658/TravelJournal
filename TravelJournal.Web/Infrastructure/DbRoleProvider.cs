using System;
using System.Linq;
using System.Reflection;
using System.Web.Security;

using TravelJournal.Data;
using TravelJournal.Data.Context;

namespace TravelJournal.Web.Infrastructure
{
    public class DbRoleProvider : RoleProvider
    {
        public override string[] GetRolesForUser(string username)
        {
            using (var db = new TravelJournalDbContext())
            {
                var user = db.Users.ToList()
                    .FirstOrDefault(u => string.Equals(GetStringProp(u, "Username"), username, StringComparison.OrdinalIgnoreCase));

                if (user == null) return new string[0];

                var role = GetStringProp(user, "Role");
                if (string.IsNullOrWhiteSpace(role)) return new string[0];

                return new[] { role };
            }
        }

        public override bool IsUserInRole(string username, string roleName)
            => GetRolesForUser(username).Any(r => string.Equals(r, roleName, StringComparison.OrdinalIgnoreCase));

        private static string GetStringProp(object obj, string propName)
        {
            if (obj == null) return null;
            var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null) return null;
            var val = prop.GetValue(obj);
            return val?.ToString();
        }

        // Minimal: restul nu ne trebuie
        public override string ApplicationName { get; set; }
        public override void CreateRole(string roleName) => throw new NotImplementedException();
        public override void AddUsersToRoles(string[] usernames, string[] roleNames) => throw new NotImplementedException();
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole) => throw new NotImplementedException();
        public override string[] FindUsersInRole(string roleName, string usernameToMatch) => throw new NotImplementedException();
        public override string[] GetAllRoles() => throw new NotImplementedException();
        public override string[] GetUsersInRole(string roleName) => throw new NotImplementedException();
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames) => throw new NotImplementedException();
        public override bool RoleExists(string roleName) => throw new NotImplementedException();
    }
}
