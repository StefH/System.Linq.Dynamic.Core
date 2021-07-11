using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Dynamic.Core.Exceptions;
using System.Linq.Dynamic.Core.Parser.SupportedMethods;
using System.Linq.Dynamic.Core.Parser.SupportedOperands;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Linq.Dynamic.Core.TypeConverters;
using System.Linq.Dynamic.Core.Validation;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace System.Linq.Dynamic.Core.Parser
{
    /// <summary>
    /// ExpressionParser
    /// </summary>
    public class ExpressionParser
    {
        static readonly string methodOrderBy = nameof(Queryable.OrderBy);
        static readonly string methodOrderByDescending = nameof(Queryable.OrderByDescending);
        static readonly string methodThenBy = nameof(Queryable.ThenBy);
        static readonly string methodThenByDescending = nameof(Queryable.ThenByDescending);

        private readonly ParsingConfig _parsingConfig;
        private readonly MethodFinder _methodFinder;
        private readonly IKeywordsHelper _keywordsHelper;
        private readonly TextParser _textParser;
        private readonly IExpressionHelper _expressionHelper;
        private readonly ITypeFinder _typeFinder;
        private readonly ITypeConverterFactory _typeConverterFactory;
        private readonly Dictionary<string, object> _internals;
        private readonly Dictionary<string, object> _symbols;

        private IDictionary<string, object> _externals;
        private ParameterExpression _it;
        private ParameterExpression _parent;
        private ParameterExpression _root;
        private Type _resultType;
        private bool _createParameterCtor;

        /// <summary>
        /// Gets name for the `it` field. By default this is set to the KeyWord value "it".
        /// </summary>
        public string ItName { get; private set; } = KeywordsHelper.KEYWORD_IT;

        /// <summary>
        /// There was a problem when an expression contained multiple lambdas where
        /// the ItName was not cleared and freed for the next lambda. This variable
        /// stores the ItName of the last parsed lambda.
        /// Not used internally by ExpressionParser, but used to preserve compatiblity of parsingConfig.RenameParameterExpression
        /// which was designed to only work with mono-lambda expressions.
        /// </summary>
        public string LastLambdaItName { get; private set; } = KeywordsHelper.KEYWORD_IT;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionParser"/> class.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="values">The values.</param>
        /// <param name="parsingConfig">The parsing configuration.</param>
        public ExpressionParser([CanBeNull] ParameterExpression[] parameters, [NotNull] string expression, [CanBeNull] object[] values, [CanBeNull] ParsingConfig parsingConfig)
        {
            Check.NotEmpty(expression, nameof(expression));

            _symbols = new Dictionary<string, object>(parsingConfig != null && parsingConfig.IsCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            _internals = new Dictionary<string, object>();

            if (parameters != null)
            {
                ProcessParameters(parameters);
            }

            if (values != null)
            {
                ProcessValues(values);
            }

            _parsingConfig = parsingConfig ?? ParsingConfig.Default;

            _keywordsHelper = new KeywordsHelper(_parsingConfig);
            _textParser = new TextParser(_parsingConfig, expression);
            _methodFinder = new MethodFinder(_parsingConfig);
            _expressionHelper = new ExpressionHelper(_parsingConfig);
            _typeFinder = new TypeFinder(_parsingConfig, _keywordsHelper);
            _typeConverterFactory = new TypeConverterFactory(_parsingConfig);
        }

        void ProcessParameters(ParameterExpression[] parameters)
        {
            foreach (ParameterExpression pe in parameters.Where(p => !string.IsNullOrEmpty(p.Name)))
            {
                AddSymbol(pe.Name, pe);
            }

            // If there is only 1 ParameterExpression, do also allow access using 'it'
            if (parameters.Length == 1)
            {
                _parent = _it;
                _it = parameters[0];

                if (_root == null)
                {
                    _root = _it;
                }
            }
        }

        void ProcessValues(object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                object value = values[i];
                IDictionary<string, object> externals;

                if (i == values.Length - 1 && (externals = value as IDictionary<string, object>) != null)
                {
                    _externals = externals;
                }
                else
                {
                    AddSymbol("@" + i.ToString(CultureInfo.InvariantCulture), value);
                }
            }
        }

        private void AddSymbol(string name, object value)
        {
            if (_symbols.ContainsKey(name))
            {
                throw ParseError(Res.DuplicateIdentifier, name);
            }

            _symbols.Add(name, value);
        }

        /// <summary>
        /// Uses the TextParser to parse the string into the specified result type.
        /// </summary>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="createParameterCtor">if set to <c>true</c> [create parameter ctor].</param>
        /// <returns>Expression</returns>
        public Expression Parse([CanBeNull] Type resultType, bool createParameterCtor = true)
        {
            _resultType = resultType;
            _createParameterCtor = createParameterCtor;

            int exprPos = _textParser.CurrentToken.Pos;
            Expression expr = ParseConditionalOperator();

            if (resultType != null)
            {
                if ((expr = _parsingConfig.ExpressionPromoter.Promote(expr, resultType, true, false)) == null)
                {
                    throw ParseError(exprPos, Res.ExpressionTypeMismatch, TypeHelper.GetTypeName(resultType));
                }
            }

            _textParser.ValidateToken(TokenId.End, Res.SyntaxError);

            return expr;
        }

#pragma warning disable 0219
        internal IList<DynamicOrdering> ParseOrdering(bool forceThenBy = false)
        {
            var orderings = new List<DynamicOrdering>();
            while (true)
            {
                Expression expr = ParseConditionalOperator();
                bool ascending = true;
                if (TokenIdentifierIs("asc") || TokenIdentifierIs("ascending"))
                {
                    _textParser.NextToken();
                }
                else if (TokenIdentifierIs("desc") || TokenIdentifierIs("descending"))
                {
                    _textParser.NextToken();
                    ascending = false;
                }

                string methodName;
                if (forceThenBy || orderings.Count > 0)
                {
                    methodName = ascending ? methodThenBy : methodThenByDescending;
                }
                else
                {
                    methodName = ascending ? methodOrderBy : methodOrderByDescending;
                }

                orderings.Add(new DynamicOrdering { Selector = expr, Ascending = ascending, MethodName = methodName });

                if (_textParser.CurrentToken.Id != TokenId.Comma)
                {
                    break;
                }

                _textParser.NextToken();
            }

            _textParser.ValidateToken(TokenId.End, Res.SyntaxError);
            return orderings;
        }
#pragma warning restore 0219

        // ?: operator
        Expression ParseConditionalOperator()
        {
            int errorPos = _textParser.CurrentToken.Pos;
            Expression expr = ParseNullCoalescingOperator();
            if (_textParser.CurrentToken.Id == TokenId.Question)
            {
                _textParser.NextToken();
                Expression expr1 = ParseConditionalOperator();
                _textParser.ValidateToken(TokenId.Colon, Res.ColonExpected);
                _textParser.NextToken();
                Expression expr2 = ParseConditionalOperator();
                expr = GenerateConditional(expr, expr1, expr2, false, errorPos);
            }
            return expr;
        }

        // ?? (null-coalescing) operator
        Expression ParseNullCoalescingOperator()
        {
            Expression expr = ParseLambdaOperator();
            if (_textParser.CurrentToken.Id == TokenId.NullCoalescing)
            {
                _textParser.NextToken();
                Expression right = ParseConditionalOperator();
                expr = Expression.Coalesce(expr, right);
            }
            return expr;
        }

        // => operator - Added Support for projection operator
        Expression ParseLambdaOperator()
        {
            Expression expr = ParseOrOperator();
            if (_textParser.CurrentToken.Id == TokenId.Lambda && _it.Type == expr.Type)
            {
                _textParser.NextToken();
                if (_textParser.CurrentToken.Id == TokenId.Identifier || _textParser.CurrentToken.Id == TokenId.OpenParen)
                {
                    var right = ParseConditionalOperator();
                    return Expression.Lambda(right, new[] { (ParameterExpression)expr });
                }
                _textParser.ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            }
            return expr;
        }

        // Or operator
        // - ||
        // - Or
        // - OrElse
        Expression ParseOrOperator()
        {
            Expression left = ParseAndOperator();
            while (_textParser.CurrentToken.Id == TokenId.DoubleBar)
            {
                Token op = _textParser.CurrentToken;
                _textParser.NextToken();
                Expression right = ParseAndOperator();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.Id, op.Text, ref left, ref right, op.Pos);
                left = Expression.OrElse(left, right);
            }
            return left;
        }

        // And operator
        // - &&
        // - And
        // - AndAlso
        Expression ParseAndOperator()
        {
            Expression left = ParseIn();
            while (_textParser.CurrentToken.Id == TokenId.DoubleAmphersand)
            {
                Token op = _textParser.CurrentToken;
                _textParser.NextToken();
                Expression right = ParseIn();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.Id, op.Text, ref left, ref right, op.Pos);
                left = Expression.AndAlso(left, right);
            }
            return left;
        }

        // in operator for literals - example: "x in (1,2,3,4)"
        // in operator to mimic contains - example: "x in @0", compare to @0.Contains(x)
        // Adapted from ticket submitted by github user mlewis9548 
        Expression ParseIn()
        {
            Expression left = ParseLogicalAndOrOperator();
            Expression accumulate = left;

            while (TokenIdentifierIs("in"))
            {
                var op = _textParser.CurrentToken;

                _textParser.NextToken();
                if (_textParser.CurrentToken.Id == TokenId.OpenParen) // literals (or other inline list)
                {
                    while (_textParser.CurrentToken.Id != TokenId.CloseParen)
                    {
                        _textParser.NextToken();

                        // we need to parse unary expressions because otherwise 'in' clause will fail in use cases like 'in (-1, -1)' or 'in (!true)'
                        Expression right = ParseUnary();

                        // if the identifier is an Enum, try to convert the right-side also to an Enum.
                        if (left.Type.GetTypeInfo().IsEnum && right is ConstantExpression)
                        {
                            right = ParseEnumToConstantExpression(op.Pos, left.Type, right as ConstantExpression);
                        }

                        // else, check for direct type match
                        else if (left.Type != right.Type)
                        {
                            CheckAndPromoteOperands(typeof(IEqualitySignatures), TokenId.DoubleEqual, "==", ref left, ref right, op.Pos);
                        }

                        if (accumulate.Type != typeof(bool))
                        {
                            accumulate = _expressionHelper.GenerateEqual(left, right);
                        }
                        else
                        {
                            accumulate = Expression.OrElse(accumulate, _expressionHelper.GenerateEqual(left, right));
                        }

                        if (_textParser.CurrentToken.Id == TokenId.End)
                        {
                            throw ParseError(op.Pos, Res.CloseParenOrCommaExpected);
                        }
                    }
                }
                else if (_textParser.CurrentToken.Id == TokenId.Identifier) // a single argument
                {
                    Expression right = ParsePrimary();

                    if (!typeof(IEnumerable).IsAssignableFrom(right.Type))
                    {
                        throw ParseError(_textParser.CurrentToken.Pos, Res.IdentifierImplementingInterfaceExpected, typeof(IEnumerable));
                    }

                    var args = new[] { left };

                    Expression nullExpressionReference = null;
                    if (_methodFinder.FindMethod(typeof(IEnumerableSignatures), nameof(IEnumerableSignatures.Contains), false, ref nullExpressionReference, ref args, out MethodBase containsSignature) != 1)
                    {
                        throw ParseError(op.Pos, Res.NoApplicableAggregate, nameof(IEnumerableSignatures.Contains), string.Join(",", args.Select(a => a.Type.Name).ToArray()));
                    }

                    var typeArgs = new[] { left.Type };

                    args = new[] { right, left };

                    accumulate = Expression.Call(typeof(Enumerable), containsSignature.Name, typeArgs, args);
                }
                else
                {
                    throw ParseError(op.Pos, Res.OpenParenOrIdentifierExpected);
                }

                _textParser.NextToken();
            }

            return accumulate;
        }

        // &, | bitwise operators
        Expression ParseLogicalAndOrOperator()
        {
            Expression left = ParseComparisonOperator();
            while (_textParser.CurrentToken.Id == TokenId.Amphersand || _textParser.CurrentToken.Id == TokenId.Bar)
            {
                Token op = _textParser.CurrentToken;
                _textParser.NextToken();
                Expression right = ParseComparisonOperator();

                if (left.Type.GetTypeInfo().IsEnum)
                {
                    left = Expression.Convert(left, Enum.GetUnderlyingType(left.Type));
                }

                if (right.Type.GetTypeInfo().IsEnum)
                {
                    right = Expression.Convert(right, Enum.GetUnderlyingType(right.Type));
                }

                switch (op.Id)
                {
                    case TokenId.Amphersand:
                        int parseValue;
                        if (left.Type == typeof(string) && left.NodeType == ExpressionType.Constant && int.TryParse((string)((ConstantExpression)left).Value, out parseValue) && TypeHelper.IsNumericType(right.Type))
                        {
                            left = Expression.Constant(parseValue);
                        }
                        else if (right.Type == typeof(string) && right.NodeType == ExpressionType.Constant && int.TryParse((string)((ConstantExpression)right).Value, out parseValue) && TypeHelper.IsNumericType(left.Type))
                        {
                            right = Expression.Constant(parseValue);
                        }

                        // When at least one side of the operator is a string, consider it's a VB-style concatenation operator.
                        // Doesn't break any other function since logical AND with a string is invalid anyway.
                        if (left.Type == typeof(string) || right.Type == typeof(string))
                        {
                            left = _expressionHelper.GenerateStringConcat(left, right);
                        }
                        else
                        {
                            _expressionHelper.ConvertNumericTypeToBiggestCommonTypeForBinaryOperator(ref left, ref right);
                            left = Expression.And(left, right);
                        }
                        break;

                    case TokenId.Bar:
                        _expressionHelper.ConvertNumericTypeToBiggestCommonTypeForBinaryOperator(ref left, ref right);
                        left = Expression.Or(left, right);
                        break;
                }
            }
            return left;
        }

        // =, ==, !=, <>, >, >=, <, <= operators
        Expression ParseComparisonOperator()
        {
            Expression left = ParseShiftOperator();
            while (_textParser.CurrentToken.Id == TokenId.Equal || _textParser.CurrentToken.Id == TokenId.DoubleEqual ||
                   _textParser.CurrentToken.Id == TokenId.ExclamationEqual || _textParser.CurrentToken.Id == TokenId.LessGreater ||
                   _textParser.CurrentToken.Id == TokenId.GreaterThan || _textParser.CurrentToken.Id == TokenId.GreaterThanEqual ||
                   _textParser.CurrentToken.Id == TokenId.LessThan || _textParser.CurrentToken.Id == TokenId.LessThanEqual)
            {
                ConstantExpression constantExpr;
                TypeConverter typeConverter;
                Token op = _textParser.CurrentToken;
                _textParser.NextToken();
                Expression right = ParseShiftOperator();
                bool isEquality = op.Id == TokenId.Equal || op.Id == TokenId.DoubleEqual || op.Id == TokenId.ExclamationEqual || op.Id == TokenId.LessGreater;

                if (isEquality && (!left.Type.GetTypeInfo().IsValueType && !right.Type.GetTypeInfo().IsValueType || left.Type == typeof(Guid) && right.Type == typeof(Guid)))
                {
                    // If left or right is NullLiteral, just continue. Else check if the types differ.
                    if (!(Constants.IsNull(left) || Constants.IsNull(right)) && left.Type != right.Type)
                    {
                        if (left.Type.IsAssignableFrom(right.Type) || HasImplicitConversion(right.Type, left.Type))
                        {
                            right = Expression.Convert(right, left.Type);
                        }
                        else if (right.Type.IsAssignableFrom(left.Type) || HasImplicitConversion(left.Type, right.Type))
                        {
                            left = Expression.Convert(left, right.Type);
                        }
                        else
                        {
                            throw IncompatibleOperandsError(op.Text, left, right, op.Pos);
                        }
                    }
                }
                else if (TypeHelper.IsEnumType(left.Type) || TypeHelper.IsEnumType(right.Type))
                {
                    if (left.Type != right.Type)
                    {
                        Expression e;
                        if ((e = _parsingConfig.ExpressionPromoter.Promote(right, left.Type, true, false)) != null)
                        {
                            right = e;
                        }
                        else if ((e = _parsingConfig.ExpressionPromoter.Promote(left, right.Type, true, false)) != null)
                        {
                            left = e;
                        }
                        else if (TypeHelper.IsEnumType(left.Type) && (constantExpr = right as ConstantExpression) != null)
                        {
                            right = ParseEnumToConstantExpression(op.Pos, left.Type, constantExpr);
                        }
                        else if (TypeHelper.IsEnumType(right.Type) && (constantExpr = left as ConstantExpression) != null)
                        {
                            left = ParseEnumToConstantExpression(op.Pos, right.Type, constantExpr);
                        }
                        else
                        {
                            throw IncompatibleOperandsError(op.Text, left, right, op.Pos);
                        }
                    }
                }
                else if ((constantExpr = right as ConstantExpression) != null && constantExpr.Value is string stringValueR && (typeConverter = _typeConverterFactory.GetConverter(left.Type)) != null)
                {
                    right = Expression.Constant(typeConverter.ConvertFromInvariantString(stringValueR), left.Type);
                }
                else if ((constantExpr = left as ConstantExpression) != null && constantExpr.Value is string stringValueL && (typeConverter = _typeConverterFactory.GetConverter(right.Type)) != null)
                {
                    left = Expression.Constant(typeConverter.ConvertFromInvariantString(stringValueL), right.Type);
                }
                else
                {
                    bool typesAreSameAndImplementCorrectInterface = false;
                    if (left.Type == right.Type)
                    {
                        var interfaces = left.Type.GetInterfaces().Where(x => x.GetTypeInfo().IsGenericType);
                        if (isEquality)
                        {
                            typesAreSameAndImplementCorrectInterface = interfaces.Any(x => x.GetGenericTypeDefinition() == typeof(IEquatable<>));
                        }
                        else
                        {
                            typesAreSameAndImplementCorrectInterface = interfaces.Any(x => x.GetGenericTypeDefinition() == typeof(IComparable<>));
                        }
                    }


                    if (!typesAreSameAndImplementCorrectInterface)
                    {
                        if ((TypeHelper.IsClass(left.Type) || TypeHelper.IsStruct(left.Type)) && right is ConstantExpression)
                        {
                            if (HasImplicitConversion(left.Type, right.Type))
                            {
                                left = Expression.Convert(left, right.Type);
                            }
                            else if (HasImplicitConversion(right.Type, left.Type))
                            {
                                right = Expression.Convert(right, left.Type);
                            }
                        }
                        else if ((TypeHelper.IsClass(right.Type) || TypeHelper.IsStruct(right.Type)) && left is ConstantExpression)
                        {
                            if (HasImplicitConversion(right.Type, left.Type))
                            {
                                right = Expression.Convert(right, left.Type);
                            }
                            else if (HasImplicitConversion(left.Type, right.Type))
                            {
                                left = Expression.Convert(left, right.Type);
                            }
                        }
                        else
                        {
                            CheckAndPromoteOperands(isEquality ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures), op.Id, op.Text, ref left, ref right, op.Pos);
                        }
                    }
                }

                switch (op.Id)
                {
                    case TokenId.Equal:
                    case TokenId.DoubleEqual:
                        left = _expressionHelper.GenerateEqual(left, right);
                        break;
                    case TokenId.ExclamationEqual:
                    case TokenId.LessGreater:
                        left = _expressionHelper.GenerateNotEqual(left, right);
                        break;
                    case TokenId.GreaterThan:
                        left = _expressionHelper.GenerateGreaterThan(left, right);
                        break;
                    case TokenId.GreaterThanEqual:
                        left = _expressionHelper.GenerateGreaterThanEqual(left, right);
                        break;
                    case TokenId.LessThan:
                        left = _expressionHelper.GenerateLessThan(left, right);
                        break;
                    case TokenId.LessThanEqual:
                        left = _expressionHelper.GenerateLessThanEqual(left, right);
                        break;
                }
            }

            return left;
        }

        private bool HasImplicitConversion(Type baseType, Type targetType)
        {
            var baseTypeHasConversion = baseType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(mi => mi.Name == "op_Implicit" && mi.ReturnType == targetType)
                .Any(mi => mi.GetParameters().FirstOrDefault()?.ParameterType == baseType);

            if (baseTypeHasConversion)
            {
                return true;
            }

            return targetType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(mi => mi.Name == "op_Implicit" && mi.ReturnType == targetType)
                .Any(mi => mi.GetParameters().FirstOrDefault()?.ParameterType == baseType);
        }

        private ConstantExpression ParseEnumToConstantExpression(int pos, Type leftType, ConstantExpression constantExpr)
        {
            return Expression.Constant(ParseConstantExpressionToEnum(pos, leftType, constantExpr), leftType);
        }

        private object ParseConstantExpressionToEnum(int pos, Type leftType, ConstantExpression constantExpr)
        {
            try
            {
                if (constantExpr.Value is string stringValue)
                {
                    return Enum.Parse(TypeHelper.GetNonNullableType(leftType), stringValue, true);
                }
            }
            catch
            {
                throw ParseError(pos, Res.ExpressionTypeMismatch, leftType);
            }

            try
            {
                return Enum.ToObject(TypeHelper.GetNonNullableType(leftType), constantExpr.Value);
            }
            catch
            {
                throw ParseError(pos, Res.ExpressionTypeMismatch, leftType);
            }
        }

        // <<, >> operators
        Expression ParseShiftOperator()
        {
            Expression left = ParseAdditive();
            while (_textParser.CurrentToken.Id == TokenId.DoubleLessThan || _textParser.CurrentToken.Id == TokenId.DoubleGreaterThan)
            {
                Token op = _textParser.CurrentToken;
                _textParser.NextToken();
                Expression right = ParseAdditive();
                switch (op.Id)
                {
                    case TokenId.DoubleLessThan:
                        CheckAndPromoteOperands(typeof(IShiftSignatures), op.Id, op.Text, ref left, ref right, op.Pos);
                        left = Expression.LeftShift(left, right);
                        break;
                    case TokenId.DoubleGreaterThan:
                        CheckAndPromoteOperands(typeof(IShiftSignatures), op.Id, op.Text, ref left, ref right, op.Pos);
                        left = Expression.RightShift(left, right);
                        break;
                }
            }
            return left;
        }

        // +, - operators
        Expression ParseAdditive()
        {
            Expression left = ParseMultiplicative();
            while (_textParser.CurrentToken.Id == TokenId.Plus || _textParser.CurrentToken.Id == TokenId.Minus)
            {
                Token op = _textParser.CurrentToken;
                _textParser.NextToken();
                Expression right = ParseMultiplicative();
                switch (op.Id)
                {
                    case TokenId.Plus:
                        if (left.Type == typeof(string) || right.Type == typeof(string))
                        {
                            left = _expressionHelper.GenerateStringConcat(left, right);
                        }
                        else
                        {
                            CheckAndPromoteOperands(typeof(IAddSignatures), op.Id, op.Text, ref left, ref right, op.Pos);
                            left = _expressionHelper.GenerateAdd(left, right);
                        }
                        break;
                    case TokenId.Minus:
                        CheckAndPromoteOperands(typeof(ISubtractSignatures), op.Id, op.Text, ref left, ref right, op.Pos);
                        left = _expressionHelper.GenerateSubtract(left, right);
                        break;
                }
            }
            return left;
        }

        // *, /, %, mod operators
        Expression ParseMultiplicative()
        {
            Expression left = ParseUnary();
            while (_textParser.CurrentToken.Id == TokenId.Asterisk || _textParser.CurrentToken.Id == TokenId.Slash ||
                _textParser.CurrentToken.Id == TokenId.Percent || TokenIdentifierIs("mod"))
            {
                Token op = _textParser.CurrentToken;
                _textParser.NextToken();
                Expression right = ParseUnary();
                CheckAndPromoteOperands(typeof(IArithmeticSignatures), op.Id, op.Text, ref left, ref right, op.Pos);
                switch (op.Id)
                {
                    case TokenId.Asterisk:
                        left = Expression.Multiply(left, right);
                        break;
                    case TokenId.Slash:
                        left = Expression.Divide(left, right);
                        break;
                    case TokenId.Percent:
                    case TokenId.Identifier:
                        left = Expression.Modulo(left, right);
                        break;
                }
            }
            return left;
        }

        // -, !, not unary operators
        Expression ParseUnary()
        {
            if (_textParser.CurrentToken.Id == TokenId.Minus || _textParser.CurrentToken.Id == TokenId.Exclamation || TokenIdentifierIs("not"))
            {
                Token op = _textParser.CurrentToken;
                _textParser.NextToken();
                if (op.Id == TokenId.Minus && (_textParser.CurrentToken.Id == TokenId.IntegerLiteral || _textParser.CurrentToken.Id == TokenId.RealLiteral))
                {
                    _textParser.CurrentToken.Text = "-" + _textParser.CurrentToken.Text;
                    _textParser.CurrentToken.Pos = op.Pos;
                    return ParsePrimary();
                }

                Expression expr = ParseUnary();
                if (op.Id == TokenId.Minus)
                {
                    CheckAndPromoteOperand(typeof(INegationSignatures), op.Text, ref expr, op.Pos);
                    expr = Expression.Negate(expr);
                }
                else
                {
                    CheckAndPromoteOperand(typeof(INotSignatures), op.Text, ref expr, op.Pos);
                    expr = Expression.Not(expr);
                }

                return expr;
            }

            return ParsePrimary();
        }

        Expression ParsePrimary()
        {
            Expression expr = ParsePrimaryStart();
            _expressionHelper.WrapConstantExpression(ref expr);

            while (true)
            {
                if (_textParser.CurrentToken.Id == TokenId.Dot)
                {
                    _textParser.NextToken();
                    expr = ParseMemberAccess(null, expr);
                }
                else if (_textParser.CurrentToken.Id == TokenId.NullPropagation)
                {
                    throw new NotSupportedException("An expression tree lambda may not contain a null propagating operator. Use the 'np()' or 'np(...)' (null-propagation) function instead.");
                }
                else if (_textParser.CurrentToken.Id == TokenId.OpenBracket)
                {
                    expr = ParseElementAccess(expr);
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        Expression ParsePrimaryStart()
        {
            switch (_textParser.CurrentToken.Id)
            {
                case TokenId.Identifier:
                    return ParseIdentifier();
                case TokenId.StringLiteral:
                    return ParseStringLiteral();
                case TokenId.IntegerLiteral:
                    return ParseIntegerLiteral();
                case TokenId.RealLiteral:
                    return ParseRealLiteral();
                case TokenId.OpenParen:
                    return ParseParenExpression();
                default:
                    throw ParseError(Res.ExpressionExpected);
            }
        }

        Expression ParseStringLiteral()
        {
            _textParser.ValidateToken(TokenId.StringLiteral);

            string result = StringParser.ParseString(_textParser.CurrentToken.Text);

            if (_textParser.CurrentToken.Text[0] == '\'')
            {
                if (result.Length > 1)
                {
                    throw ParseError(Res.InvalidCharacterLiteral);
                }

                _textParser.NextToken();
                return ConstantExpressionHelper.CreateLiteral(result[0], result);
            }

            _textParser.NextToken();
            return ConstantExpressionHelper.CreateLiteral(result, result);
        }

        Expression ParseIntegerLiteral()
        {
            _textParser.ValidateToken(TokenId.IntegerLiteral);

            string text = _textParser.CurrentToken.Text;
            string qualifier = null;
            char last = text[text.Length - 1];
            bool isHexadecimal = text.StartsWith(text[0] == '-' ? "-0x" : "0x", StringComparison.OrdinalIgnoreCase);
            char[] qualifierLetters = isHexadecimal
                                          ? new[] { 'U', 'u', 'L', 'l' }
                                          : new[] { 'U', 'u', 'L', 'l', 'F', 'f', 'D', 'd', 'M', 'm' };

            if (qualifierLetters.Contains(last))
            {
                int pos = text.Length - 1, count = 0;
                while (qualifierLetters.Contains(text[pos]))
                {
                    ++count;
                    --pos;
                }
                qualifier = text.Substring(text.Length - count, count);
                text = text.Substring(0, text.Length - count);
            }

            if (text[0] != '-')
            {
                if (isHexadecimal)
                {
                    text = text.Substring(2);
                }

                if (!ulong.TryParse(text, isHexadecimal ? NumberStyles.HexNumber : NumberStyles.Integer, _parsingConfig.NumberParseCulture, out ulong value))
                {
                    throw ParseError(Res.InvalidIntegerLiteral, text);
                }

                _textParser.NextToken();
                if (!string.IsNullOrEmpty(qualifier))
                {
                    if (qualifier == "U" || qualifier == "u") return ConstantExpressionHelper.CreateLiteral((uint)value, text);
                    if (qualifier == "L" || qualifier == "l") return ConstantExpressionHelper.CreateLiteral((long)value, text);

                    // in case of UL, just return
                    return ConstantExpressionHelper.CreateLiteral(value, text);
                }

                // if (value <= (int)short.MaxValue) return ConstantExpressionHelper.CreateLiteral((short)value, text);
                if (value <= int.MaxValue) return ConstantExpressionHelper.CreateLiteral((int)value, text);
                if (value <= uint.MaxValue) return ConstantExpressionHelper.CreateLiteral((uint)value, text);
                if (value <= long.MaxValue) return ConstantExpressionHelper.CreateLiteral((long)value, text);

                return ConstantExpressionHelper.CreateLiteral(value, text);
            }
            else
            {
                if (isHexadecimal)
                {
                    text = text.Substring(3);
                }

                if (!long.TryParse(text, isHexadecimal ? NumberStyles.HexNumber : NumberStyles.Integer, _parsingConfig.NumberParseCulture, out long value))
                {
                    throw ParseError(Res.InvalidIntegerLiteral, text);
                }

                if (isHexadecimal)
                {
                    value = -value;
                }

                _textParser.NextToken();
                if (!string.IsNullOrEmpty(qualifier))
                {
                    if (qualifier == "L" || qualifier == "l")
                        return ConstantExpressionHelper.CreateLiteral(value, text);

                    if (qualifier == "F" || qualifier == "f")
                        return TryParseAsFloat(text, qualifier[0]);

                    if (qualifier == "D" || qualifier == "d")
                        return TryParseAsDouble(text, qualifier[0]);

                    if (qualifier == "M" || qualifier == "m")
                        return TryParseAsDecimal(text, qualifier[0]);

                    throw ParseError(Res.MinusCannotBeAppliedToUnsignedInteger);
                }

                if (value <= int.MaxValue)
                {
                    return ConstantExpressionHelper.CreateLiteral((int)value, text);
                }

                return ConstantExpressionHelper.CreateLiteral(value, text);
            }
        }

        Expression ParseRealLiteral()
        {
            _textParser.ValidateToken(TokenId.RealLiteral);

            string text = _textParser.CurrentToken.Text;
            char qualifier = text[text.Length - 1];

            _textParser.NextToken();
            return TryParseAsFloat(text, qualifier);
        }

        Expression TryParseAsFloat(string text, char qualifier)
        {
            if (qualifier == 'F' || qualifier == 'f')
            {
                if (float.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Float, _parsingConfig.NumberParseCulture, out float f))
                {
                    return ConstantExpressionHelper.CreateLiteral(f, text);
                }
            }

            // not possible to find float qualifier, so try to parse as double
            return TryParseAsDecimal(text, qualifier);
        }

        Expression TryParseAsDecimal(string text, char qualifier)
        {
            if (qualifier == 'M' || qualifier == 'm')
            {
                if (decimal.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Number, _parsingConfig.NumberParseCulture, out decimal d))
                {
                    return ConstantExpressionHelper.CreateLiteral(d, text);
                }
            }

            // not possible to find float qualifier, so try to parse as double
            return TryParseAsDouble(text, qualifier);
        }

        Expression TryParseAsDouble(string text, char qualifier)
        {
            double d;
            if (qualifier == 'D' || qualifier == 'd')
            {
                if (double.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Number, _parsingConfig.NumberParseCulture, out d))
                {
                    return ConstantExpressionHelper.CreateLiteral(d, text);
                }
            }

            if (double.TryParse(text, NumberStyles.Number, _parsingConfig.NumberParseCulture, out d))
            {
                return ConstantExpressionHelper.CreateLiteral(d, text);
            }

            throw ParseError(Res.InvalidRealLiteral, text);
        }

        Expression ParseParenExpression()
        {
            _textParser.ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            _textParser.NextToken();
            Expression e = ParseConditionalOperator();
            _textParser.ValidateToken(TokenId.CloseParen, Res.CloseParenOrOperatorExpected);
            _textParser.NextToken();
            return e;
        }

        Expression ParseIdentifier()
        {
            _textParser.ValidateToken(TokenId.Identifier);

            if (_keywordsHelper.TryGetValue(_textParser.CurrentToken.Text, out object value) &&
                // Prioritize property or field over the type
                !(value is Type && _it != null && FindPropertyOrField(_it.Type, _textParser.CurrentToken.Text, false) != null))
            {
                Type typeValue = value as Type;
                if (typeValue != null)
                {
                    return ParseTypeAccess(typeValue);
                }

                switch (value)
                {
                    case KeywordsHelper.KEYWORD_IT:
                    case KeywordsHelper.SYMBOL_IT:
                        return ParseIt();

                    case KeywordsHelper.KEYWORD_PARENT:
                    case KeywordsHelper.SYMBOL_PARENT:
                        return ParseParent();

                    case KeywordsHelper.KEYWORD_ROOT:
                    case KeywordsHelper.SYMBOL_ROOT:
                        return ParseRoot();

                    case KeywordsHelper.FUNCTION_IIF:
                        return ParseFunctionIif();

                    case KeywordsHelper.FUNCTION_ISNULL:
                        return ParseFunctionIsNull();

                    case KeywordsHelper.FUNCTION_NEW:
                        return ParseNew();

                    case KeywordsHelper.FUNCTION_NULLPROPAGATION:
                        return ParseFunctionNullPropagation();

                    case KeywordsHelper.FUNCTION_IS:
                        return ParseFunctionIs();

                    case KeywordsHelper.FUNCTION_AS:
                        return ParseFunctionAs();

                    case KeywordsHelper.FUNCTION_CAST:
                        return ParseFunctionCast();
                }

                _textParser.NextToken();

                return (Expression)value;
            }

            if (_symbols.TryGetValue(_textParser.CurrentToken.Text, out value) ||
                _externals != null && _externals.TryGetValue(_textParser.CurrentToken.Text, out value) ||
                _internals.TryGetValue(_textParser.CurrentToken.Text, out value))
            {
                var expr = value as Expression;
                if (expr == null)
                {
                    expr = Expression.Constant(value);
                }
                else
                {
                    var lambda = expr as LambdaExpression;
                    if (lambda != null)
                    {
                        return ParseLambdaInvocation(lambda);
                    }
                }

                _textParser.NextToken();

                return expr;
            }

            //// This could be enum like "MyEnum.Value1"
            //if (_textParser.CurrentToken.Id == TokenId.Identifier)
            //{
            //    var parts = new List<string> { _textParser.CurrentToken.Text };

            //    _textParser.NextToken();
            //    _textParser.ValidateToken(TokenId.Dot, Res.DotExpected);
            //    while (_textParser.CurrentToken.Id == TokenId.Dot || _textParser.CurrentToken.Id == TokenId.Plus)
            //    {
            //        parts.Add(_textParser.CurrentToken.Text);

            //        _textParser.NextToken();
            //        _textParser.ValidateToken(TokenId.Identifier, Res.IdentifierExpected);

            //        parts.Add(_textParser.CurrentToken.Text);

            //        _textParser.NextToken();
            //    }

            //    var enumTypeAsString = string.Join("", parts.Take(parts.Count - 2).ToArray());
            //    var enumType = _typeFinder.FindTypeByName(enumTypeAsString, null, true);
            //    if (enumType == null)
            //    {
            //        throw ParseError(_textParser.CurrentToken.Pos, Res.EnumTypeNotFound, enumTypeAsString);
            //    }

            //    string enumValue = parts.Last();
            //    var @enum = TypeHelper.ParseEnum(enumValue, enumType);
            //    if (@enum == null)
            //    {
            //        throw ParseError(_textParser.CurrentToken.Pos, Res.EnumValueNotDefined, enumValue, enumTypeAsString);
            //    }

            //    return Expression.Constant(@enum);
            //}

            if (_it != null)
            {
                return ParseMemberAccess(null, _it);
            }

            throw ParseError(Res.UnknownIdentifier, _textParser.CurrentToken.Text);
        }

        Expression ParseIt()
        {
            if (_it == null)
            {
                throw ParseError(Res.NoItInScope);
            }
            _textParser.NextToken();
            return _it;
        }

        Expression ParseParent()
        {
            if (_parent == null)
            {
                throw ParseError(Res.NoParentInScope);
            }
            _textParser.NextToken();
            return _parent;
        }

        Expression ParseRoot()
        {
            if (_root == null)
            {
                throw ParseError(Res.NoRootInScope);
            }
            _textParser.NextToken();
            return _root;
        }

        // isnull(a,b) function
        Expression ParseFunctionIsNull()
        {
            int errorPos = _textParser.CurrentToken.Pos;
            _textParser.NextToken();
            Expression[] args = ParseArgumentList();
            if (args.Length != 2)
            {
                throw ParseError(errorPos, Res.IsNullRequiresTwoArgs);
            }

            return Expression.Coalesce(args[0], args[1]);
        }

        // iif(test, ifTrue, ifFalse) function
        Expression ParseFunctionIif()
        {
            int errorPos = _textParser.CurrentToken.Pos;
            _textParser.NextToken();

            Expression[] args = ParseArgumentList();
            if (args.Length != 3)
            {
                throw ParseError(errorPos, Res.IifRequiresThreeArgs);
            }

            return GenerateConditional(args[0], args[1], args[2], false, errorPos);
        }

        // np(...) function
        Expression ParseFunctionNullPropagation()
        {
            int errorPos = _textParser.CurrentToken.Pos;
            _textParser.NextToken();

            Expression[] args = ParseArgumentList();

            if (args.Length != 1 && args.Length != 2)
            {
                throw ParseError(errorPos, Res.NullPropagationRequiresCorrectArgs);
            }

            if (_expressionHelper.ExpressionQualifiesForNullPropagation(args[0]))
            {
                bool hasDefaultParameter = args.Length == 2;
                Expression expressionIfFalse = hasDefaultParameter ? args[1] : Expression.Constant(null);

                if (_expressionHelper.TryGenerateAndAlsoNotNullExpression(args[0], true, out Expression generatedExpression))
                {
                    return GenerateConditional(generatedExpression, args[0], expressionIfFalse, true, errorPos);
                }

                return args[0];
            }

            throw ParseError(errorPos, Res.NullPropagationRequiresValidExpression);
        }

        // Is(...) function
        Expression ParseFunctionIs()
        {
            int errorPos = _textParser.CurrentToken.Pos;
            string functionName = _textParser.CurrentToken.Text;
            _textParser.NextToken();

            Expression[] args = ParseArgumentList();

            if (args.Length != 1)
            {
                throw ParseError(errorPos, Res.FunctionRequiresOneArg, functionName);
            }

            Type resolvedType = ResolveTypeFromArgumentExpression(functionName, args[0]);

            return Expression.TypeIs(_it, resolvedType);
        }

        // As(...) function
        Expression ParseFunctionAs()
        {
            int errorPos = _textParser.CurrentToken.Pos;
            string functionName = _textParser.CurrentToken.Text;
            _textParser.NextToken();

            Expression[] args = ParseArgumentList();

            if (args.Length != 1)
            {
                throw ParseError(errorPos, Res.FunctionRequiresOneArg, functionName);
            }

            Type resolvedType = ResolveTypeFromArgumentExpression(functionName, args[0]);

            return Expression.TypeAs(_it, resolvedType);
        }

        // Cast(...) function
        Expression ParseFunctionCast()
        {
            int errorPos = _textParser.CurrentToken.Pos;
            string functionName = _textParser.CurrentToken.Text;
            _textParser.NextToken();

            Expression[] args = ParseArgumentList();

            if (args.Length != 1)
            {
                throw ParseError(errorPos, Res.FunctionRequiresOneArg, functionName);
            }

            Type resolvedType = ResolveTypeFromArgumentExpression(functionName, args[0]);

            return Expression.ConvertChecked(_it, resolvedType);
        }

        Expression GenerateConditional(Expression test, Expression expressionIfTrue, Expression expressionIfFalse, bool nullPropagating, int errorPos)
        {
            if (test.Type != typeof(bool))
            {
                throw ParseError(errorPos, Res.FirstExprMustBeBool);
            }

            if (expressionIfTrue.Type != expressionIfFalse.Type)
            {
                // If expressionIfTrue is a null constant and expressionIfFalse is ValueType:
                if (Constants.IsNull(expressionIfTrue) && expressionIfFalse.Type.GetTypeInfo().IsValueType)
                {
                    if (nullPropagating && _parsingConfig.NullPropagatingUseDefaultValueForNonNullableValueTypes)
                    {
                        // If expressionIfFalse is a non-nullable type:
                        //  generate default expression from the expressionIfFalse-type for expressionIfTrue
                        // Else
                        //  create nullable constant from expressionIfTrue with type from expressionIfFalse

                        if (!TypeHelper.IsNullableType(expressionIfFalse.Type))
                        {
                            expressionIfTrue = _expressionHelper.GenerateDefaultExpression(expressionIfFalse.Type);
                        }
                        else
                        {
                            expressionIfTrue = Expression.Constant(null, expressionIfFalse.Type);
                        }
                    }
                    else
                    {
                        // - create nullable constant from expressionIfTrue with type from expressionIfFalse
                        // - convert expressionIfFalse to nullable (unless it's already nullable)

                        Type nullableType = TypeHelper.ToNullableType(expressionIfFalse.Type);
                        expressionIfTrue = Expression.Constant(null, nullableType);

                        if (!TypeHelper.IsNullableType(expressionIfFalse.Type))
                        {
                            expressionIfFalse = Expression.Convert(expressionIfFalse, nullableType);
                        }
                    }

                    return Expression.Condition(test, expressionIfTrue, expressionIfFalse);
                }

                // If expressionIfFalse is a null constant and expressionIfTrue is a ValueType:
                if (Constants.IsNull(expressionIfFalse) && expressionIfTrue.Type.GetTypeInfo().IsValueType)
                {
                    if (nullPropagating && _parsingConfig.NullPropagatingUseDefaultValueForNonNullableValueTypes)
                    {
                        // If expressionIfTrue is a non-nullable type:
                        //  generate default expression from the expressionIfFalse-type for expressionIfFalse
                        // Else
                        //  create nullable constant from expressionIfFalse with type from expressionIfTrue

                        if (!TypeHelper.IsNullableType(expressionIfTrue.Type))
                        {
                            expressionIfFalse = _expressionHelper.GenerateDefaultExpression(expressionIfTrue.Type);
                        }
                        else
                        {
                            expressionIfFalse = Expression.Constant(null, expressionIfTrue.Type);
                        }
                    }
                    else
                    {
                        // - create nullable constant from expressionIfFalse with type from expressionIfTrue
                        // - convert expressionIfTrue to nullable (unless it's already nullable)

                        Type nullableType = TypeHelper.ToNullableType(expressionIfTrue.Type);
                        expressionIfFalse = Expression.Constant(null, nullableType);
                        if (!TypeHelper.IsNullableType(expressionIfTrue.Type))
                        {
                            expressionIfTrue = Expression.Convert(expressionIfTrue, nullableType);
                        }
                    }

                    return Expression.Condition(test, expressionIfTrue, expressionIfFalse);
                }

                Expression expr1As2 = !Constants.IsNull(expressionIfFalse) ? _parsingConfig.ExpressionPromoter.Promote(expressionIfTrue, expressionIfFalse.Type, true, false) : null;
                Expression expr2As1 = !Constants.IsNull(expressionIfTrue) ? _parsingConfig.ExpressionPromoter.Promote(expressionIfFalse, expressionIfTrue.Type, true, false) : null;
                if (expr1As2 != null && expr2As1 == null)
                {
                    expressionIfTrue = expr1As2;
                }
                else if (expr2As1 != null && expr1As2 == null)
                {
                    expressionIfFalse = expr2As1;
                }
                else
                {
                    string type1 = !Constants.IsNull(expressionIfTrue) ? expressionIfTrue.Type.Name : "null";
                    string type2 = !Constants.IsNull(expressionIfFalse) ? expressionIfFalse.Type.Name : "null";
                    if (expr1As2 != null)
                    {
                        throw ParseError(errorPos, Res.BothTypesConvertToOther, type1, type2);
                    }

                    throw ParseError(errorPos, Res.NeitherTypeConvertsToOther, type1, type2);
                }
            }

            return Expression.Condition(test, expressionIfTrue, expressionIfFalse);
        }

        // new (...) function
        Expression ParseNew()
        {
            _textParser.NextToken();
            if (_textParser.CurrentToken.Id != TokenId.OpenParen &&
                _textParser.CurrentToken.Id != TokenId.OpenCurlyParen &&
                _textParser.CurrentToken.Id != TokenId.OpenBracket &&
                _textParser.CurrentToken.Id != TokenId.Identifier)
            {
                throw ParseError(Res.OpenParenOrIdentifierExpected);
            }

            Type newType = null;
            if (_textParser.CurrentToken.Id == TokenId.Identifier)
            {
                var newTypeName = _textParser.CurrentToken.Text;
                _textParser.NextToken();

                while (_textParser.CurrentToken.Id == TokenId.Dot || _textParser.CurrentToken.Id == TokenId.Plus)
                {
                    var sep = _textParser.CurrentToken.Text;
                    _textParser.NextToken();
                    if (_textParser.CurrentToken.Id != TokenId.Identifier)
                    {
                        throw ParseError(Res.IdentifierExpected);
                    }
                    newTypeName += sep + _textParser.CurrentToken.Text;
                    _textParser.NextToken();
                }

                newType = _typeFinder.FindTypeByName(newTypeName, new[] { _it, _parent, _root }, false);
                if (newType == null)
                {
                    throw ParseError(_textParser.CurrentToken.Pos, Res.TypeNotFound, newTypeName);
                }

                if (_textParser.CurrentToken.Id != TokenId.OpenParen &&
                    _textParser.CurrentToken.Id != TokenId.OpenBracket &&
                    _textParser.CurrentToken.Id != TokenId.OpenCurlyParen)
                {
                    throw ParseError(Res.OpenParenExpected);
                }
            }

            bool arrayInitializer = false;
            if (_textParser.CurrentToken.Id == TokenId.OpenBracket)
            {
                _textParser.NextToken();
                _textParser.ValidateToken(TokenId.CloseBracket, Res.CloseBracketExpected);
                _textParser.NextToken();
                _textParser.ValidateToken(TokenId.OpenCurlyParen, Res.OpenCurlyParenExpected);
                arrayInitializer = true;
            }

            _textParser.NextToken();

            var properties = new List<DynamicProperty>();
            var expressions = new List<Expression>();

            while (_textParser.CurrentToken.Id != TokenId.CloseParen && _textParser.CurrentToken.Id != TokenId.CloseCurlyParen)
            {
                int exprPos = _textParser.CurrentToken.Pos;
                Expression expr = ParseConditionalOperator();
                if (!arrayInitializer)
                {
                    string propName;
                    if (TokenIdentifierIs("as"))
                    {
                        _textParser.NextToken();
                        propName = GetIdentifier();
                        _textParser.NextToken();
                    }
                    else
                    {
                        if (!TryGetMemberName(expr, out propName))
                        {
                            if (expr is MethodCallExpression methodCallExpression
                                && methodCallExpression.Arguments.Count == 1
                                && methodCallExpression.Arguments[0] is ConstantExpression methodCallExpressionArgument
                                && methodCallExpressionArgument.Type == typeof(string)
                                && properties.All(x => x.Name != (string)methodCallExpressionArgument.Value))
                            {
                                propName = (string)methodCallExpressionArgument.Value;
                            }
                            else
                            {
                                throw ParseError(exprPos, Res.MissingAsClause);
                            }
                        }
                    }

                    properties.Add(new DynamicProperty(propName, expr.Type));
                }

                expressions.Add(expr);

                if (_textParser.CurrentToken.Id != TokenId.Comma)
                {
                    break;
                }

                _textParser.NextToken();
            }

            if (_textParser.CurrentToken.Id != TokenId.CloseParen && _textParser.CurrentToken.Id != TokenId.CloseCurlyParen)
            {
                throw ParseError(Res.CloseParenOrCommaExpected);
            }
            _textParser.NextToken();

            if (arrayInitializer)
            {
                return CreateArrayInitializerExpression(expressions, newType);
            }

            return CreateNewExpression(properties, expressions, newType);
        }

        private Expression CreateArrayInitializerExpression(List<Expression> expressions, Type newType)
        {
            if (expressions.Count == 0)
            {
                return Expression.NewArrayInit(newType ?? typeof(object));
            }

            if (newType != null)
            {
                return Expression.NewArrayInit(newType, expressions.Select(expression => _parsingConfig.ExpressionPromoter.Promote(expression, newType, true, true)));
            }

            return Expression.NewArrayInit(expressions.All(expression => expression.Type == expressions[0].Type) ? expressions[0].Type : typeof(object), expressions);
        }

        private Expression CreateNewExpression(List<DynamicProperty> properties, List<Expression> expressions, Type newType)
        {
            // http://solutionizing.net/category/linq/
            Type type = newType ?? _resultType;

            if (type == null)
            {
#if UAP10_0
                type = typeof(DynamicClass);
                Type typeForKeyValuePair = typeof(KeyValuePair<string, object>);

                ConstructorInfo constructorForKeyValuePair = typeForKeyValuePair.GetTypeInfo().DeclaredConstructors.First();

                var arrayIndexParams = new List<Expression>();
                for (int i = 0; i < expressions.Count; i++)
                {
                    // Just convert the expression always to an object expression.
                    UnaryExpression boxingExpression = Expression.Convert(expressions[i], typeof(object));
                    NewExpression parameter = Expression.New(constructorForKeyValuePair, (Expression)Expression.Constant(properties[i].Name), boxingExpression);

                    arrayIndexParams.Add(parameter);
                }

                // Create an expression tree that represents creating and initializing a one-dimensional array of type KeyValuePair<string, object>.
                NewArrayExpression newArrayExpression = Expression.NewArrayInit(typeof(KeyValuePair<string, object>), arrayIndexParams);

                // Get the "public DynamicClass(KeyValuePair<string, object>[] propertylist)" constructor
                ConstructorInfo constructor = type.GetTypeInfo().DeclaredConstructors.First();

                return Expression.New(constructor, newArrayExpression);
#else
                type = DynamicClassFactory.CreateType(properties, _createParameterCtor);
#endif
            }

            IEnumerable<PropertyInfo> propertyInfos = type.GetProperties();
            if (type.GetTypeInfo().BaseType == typeof(DynamicClass))
            {
                propertyInfos = propertyInfos.Where(x => x.Name != "Item");
            }

            Type[] propertyTypes = propertyInfos.Select(p => p.PropertyType).ToArray();
            ConstructorInfo ctor = type.GetConstructor(propertyTypes);
            if (ctor != null && ctor.GetParameters().Length == expressions.Count)
            {
                var expressionsPromoted = new List<Expression>();

                // Loop all expressions and promote if needed
                for (int i = 0; i < propertyTypes.Length; i++)
                {
                    Type propertyType = propertyTypes[i];

                    // Promote from Type to Nullable Type if needed
                    expressionsPromoted.Add(_parsingConfig.ExpressionPromoter.Promote(expressions[i], propertyType, true, true));
                }

                return Expression.New(ctor, expressionsPromoted, (IEnumerable<MemberInfo>)propertyInfos);
            }

            MemberBinding[] bindings = new MemberBinding[properties.Count];
            for (int i = 0; i < bindings.Length; i++)
            {
                string propertyOrFieldName = properties[i].Name;
                Type propertyOrFieldType;
                MemberInfo memberInfo;
                PropertyInfo propertyInfo = type.GetProperty(propertyOrFieldName);
                if (propertyInfo != null)
                {
                    memberInfo = propertyInfo;
                    propertyOrFieldType = propertyInfo.PropertyType;
                }
                else
                {
                    FieldInfo fieldInfo = type.GetField(propertyOrFieldName);
                    if (fieldInfo == null)
                    {
                        throw ParseError(Res.UnknownPropertyOrField, propertyOrFieldName, TypeHelper.GetTypeName(type));
                    }

                    memberInfo = fieldInfo;
                    propertyOrFieldType = fieldInfo.FieldType;
                }

                // Promote from Type to Nullable Type if needed
                bindings[i] = Expression.Bind(memberInfo, _parsingConfig.ExpressionPromoter.Promote(expressions[i], propertyOrFieldType, true, true));
            }

            return Expression.MemberInit(Expression.New(type), bindings);
        }

        Expression ParseLambdaInvocation(LambdaExpression lambda)
        {
            int errorPos = _textParser.CurrentToken.Pos;
            _textParser.NextToken();
            Expression[] args = ParseArgumentList();

            Expression nullExpressionReference = null;
            if (_methodFinder.FindMethod(lambda.Type, nameof(Expression.Invoke), false, ref nullExpressionReference, ref args, out MethodBase _) != 1)
            {
                throw ParseError(errorPos, Res.ArgsIncompatibleWithLambda);
            }

            return Expression.Invoke(lambda, args);
        }

        Expression ParseTypeAccess(Type type)
        {
            int errorPos = _textParser.CurrentToken.Pos;
            _textParser.NextToken();

            if (_textParser.CurrentToken.Id == TokenId.Question)
            {
                if (!type.GetTypeInfo().IsValueType || TypeHelper.IsNullableType(type))
                {
                    throw ParseError(errorPos, Res.TypeHasNoNullableForm, TypeHelper.GetTypeName(type));
                }

                type = typeof(Nullable<>).MakeGenericType(type);
                _textParser.NextToken();
            }

            Expression generatedExpression = null;

            // This is a shorthand for explicitly converting a string to something
            bool shorthand = _textParser.CurrentToken.Id == TokenId.StringLiteral;
            if (_textParser.CurrentToken.Id == TokenId.OpenParen || shorthand)
            {
                Expression[] args = shorthand ? new[] { ParseStringLiteral() } : ParseArgumentList();

                // If only 1 argument and
                // - the arg is ConstantExpression, return the conversion
                // OR
                // - the arg is null, return the conversion (Can't use constructor)
                //
                // Then try to GenerateConversion
                if (args.Length == 1 && (args[0] == null || args[0] is ConstantExpression) && TryGenerateConversion(args[0], type, out generatedExpression))
                {
                    return generatedExpression;
                }

                // If only 1 argument, and if the type is a ValueType and argType is also a ValueType, just Convert
                if (args.Length == 1)
                {
                    Type argType = args[0].Type;

                    if (type.GetTypeInfo().IsValueType && TypeHelper.IsNullableType(type) && argType.GetTypeInfo().IsValueType)
                    {
                        return Expression.Convert(args[0], type);
                    }
                }

                var constructorsWithOutPointerArguments = type.GetConstructors()
                    .Where(c => !c.GetParameters().Any(p => p.ParameterType.GetTypeInfo().IsPointer))
                    .ToArray();
                switch (_methodFinder.FindBestMethodBasedOnArguments(constructorsWithOutPointerArguments, ref args, out MethodBase method))
                {
                    case 0:
                        if (args.Length == 1 && TryGenerateConversion(args[0], type, out generatedExpression))
                        {
                            return generatedExpression;
                        }

                        throw ParseError(errorPos, Res.NoMatchingConstructor, TypeHelper.GetTypeName(type));

                    case 1:
                        return Expression.New((ConstructorInfo)method, args);

                    default:
                        throw ParseError(errorPos, Res.AmbiguousConstructorInvocation, TypeHelper.GetTypeName(type));
                }
            }

            // throw ParseError(errorPos, Res.CannotConvertValue, TypeHelper.GetTypeName(exprType), TypeHelper.GetTypeName(type));

            _textParser.ValidateToken(TokenId.Dot, Res.DotOrOpenParenOrStringLiteralExpected);
            _textParser.NextToken();

            return ParseMemberAccess(type, null);
        }

        private bool TryGenerateConversion(Expression sourceExpression, Type type, out Expression expression)
        {
            Type exprType = sourceExpression.Type;
            if (exprType == type)
            {
                expression = sourceExpression;
                return true;
            }

            if (exprType.GetTypeInfo().IsValueType && type.GetTypeInfo().IsValueType)
            {
                if ((TypeHelper.IsNullableType(exprType) || TypeHelper.IsNullableType(type)) && TypeHelper.GetNonNullableType(exprType) == TypeHelper.GetNonNullableType(type))
                {
                    expression = Expression.Convert(sourceExpression, type);
                    return true;
                }

                if ((TypeHelper.IsNumericType(exprType) || TypeHelper.IsEnumType(exprType)) && TypeHelper.IsNumericType(type) || TypeHelper.IsEnumType(type))
                {
                    expression = Expression.ConvertChecked(sourceExpression, type);
                    return true;
                }
            }

            if (exprType.IsAssignableFrom(type) || type.IsAssignableFrom(exprType) || exprType.GetTypeInfo().IsInterface || type.GetTypeInfo().IsInterface)
            {
                expression = Expression.Convert(sourceExpression, type);
                return true;
            }

            // Try to Parse the string rather than just generate the convert statement
            if (sourceExpression.NodeType == ExpressionType.Constant && exprType == typeof(string))
            {
                string text = (string)((ConstantExpression)sourceExpression).Value;

                var typeConvertor = _typeConverterFactory.GetConverter(type);
                if (typeConvertor != null && typeConvertor.CanConvertFrom(typeof(string)))
                {
                    var value = typeConvertor.ConvertFromInvariantString(text);
                    expression = Expression.Constant(value, type);
                    return true;
                }
            }

            // Check if there are any explicit conversion operators on the source type which fit the requirement (cast to the return type).
            bool explicitOperatorAvailable = exprType.GetTypeInfo().GetDeclaredMethods("op_Explicit").Any(m => m.ReturnType == type);
            if (explicitOperatorAvailable)
            {
                expression = Expression.Convert(sourceExpression, type);
                return true;
            }

            expression = null;
            return false;
        }

        Expression ParseMemberAccess(Type type, Expression expression)
        {
            if (expression != null)
            {
                type = expression.Type;
            }

            int errorPos = _textParser.CurrentToken.Pos;
            string id = GetIdentifier();
            _textParser.NextToken();

            if (_textParser.CurrentToken.Id == TokenId.OpenParen)
            {
                if (expression != null && type != typeof(string))
                {
                    Type enumerableType = TypeHelper.FindGenericType(typeof(IEnumerable<>), type);
                    if (enumerableType != null)
                    {
                        Type elementType = enumerableType.GetTypeInfo().GetGenericTypeArguments()[0];
                        return ParseEnumerable(expression, elementType, id, errorPos, type);
                    }
                }

                Expression[] args = ParseArgumentList();
                switch (_methodFinder.FindMethod(type, id, expression == null, ref expression, ref args, out MethodBase mb))
                {
                    case 0:
                        throw ParseError(errorPos, Res.NoApplicableMethod, id, TypeHelper.GetTypeName(type));

                    case 1:
                        MethodInfo method = (MethodInfo)mb;
                        if (!PredefinedTypesHelper.IsPredefinedType(_parsingConfig, method.DeclaringType) && !(method.IsPublic && PredefinedTypesHelper.IsPredefinedType(_parsingConfig, method.ReturnType)))
                        {
                            throw ParseError(errorPos, Res.MethodsAreInaccessible, TypeHelper.GetTypeName(method.DeclaringType));
                        }

                        if (method.IsGenericMethod)
                        {
                            var genericParameters = method.GetParameters().Where(p => p.ParameterType.IsGenericParameter);
                            var typeArguments = genericParameters.Select(a => args[a.Position].Type);
                            var constructedMethod = method.MakeGenericMethod(typeArguments.ToArray());
                            return Expression.Call(expression, constructedMethod, args);
                        }
                        else
                        {
                            return Expression.Call(expression, method, args);
                        }

                    default:
                        throw ParseError(errorPos, Res.AmbiguousMethodInvocation, id, TypeHelper.GetTypeName(type));
                }
            }

            var @enum = TypeHelper.ParseEnum(id, type);
            if (@enum != null)
            {
                return Expression.Constant(@enum);
            }

#if UAP10_0 || NETSTANDARD1_3
            if (type == typeof(DynamicClass))
            {
                return Expression.MakeIndex(expression, typeof(DynamicClass).GetProperty("Item"), new[] { Expression.Constant(id) });
            }
#endif
            MemberInfo member = FindPropertyOrField(type, id, expression == null);
            if (member is PropertyInfo property)
            {
                return Expression.Property(expression, property);
            }

            if (member is FieldInfo field)
            {
                return Expression.Field(expression, field);
            }

            if (!_parsingConfig.DisableMemberAccessToIndexAccessorFallback && expression != null)
            {
                MethodInfo indexerMethod = expression.Type.GetMethod("get_Item", new[] { typeof(string) });
                if (indexerMethod != null)
                {
                    return Expression.Call(expression, indexerMethod, Expression.Constant(id));
                }
            }

#if !NET35 && !UAP10_0 && !NETSTANDARD1_3
            if (type == typeof(object))
            {
                // The member is a dynamic or ExpandoObject, so convert this
                return _expressionHelper.ConvertToExpandoObjectAndCreateDynamicExpression(expression, type, id);
            }
#endif
            // Parse as Lambda
            if (_textParser.CurrentToken.Id == TokenId.Lambda && _it.Type == type)
            {
                return ParseAsLambda(id);
            }

            // This could be enum like "A.B.C.MyEnum.Value1" or "A.B.C+MyEnum.Value1"
            if (_textParser.CurrentToken.Id == TokenId.Dot || _textParser.CurrentToken.Id == TokenId.Plus)
            {
                return ParseAsEnum(id);
            }

            throw ParseError(errorPos, Res.UnknownPropertyOrField, id, TypeHelper.GetTypeName(type));
        }

        private Expression ParseAsLambda(string id)
        {
            // This might be an internal variable for use within a lambda expression, so store it as such
            _internals.Add(id, _it);
            string _previousItName = ItName;

            // Also store ItName (only once)
            if (string.Equals(ItName, KeywordsHelper.KEYWORD_IT))
            {
                ItName = id;
            }

            // next
            _textParser.NextToken();

            LastLambdaItName = ItName;
            var exp = ParseConditionalOperator();

            // Restore previous context and clear internals
            _internals.Remove(id);
            ItName = _previousItName;

            return exp;
        }

        private Expression ParseAsEnum(string id)
        {
            var parts = new List<string> { id };

            while (_textParser.CurrentToken.Id == TokenId.Dot || _textParser.CurrentToken.Id == TokenId.Plus)
            {
                if (_textParser.CurrentToken.Id == TokenId.Dot || _textParser.CurrentToken.Id == TokenId.Plus)
                {
                    parts.Add(_textParser.CurrentToken.Text);
                    _textParser.NextToken();
                }

                if (_textParser.CurrentToken.Id == TokenId.Identifier)
                {
                    parts.Add(_textParser.CurrentToken.Text);
                    _textParser.NextToken();
                }
            }

            var enumTypeAsString = string.Join("", parts.Take(parts.Count - 2).ToArray());
            var enumType = _typeFinder.FindTypeByName(enumTypeAsString, null, true);
            if (enumType == null)
            {
                throw ParseError(_textParser.CurrentToken.Pos, Res.EnumTypeNotFound, enumTypeAsString);
            }

            string enumValueAsString = parts.LastOrDefault();
            if (enumValueAsString == null)
            {
                throw ParseError(_textParser.CurrentToken.Pos, Res.EnumValueExpected);
            }

            var enumValue = TypeHelper.ParseEnum(enumValueAsString, enumType);
            if (enumValue == null)
            {
                throw ParseError(_textParser.CurrentToken.Pos, Res.EnumValueNotDefined, enumValueAsString, enumTypeAsString);
            }

            return Expression.Constant(enumValue);
        }

        Expression ParseEnumerable(Expression instance, Type elementType, string methodName, int errorPos, Type type)
        {
            bool isQueryable = TypeHelper.FindGenericType(typeof(IQueryable<>), type) != null;
            bool isDictionary = TypeHelper.IsDictionary(type);

            var oldParent = _parent;

            ParameterExpression outerIt = _it;
            ParameterExpression innerIt = ParameterExpressionHelper.CreateParameterExpression(elementType, string.Empty, _parsingConfig.RenameEmptyParameterExpressionNames);

            _parent = _it;

            if (methodName == "Contains" || methodName == "ContainsKey" || methodName == "Skip" || methodName == "Take")
            {
                // for any method that acts on the parent element type, we need to specify the outerIt as scope.
                _it = outerIt;
            }
            else
            {
                _it = innerIt;
            }

            Expression[] args = ParseArgumentList();

            _it = outerIt;
            _parent = oldParent;

            if (isDictionary && _methodFinder.ContainsMethod(typeof(IDictionarySignatures), methodName, false, null, ref args))
            {
                var method = type.GetMethod(methodName);
                return Expression.Call(instance, method, args);
            }

            if (!_methodFinder.ContainsMethod(typeof(IEnumerableSignatures), methodName, false, null, ref args))
            {
                throw ParseError(errorPos, Res.NoApplicableAggregate, methodName, string.Join(",", args.Select(a => a.Type.Name).ToArray()));
            }

            Type callType = typeof(Enumerable);
            if (isQueryable && _methodFinder.ContainsMethod(typeof(IQueryableSignatures), methodName, false, null, ref args))
            {
                callType = typeof(Queryable);
            }

            Type[] typeArgs;
            if (new[] { "OfType", "Cast" }.Contains(methodName))
            {
                if (args.Length != 1)
                {
                    throw ParseError(_textParser.CurrentToken.Pos, Res.FunctionRequiresOneArg, methodName);
                }

                typeArgs = new[] { ResolveTypeFromArgumentExpression(methodName, args[0]) };
                args = new Expression[0];
            }
            else if (new[] { "Min", "Max", "Select", "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending", "GroupBy" }.Contains(methodName))
            {
                if (args.Length == 2)
                {
                    typeArgs = new[] { elementType, args[0].Type, args[1].Type };
                }
                else
                {
                    typeArgs = new[] { elementType, args[0].Type };
                }
            }
            else if (methodName == "SelectMany")
            {
                var bodyType = Expression.Lambda(args[0], innerIt).Body.Type;
                var interfaces = bodyType.GetInterfaces().Union(new[] { bodyType });
                Type interfaceType = interfaces.Single(i => i.Name == typeof(IEnumerable<>).Name);
                Type resultType = interfaceType.GetTypeInfo().GetGenericTypeArguments()[0];
                typeArgs = new[] { elementType, resultType };
            }
            else
            {
                typeArgs = new[] { elementType };
            }

            if (args.Length == 0)
            {
                args = new[] { instance };
            }
            else
            {
                if (new[] { "Concat", "Contains", "DefaultIfEmpty", "Except", "Intersect", "Skip", "Take", "Union" }.Contains(methodName))
                {
                    args = new[] { instance, args[0] };
                }
                else
                {
                    if (args.Length == 2)
                    {
                        args = new[] { instance, Expression.Lambda(args[0], innerIt), Expression.Lambda(args[1], innerIt) };
                    }
                    else
                    {
                        args = new[] { instance, Expression.Lambda(args[0], innerIt) };
                    }
                }
            }

            return Expression.Call(callType, methodName, typeArgs, args);
        }

        private Type ResolveTypeFromArgumentExpression(string functionName, Expression argumentExpression)
        {
            switch (argumentExpression)
            {
                case ConstantExpression constantExpression:
                    switch (constantExpression.Value)
                    {
                        case string typeName:
                            return ResolveTypeStringFromArgument(functionName, typeName);

                        case Type type:
                            return type;

                        default:
                            throw ParseError(_textParser.CurrentToken.Pos, Res.FunctionRequiresOneNotNullArgOfType, functionName, "string or System.Type");
                    }

                default:
                    throw ParseError(_textParser.CurrentToken.Pos, Res.FunctionRequiresOneNotNullArgOfType, functionName, "ConstantExpression");
            }
        }

        private Type ResolveTypeStringFromArgument(string functionName, string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw ParseError(_textParser.CurrentToken.Pos, Res.FunctionRequiresOneNotNullArg, functionName, typeName);
            }

            Type resultType = _typeFinder.FindTypeByName(typeName, new[] { _it, _parent, _root }, true);
            if (resultType == null)
            {
                throw ParseError(_textParser.CurrentToken.Pos, Res.TypeNotFound, typeName);
            }

            return resultType;
        }

        Expression[] ParseArgumentList()
        {
            _textParser.ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            _textParser.NextToken();
            Expression[] args = _textParser.CurrentToken.Id != TokenId.CloseParen ? ParseArguments() : new Expression[0];
            _textParser.ValidateToken(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            _textParser.NextToken();
            return args;
        }

        Expression[] ParseArguments()
        {
            var argList = new List<Expression>();
            while (true)
            {
                var argumentExpression = ParseConditionalOperator();

                _expressionHelper.WrapConstantExpression(ref argumentExpression);

                argList.Add(argumentExpression);

                if (_textParser.CurrentToken.Id != TokenId.Comma)
                {
                    break;
                }

                _textParser.NextToken();
            }

            return argList.ToArray();
        }

        Expression ParseElementAccess(Expression expr)
        {
            int errorPos = _textParser.CurrentToken.Pos;
            _textParser.ValidateToken(TokenId.OpenBracket, Res.OpenParenExpected);
            _textParser.NextToken();

            Expression[] args = ParseArguments();
            _textParser.ValidateToken(TokenId.CloseBracket, Res.CloseBracketOrCommaExpected);
            _textParser.NextToken();

            if (expr.Type.IsArray)
            {
                if (expr.Type.GetArrayRank() != 1 || args.Length != 1)
                {
                    throw ParseError(errorPos, Res.CannotIndexMultiDimArray);
                }
                Expression index = _parsingConfig.ExpressionPromoter.Promote(args[0], typeof(int), true, false);

                if (index == null)
                {
                    throw ParseError(errorPos, Res.InvalidIndex);
                }

                return Expression.ArrayIndex(expr, index);
            }

            switch (_methodFinder.FindIndexer(expr.Type, args, out var mb))
            {
                case 0:
                    throw ParseError(errorPos, Res.NoApplicableIndexer,
                        TypeHelper.GetTypeName(expr.Type));
                case 1:
                    return Expression.Call(expr, (MethodInfo)mb, args);

                default:
                    throw ParseError(errorPos, Res.AmbiguousIndexerInvocation, TypeHelper.GetTypeName(expr.Type));
            }
        }

        internal static Type ToNullableType(Type type)
        {
            Check.NotNull(type, nameof(type));

            if (!type.GetTypeInfo().IsValueType || TypeHelper.IsNullableType(type))
            {
                throw ParseError(-1, Res.TypeHasNoNullableForm, TypeHelper.GetTypeName(type));
            }

            return typeof(Nullable<>).MakeGenericType(type);
        }

        static bool TryGetMemberName(Expression expression, out string memberName)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression == null && expression.NodeType == ExpressionType.Coalesce)
            {
                memberExpression = (expression as BinaryExpression).Left as MemberExpression;
            }

            if (memberExpression != null)
            {
                memberName = memberExpression.Member.Name;
                return true;
            }

#if NETFX_CORE
            var indexExpression = expression as IndexExpression;
            if (indexExpression != null && indexExpression.Indexer.DeclaringType == typeof(DynamicObjectClass))
            {
                memberName = ((ConstantExpression)indexExpression.Arguments.First()).Value as string;
                return true;
            }
#endif
            memberName = null;
            return false;
        }

        void CheckAndPromoteOperand(Type signatures, string opName, ref Expression expr, int errorPos)
        {
            Expression[] args = { expr };

            if (!_methodFinder.ContainsMethod(signatures, "F", false, null, ref args))
            {
                throw IncompatibleOperandError(opName, expr, errorPos);
            }

            expr = args[0];
        }

        static string GetOverloadedOperationName(TokenId tokenId)
        {
            switch (tokenId)
            {
                case TokenId.DoubleEqual:
                case TokenId.Equal:
                    return "op_Equality";
                case TokenId.ExclamationEqual:
                    return "op_Inequality";
                default:
                    return null;
            }
        }

        void CheckAndPromoteOperands(Type signatures, TokenId opId, string opName, ref Expression left, ref Expression right, int errorPos)
        {
            Expression[] args = { left, right };

            // support operator overloading
            var nativeOperation = GetOverloadedOperationName(opId);
            bool found = false;

            if (nativeOperation != null)
            {
                // first try left operand's equality operators
                found = _methodFinder.ContainsMethod(left.Type, nativeOperation, true, null, ref args);
                if (!found)
                {
                    found = _methodFinder.ContainsMethod(right.Type, nativeOperation, true, null, ref args);
                }
            }

            if (!found && !_methodFinder.ContainsMethod(signatures, "F", false, null, ref args))
            {
                throw IncompatibleOperandsError(opName, left, right, errorPos);
            }

            left = args[0];
            right = args[1];
        }

        static Exception IncompatibleOperandError(string opName, Expression expr, int errorPos)
        {
            return ParseError(errorPos, Res.IncompatibleOperand, opName, TypeHelper.GetTypeName(expr.Type));
        }

        static Exception IncompatibleOperandsError(string opName, Expression left, Expression right, int errorPos)
        {
            return ParseError(errorPos, Res.IncompatibleOperands, opName, TypeHelper.GetTypeName(left.Type), TypeHelper.GetTypeName(right.Type));
        }

        private MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
        {
#if !(NETFX_CORE || WINDOWS_APP ||  UAP10_0 || NETSTANDARD)
            BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly | (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            foreach (Type t in TypeHelper.GetSelfAndBaseTypes(type))
            {
                MemberInfo[] members = null;

                var findMembersType = _parsingConfig?.IsCaseSensitive == true ? Type.FilterName : Type.FilterNameIgnoreCase;
                members = t.FindMembers(MemberTypes.Property | MemberTypes.Field, flags, findMembersType, memberName);

                if (members.Length != 0)
                {
                    return members[0];
                }
            }
            return null;
#else
            var isCaseSensitive = _parsingConfig?.IsCaseSensitive == true;
            foreach (Type t in TypeHelper.GetSelfAndBaseTypes(type))
            {
                // Try to find a property with the specified memberName
                MemberInfo member = t.GetTypeInfo().DeclaredProperties.FirstOrDefault(x => (!staticAccess || x.GetAccessors(true)[0].IsStatic) && ((x.Name == memberName) || (!isCaseSensitive && x.Name.Equals(memberName, StringComparison.OrdinalIgnoreCase))));
                if (member != null)
                {
                    return member;
                }

                // If no property is found, try to get a field with the specified memberName
                member = t.GetTypeInfo().DeclaredFields.FirstOrDefault(x => (!staticAccess || x.IsStatic) && ((x.Name == memberName) || (!isCaseSensitive && x.Name.Equals(memberName, StringComparison.OrdinalIgnoreCase))));
                if (member != null)
                {
                    return member;
                }

                // No property or field is found, try the base type.
            }
            return null;
#endif
        }

        bool TokenIdentifierIs(string id)
        {
            return _textParser.CurrentToken.Id == TokenId.Identifier && string.Equals(id, _textParser.CurrentToken.Text, StringComparison.OrdinalIgnoreCase);
        }

        string GetIdentifier()
        {
            _textParser.ValidateToken(TokenId.Identifier, Res.IdentifierExpected);
            string id = _textParser.CurrentToken.Text;
            if (id.Length > 1 && id[0] == '@')
            {
                id = id.Substring(1);
            }

            return id;
        }

        Exception ParseError(string format, params object[] args)
        {
            return ParseError(_textParser?.CurrentToken.Pos ?? 0, format, args);
        }

        static Exception ParseError(int pos, string format, params object[] args)
        {
            return new ParseException(string.Format(CultureInfo.CurrentCulture, format, args), pos);
        }
    }
}
