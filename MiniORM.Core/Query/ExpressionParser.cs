using System.Linq.Expressions;
using System.Reflection;

namespace MiniORM.Core.Query;

/// <summary>
/// Parses LINQ expressions into SQL WHERE clauses.
/// 
/// Design Pattern: Visitor Pattern
/// - Traverses expression tree nodes
/// - Converts each node type to SQL equivalent
/// 
/// Design Pattern: Interpreter Pattern
/// - Interprets expression tree as SQL
/// </summary>
public class ExpressionParser
{
    private SqlQueryBuilder? _queryBuilder;
    
    private static readonly Dictionary<ExpressionType, string> OperatorMap = new()
    {
        { ExpressionType.Equal, "=" },
        { ExpressionType.NotEqual, "<>" },
        { ExpressionType.LessThan, "<" },
        { ExpressionType.LessThanOrEqual, "<=" },
        { ExpressionType.GreaterThan, ">" },
        { ExpressionType.GreaterThanOrEqual, ">=" },
        { ExpressionType.AndAlso, "AND" },
        { ExpressionType.OrElse, "OR" }
    };

    /// <summary>
    /// Parses a predicate expression to SQL WHERE clause.
    /// </summary>
    public string Parse<TEntity>(Expression<Func<TEntity, bool>> predicate, SqlQueryBuilder queryBuilder)
    {
        _queryBuilder = queryBuilder;
        return Visit(predicate.Body);
    }

    private string Visit(Expression expression)
    {
        return expression switch
        {
            BinaryExpression binary => VisitBinary(binary),
            MemberExpression member => VisitMember(member),
            ConstantExpression constant => VisitConstant(constant),
            MethodCallExpression methodCall => VisitMethodCall(methodCall),
            UnaryExpression unary => VisitUnary(unary),
            _ => throw new NotSupportedException($"Expression type '{expression.NodeType}' is not supported.")
        };
    }

    private string VisitBinary(BinaryExpression expression)
    {
        // Handle logical operators (AND, OR)
        if (expression.NodeType == ExpressionType.AndAlso ||
            expression.NodeType == ExpressionType.OrElse)
        {
            var left = Visit(expression.Left);
            var right = Visit(expression.Right);
            var op = OperatorMap[expression.NodeType];
            return $"({left}) {op} ({right})";
        }

        // Handle comparison operators
        if (OperatorMap.TryGetValue(expression.NodeType, out var sqlOp))
        {
            var left = Visit(expression.Left);
            var right = GetValue(expression.Right);
            
            // Handle null comparisons
            if (right == null)
            {
                return expression.NodeType == ExpressionType.Equal
                    ? $"{left} IS NULL"
                    : $"{left} IS NOT NULL";
            }
            
            var paramName = _queryBuilder!.AddParameter(right);
            return $"{left} {sqlOp} {paramName}";
        }

        throw new NotSupportedException($"Binary operator '{expression.NodeType}' is not supported.");
    }

    private string VisitMember(MemberExpression expression)
    {
        // Check if it's a property access on the parameter (e.g., x.Name)
        if (expression.Expression is ParameterExpression)
        {
            // Get the column name from the property
            var propertyInfo = expression.Member as PropertyInfo;
            if (propertyInfo != null)
            {
                var metadata = EntityMapper.GetMetadata(propertyInfo.DeclaringType!);
                var propMeta = metadata.MappedProperties.FirstOrDefault(p => 
                    p.PropertyName == propertyInfo.Name);
                
                return $"[{propMeta?.ColumnName ?? propertyInfo.Name}]";
            }
        }

        // It's a captured variable or property chain, evaluate it
        return EvaluateExpression(expression)?.ToString() ?? "NULL";
    }

    private string VisitConstant(ConstantExpression expression)
    {
        if (expression.Value == null)
        {
            return "NULL";
        }
        var paramName = _queryBuilder!.AddParameter(expression.Value);
        return paramName;
    }

    private string VisitMethodCall(MethodCallExpression expression)
    {
        // Handle string methods
        if (expression.Object != null && expression.Object.Type == typeof(string))
        {
            var column = Visit(expression.Object);
            
            switch (expression.Method.Name)
            {
                case "Contains":
                    var containsValue = EvaluateExpression(expression.Arguments[0]);
                    var containsParam = _queryBuilder!.AddParameter($"%{containsValue}%");
                    return $"{column} LIKE {containsParam}";

                case "StartsWith":
                    var startsValue = EvaluateExpression(expression.Arguments[0]);
                    var startsParam = _queryBuilder!.AddParameter($"{startsValue}%");
                    return $"{column} LIKE {startsParam}";

                case "EndsWith":
                    var endsValue = EvaluateExpression(expression.Arguments[0]);
                    var endsParam = _queryBuilder!.AddParameter($"%{endsValue}");
                    return $"{column} LIKE {endsParam}";

                case "ToUpper":
                    return $"UPPER({column})";

                case "ToLower":
                    return $"LOWER({column})";

                case "Trim":
                    return $"TRIM({column})";
            }
        }

        // Handle Enumerable.Contains for IN clause
        if (expression.Method.Name == "Contains" && 
            expression.Method.DeclaringType == typeof(Enumerable))
        {
            var values = EvaluateExpression(expression.Arguments[0]) as System.Collections.IEnumerable;
            var column = Visit(expression.Arguments[1]);
            
            if (values != null)
            {
                var paramNames = new List<string>();
                foreach (var val in values)
                {
                    paramNames.Add(_queryBuilder!.AddParameter(val));
                }
                return $"{column} IN ({string.Join(", ", paramNames)})";
            }
        }

        throw new NotSupportedException($"Method '{expression.Method.Name}' is not supported.");
    }

    private string VisitUnary(UnaryExpression expression)
    {
        if (expression.NodeType == ExpressionType.Not)
        {
            var operand = Visit(expression.Operand);
            return $"NOT ({operand})";
        }

        if (expression.NodeType == ExpressionType.Convert)
        {
            return Visit(expression.Operand);
        }

        throw new NotSupportedException($"Unary operator '{expression.NodeType}' is not supported.");
    }

    private object? GetValue(Expression expression)
    {
        return EvaluateExpression(expression);
    }

    /// <summary>
    /// Evaluates an expression to get its runtime value.
    /// </summary>
    private object? EvaluateExpression(Expression expression)
    {
        // Compile and invoke to get the actual value
        var lambda = Expression.Lambda(expression);
        var compiled = lambda.Compile();
        return compiled.DynamicInvoke();
    }
}
