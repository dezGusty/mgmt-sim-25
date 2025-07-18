using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.PatchHelper
{
    public static class PatchRequestToEntity
    {
        public static void PatchFrom<TDto, TEntity>(this TEntity entity, TDto dto)
           where TEntity : BaseEntity
           where TDto : class
        {
            var entityType = entity.GetType();
            var dtoType = dto.GetType();

            var dtoProperties = dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in dtoProperties)
            {
                if (!property.CanRead)
                    continue;

                var entityProperty = entityType.GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);

                if (entityProperty == null || !entityProperty.CanWrite)
                    continue;

                var dtoValue = property.GetValue(dto);
                if (!ShouldPatchValue(dtoValue, property.GetType()))
                    continue;

                if (dtoValue != null)
                {
                    var targetType = entityProperty.PropertyType;

                    if (IsNullableType(targetType))
                    {
                        targetType = Nullable.GetUnderlyingType(targetType);
                    }

                    var convertedValue = Convert.ChangeType(dtoValue, targetType);
                    entityProperty.SetValue(entity, convertedValue);
                }
            }
        }

        private static bool ShouldPatchValue(object value, Type propertyType)
        {
            if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                return true;

            if (value == null)
                return false;

            if (value is string str && string.IsNullOrWhiteSpace(str))
                return false;

            if (propertyType.IsValueType && !IsNullableType(propertyType))
            {
                var defaultValue = Activator.CreateInstance(propertyType);
                if (value.Equals(defaultValue))
                    return false;
            }

            return true;
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
