using Applet.Nat.Api.Static;
using System.Linq.Expressions;
using System.Reflection;

namespace Applet.Nat.Api.Br.Models
{
    public class Filter
    {
        public List<FilterItem> coFilterItems { get; set; }
        public string ivstrDtmFormat;
        public Func<T, bool> Build<T>()
        {
            Expression<Func<T, bool>> lioExpression = null;
            if (coFilterItems.Count == 0)
                return x => true;
            ParameterExpression lioParameterExpression = Expression.Parameter(typeof(T));
            foreach (FilterItem lioFilterItem in coFilterItems)
                lioExpression = FilterItemToFunc(lioFilterItem, lioParameterExpression, lioExpression);
            return lioExpression.Compile();
        }
        private Expression<Func<T, bool>> FilterItemToFunc<T>(FilterItem vioFilterItem, ParameterExpression vioParameterExpression, Expression<Func<T, bool>> vioPrevExpression)
        {
            Expression<Func<T, bool>> func = null;
            PropertyInfo? lioTProperty = typeof(T).GetProperty(vioFilterItem.ivstrPropName);
            if (lioTProperty == null)
                return vioPrevExpression;
            Expression lioLeftSideExpression = Expression.Property(vioParameterExpression, lioTProperty);
            Expression lioRightSideExpression = Expression.Convert(ToExprConstant(lioTProperty, vioFilterItem.ivstrPropValue, vioFilterItem.ivstrOper), lioTProperty.PropertyType);
            Expression<Func<T, bool>> lioItemExpression = Expression.Lambda<Func<T, bool>>(ApplyFilter(vioFilterItem.ivstrOper, lioLeftSideExpression, lioRightSideExpression), vioParameterExpression);
            if (vioPrevExpression != null)
            {
                var invokedExpr = Expression.Invoke(lioItemExpression, lioItemExpression.Parameters.Cast<Expression>());
                return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(vioPrevExpression.Body, invokedExpr), vioPrevExpression.Parameters);
            }
            return lioItemExpression;
        }
        private Expression ToExprConstant(PropertyInfo prop, object value, string vivstrOper)
        {
            object val = null;
            if (prop.Name.StartsWith("ivdtm"))
                val = DateTime.ParseExact(value.ToString(), ivstrDtmFormat, null);
            else
                val = Convert.ChangeType(value, prop.PropertyType);
            return Expression.Constant(val);
        }
        private BinaryExpression ApplyFilter(string vivstrOper, Expression left, Expression right)
        {
            BinaryExpression lioBinaryExpression = null;
            switch (vivstrOper)
            {
                case "EQ":
                    {
                        lioBinaryExpression = Expression.Equal(left, right);
                        break;
                    }
                case "L":
                    {
                        lioBinaryExpression = Expression.LessThan(left, right);
                        break;
                    }
                case "G":
                    {
                        lioBinaryExpression = Expression.GreaterThan(left, right);
                        break;
                    }
                case "GE":
                    {
                        lioBinaryExpression = Expression.GreaterThanOrEqual(left, right);
                        break;
                    }
                case "LE":
                    {
                        lioBinaryExpression = Expression.LessThanOrEqual(left, right);
                        break;
                    }
                case "NE":
                    {
                        lioBinaryExpression = Expression.NotEqual(left, right);
                        break;
                    }
                case "LIKE":
                    {
                        MethodInfo vioMethod = typeof(FilterItem).GetMethod("Like");
                        lioBinaryExpression = Expression.MakeBinary(ExpressionType.Equal, left, right, true, vioMethod);
                        break;
                    }
            }
            return lioBinaryExpression;
        }

    }
    public class FilterItem
    {
        public string ivstrPropName { get; set; }
        public string ivstrPropValue { get; set; }
        public string ivstrOper { get; set; }
        public static bool Like(string vioText, string vioWildcard)
        {
            if (string.IsNullOrEmpty(vioText) || string.IsNullOrEmpty(vioWildcard))
                return false;
            return vioText.Contains(vioWildcard, StringComparison.OrdinalIgnoreCase);
        }
    }
}