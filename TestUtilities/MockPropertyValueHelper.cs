using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace YadaYadaSoftware.TestUtilities;

public static class MockPropertyValueHelper
{
    public static void SetToGuid<TEntity, TDiscriminator>(
        [NotNull] this TEntity entity,
        [NotNull] Expression<Func<TEntity, TDiscriminator>> propertyExpression)
    {
        var propertyInfo = GetPropertyInfo(entity, propertyExpression);
        var maximum = propertyInfo.GetCustomAttribute<MaxLengthAttribute>();
        var value = Guid.NewGuid().ToString().Substring(0, maximum?.Length < Guid.NewGuid().ToString().Length ? maximum.Length : Guid.NewGuid().ToString().Length);
        propertyInfo.GetSetMethod().Invoke(entity, new object[] { value });
    }

    public static PropertyInfo GetPropertyInfo<TSource, TProperty>(
        TSource source,
        Expression<Func<TSource, TProperty>> propertyLambda)
    {
        Type type = typeof(TSource);

        MemberExpression member = propertyLambda.Body as MemberExpression;
        if (member == null)
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a method, not a property.",
                propertyLambda.ToString()));

        PropertyInfo propInfo = member.Member as PropertyInfo;
        if (propInfo == null)
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a field, not a property.",
                propertyLambda.ToString()));

        if (type != propInfo.ReflectedType &&
            !type.IsSubclassOf(propInfo.ReflectedType))
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a property that is not from type {1}.",
                propertyLambda.ToString(),
                type));

        return propInfo;
    }


}