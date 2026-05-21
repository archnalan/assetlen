using assetlen.Shared.Models.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Service.Extensions
{
    public static class OtherDomainMethods
    {
        public static List<RoleStatusDto> GetRoleStatuses(this UserRolesDto userRoles)
        {
            List<RoleStatusDto> roleStatuses = new List<RoleStatusDto>();

            foreach (PropertyInfo prop in typeof(UserRolesDto).GetProperties())
            {
                if (prop.PropertyType == typeof(bool)) // Ensure only bool properties are considered
                {
                    bool value = (bool)prop.GetValue(userRoles);
                    roleStatuses.Add(new RoleStatusDto
                    {
                        Name = prop.Name,
                        Status = value
                    });
                }
            }

            return roleStatuses;
        }
        public static UserRolesDto GenerateUserRoles(List<string> roleNames)
        {
            UserRolesDto userRoles = new UserRolesDto();

            foreach (PropertyInfo prop in typeof(UserRolesDto).GetProperties())
            {
                if (prop.PropertyType == typeof(bool) && roleNames.Contains(prop.Name))
                {
                    prop.SetValue(userRoles, true);
                }
            }

            return userRoles;
        }
    }
}
