using FluentAssertions;
using NFluent;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Dynamic.Core.Exceptions;
using System.Linq.Dynamic.Core.Tests.Helpers.Models;
using Xunit;

namespace System.Linq.Dynamic.Core.Tests
{
#if NET46_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NETSTANDARD1_3_OR_GREATER || UAP10_0
    public partial class QueryableTests 
    {

    [Fact]
        public void All_WithArgs_FS()
        {
            const int value = 50;

            //Arrange
            var queryable = User.GenerateSampleModels(100).AsQueryable();

            //Act
            bool expected = queryable.All(u => u.Income > value);
            bool result = queryable.AllInterpolated($"Income > {value}");
            //Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Any_Predicate_FS()
        {
            //Arrange
            var queryable = User.GenerateSampleModels(100).AsQueryable();

            int value = 50;
            //Act
            bool expected = queryable.Any(u => u.Income > value);
            bool result = queryable.AnyInterpolated($"Income > {value}");

            //Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Any_Predicate_WithArgs_FS()
        {
            const int value = 50;

            //Arrange
            var queryable = User.GenerateSampleModels(100).AsQueryable();

            //Act
            bool expected = queryable.Any(u => u.Income > value);
            bool result = queryable.AnyInterpolated($"Income > {value}");

            //Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Any_Dynamic_Where_FS()
        {
            const string search = "e";

            // Arrange
            var testList = User.GenerateSampleModels(10);
            var queryable = testList.AsQueryable();

            // Act
            var expected = queryable.Where(u => u.Roles.Any(r => r.Name.Contains(search))).ToArray();
            var result = queryable.WhereInterpolated($"Roles.Any(Name.Contains({search}))").ToArray();

            Assert.Equal(expected, result);
        }

        // https://dynamiclinq.codeplex.com/discussions/654313
        [Fact]
        public void Any_Dynamic_Where_Nested_FS()
        {
            const string search = "a";

            // Arrange
            var testList = User.GenerateSampleModels(10);
            var queryable = testList.AsQueryable();

            // Act
            var expected = queryable.Where(u => u.Roles.Any(r => r.Permissions.Any(p => p.Name.Contains(search)))).ToArray();
            var result = queryable.WhereInterpolated($"Roles.Any(Permissions.Any(Name.Contains({search})))").ToArray();

            Assert.Equal(expected, result);
        }

        // http://stackoverflow.com/questions/30846189/nested-any-in-is-not-working-in-dynamic-linq
        [Fact]
        public void Any_Dynamic_Where_Nested2_FS()
        {
            // arrange
            var list = new List<A>
            {
                new A {Bs = new List<B> {new B {A = new A(), Cs = new List<C> {new C {B = new B()}}}}}
            };
            var queryable = list.AsQueryable();

            // act : 1
            var result1 = queryable.WhereInterpolated($"(Name = \"\") && (Bs.Any(Cs.Any()))").ToList();
            var expected1 = queryable.Where(a => a.Name == "" && a.Bs.Any(b => b.Cs.Any()));
            Assert.Equal(expected1, result1);

            // act : 2
            var result2 = queryable.WhereInterpolated($"(Bs.Any(Cs.Any())) && (Name = \"\")").ToList();
            var expected2 = queryable.Where(a => a.Bs.Any(b => b.Cs.Any() && a.Name == ""));
            Assert.Equal(expected2, result2);
        }

        [Fact]
        public void First_Predicate_FS()
        {
            //Arrange
            var testList = User.GenerateSampleModels(100);
            var queryable = testList.AsQueryable();

            //Act
            int value = 1000;
            var expected = queryable.First(u => u.Income > value);
            var result = queryable.FirstInterpolated($"Income > {value}");

            //Assert
            Assert.Equal(expected as object, result);
        }

        [Fact]
        public void First_Predicate_WithArgs_FS()
        {
            const int value = 1000;

            //Arrange
            var testList = User.GenerateSampleModels(100);
            var queryable = testList.AsQueryable();

            //Act
            var expected = queryable.First(u => u.Income > value);
            var result = queryable.FirstInterpolated($"Income > {value}");

            //Assert
            Assert.Equal(expected as object, result);
        }

        [Fact]
        public void FirstOrDefault_Predicate_FS()
        {
            //Arrange
            var testList = User.GenerateSampleModels(100);
            var queryable = testList.AsQueryable();

            // Act
            int value = 1000;
            var expected = queryable.FirstOrDefault(u => u.Income > value);
            var result = queryable.FirstOrDefaultInterpolated($"Income > {value}");

            // Assert
            Check.That(result).Equals(expected);
        }

        [Fact]
        public void FirstOrDefault_Predicate_WithArgs_FS()
        {
            const int value = 1000;

            // Arrange
            var testList = User.GenerateSampleModels(100);
            var queryable = testList.AsQueryable();

            // Act
            var expected = queryable.FirstOrDefault(u => u.Income > value);
            var result = queryable.FirstOrDefaultInterpolated($"Income > {value}");

            // Assert
            Check.That(result).Equals(expected);
        }

        [Fact]
        public void Last_Predicate_FS()
        {
            //Arrange
            var testList = User.GenerateSampleModels(100);
            var queryable = testList.AsQueryable();

            //Act
            int value = 1000;
            var expected = queryable.Last(u => u.Income > value);
            var result = queryable.LastInterpolated($"Income > {value}");

            //Assert
            Assert.Equal(expected as object, result);
        }

        [Fact]
        public void LastOrDefault_Predicate_FS()
        {
            //Arrange
            var testList = User.GenerateSampleModels(100);
            var queryable = testList.AsQueryable();

            //Act
            int value = 1000;
            var expected = queryable.LastOrDefault(u => u.Income > value);
            var result = queryable.LastOrDefaultInterpolated($"Income > {value}");

            //Assert
            Assert.Equal(expected as object, result);
        }

        [Fact]
        public void LongCount_Predicate_FS()
        {
            //Arrange
            var queryable = User.GenerateSampleModels(100).AsQueryable();

            //Act
            long expected = queryable.LongCount(u => u.Income > 50);
            long result = queryable.LongCount("Income > 50");

            //Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void LongCount_Predicate_WithArgs_FS()
        {
            const int value = 50;

            //Arrange
            var queryable = User.GenerateSampleModels(100).AsQueryable();

            //Act
            long expected = queryable.LongCount(u => u.Income > value);
            long result = queryable.LongCountInterpolated($"Income >{value}");

            //Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void LongCount_Dynamic_Where_FS()
        {
            const string search = "e";

            // Arrange
            var testList = User.GenerateSampleModels(10);
            var queryable = testList.AsQueryable();

            // Act
            var expected = queryable.Where(u => u.Roles.LongCount(r => r.Name.Contains(search)) > 0).ToArray();
            var result = queryable.WhereInterpolated($"Roles.LongCount(Name.Contains({search})) > 0").ToArray();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void OrderBy_Dynamic_Exceptions_FS()
        {
            //Arrange
            var testList = User.GenerateSampleModels(100, allowNullableProfiles: true);
            var qry = testList.AsQueryable();

            //Act
            int bad = 3;
            int id = 123;
            Assert.Throws<ParseException>(() => qry.OrderByInterpolated($"Bad={bad}"));
            Assert.Throws<ParseException>(() => qry.WhereInterpolated($"Id={id}"));

            Assert.Throws<ArgumentNullException>(() => DynamicQueryableExtensions.OrderBy(null, "Id"));
            Assert.Throws<ArgumentException>(() => qry.OrderByInterpolated($""));
            Assert.Throws<ArgumentException>(() => qry.OrderByInterpolated($" "));
        }

        [Fact]
        public void Single_Predicate_FS()
        {
            //Arrange
            var testList = User.GenerateSampleModels(100);
            var testListQry = testList.AsQueryable();

            //Act
            string value = "User4";
            var expected = testListQry.Single(u => u.UserName ==value);
            var result = testListQry.SingleInterpolated($"UserName == {value}");

            //Assert
            Assert.Equal(expected as object, result);
        }

        [Fact]
        public void SingleOrDefault_Predicate_FS()
        {
            //Arrange
            var testList = User.GenerateSampleModels(100);
            var testListQry = testList.AsQueryable();

            //Act
            string value = "User4";
            var expected = testListQry.SingleOrDefault(u => u.UserName == value);
            var result = testListQry.SingleOrDefaultInterpolated($"UserName == {value}");

            //Assert
            Assert.Equal(expected as object, result);
        }

        [Fact]
        public void SkipWhile_Predicate_FS()
        {
            //Arrange
            var testList = User.GenerateSampleModels(100);
            IQueryable testListQry = testList.AsQueryable();

            //Act
            int value = 1000;
            var expected = testList.SkipWhile(u => u.Income > value);
            var result = testListQry.SkipWhileInterpolated($"Income > {value}");

            //Assert
            Assert.Equal(expected.ToArray(), result.Cast<User>().ToArray());
        }

        [Fact]
        public void SkipWhile_Predicate_Args_FS()
        {
            const int income = 1000;

            //Arrange
            var testList = User.GenerateSampleModels(100);
            IQueryable testListQry = testList.AsQueryable();

            //Act
            int value = 1000;
            var expected = testList.SkipWhile(u => u.Income > value);
            var result = testListQry.SkipWhileInterpolated($"Income > {value}");

            //Assert
            Assert.Equal(expected.ToArray(), result.Cast<User>().ToArray());
        }

        [Fact]
        public void TakeWhile_Predicate_FS()
        {
            //Arrange
            var testList = User.GenerateSampleModels(100);
            IQueryable testListQry = testList.AsQueryable();

            //Act
            int value = 1000;
            var expected = testList.TakeWhile(u => u.Income > 1000);
            var result = testListQry.TakeWhileInterpolated($"Income > {value}");

            //Assert
            Assert.Equal(expected.ToArray(), result.Cast<User>().ToArray());
        }

        [Fact]
        public void TakeWhile_Predicate_Args_FS()
        {
            const int income = 1000;

            //Arrange
            var testList = User.GenerateSampleModels(100);
            IQueryable testListQry = testList.AsQueryable();

            //Act
            var expected = testList.TakeWhile(u => u.Income > income);
            var result = testListQry.TakeWhileInterpolated($"Income > {income}");

            //Assert
            Assert.Equal(expected.ToArray(), result.Cast<User>().ToArray());
        }

        [Fact]
        public void ThenBy_Dynamic_Exceptions_FS()
        {
            //Arrange
            var testList = User.GenerateSampleModels(100, allowNullableProfiles: true);
            var qry = testList.AsQueryable();

            //Act
            int bad = 3;
            int id = 123;
            var ordered = qry.OrderBy("Id");
            Assert.Throws<ParseException>(() => ordered.ThenByInterpolated($"Bad={bad}"));
            Assert.Throws<ParseException>(() => ordered.WhereInterpolated($"Id={id}"));

            Assert.Throws<ArgumentNullException>(() => DynamicQueryableExtensions.ThenBy(null, "Id"));
            Assert.Throws<ArgumentException>(() => ordered.ThenByInterpolated($""));
            Assert.Throws<ArgumentException>(() => ordered.ThenByInterpolated($" "));
        }

        [Fact]
        public void Where_Dynamic_Exceptions_FS()
        {
            //Arrange
            var testList = User.GenerateSampleModels(100, allowNullableProfiles: true);
            var qry = testList.AsQueryable();

            //Act
            int bad = 3;
            int id = 123;
            Assert.Throws<InvalidOperationException>(() => qry.WhereInterpolated($"Id"));
            Assert.Throws<ParseException>(() => qry.WhereInterpolated($"Bad={bad}"));
            Assert.Throws<ParseException>(() => qry.WhereInterpolated($"Id={id}"));

            Assert.Throws<ArgumentNullException>(() => DynamicQueryableExtensions.Where(null, "Id=1"));
            Assert.Throws<ArgumentException>(() => qry.WhereInterpolated($""));
            Assert.Throws<ArgumentException>(() => qry.WhereInterpolated($" "));
        }

        [Fact]
        public void Where_Dynamic_StringQuoted_FS()
        {
            // Arrange
            var testList = User.GenerateSampleModels(2, allowNullableProfiles: true);
            testList[0].UserName = @"This \""is\"" a test.";
            var qry = testList.AsQueryable();

            // Act
            // var result1a = qry.Where(@"UserName == ""This \\""is\\"" a test.""").ToArray();
            var result1b = qry.WhereInterpolated($"UserName == \"This \\\\\\\"is\\\\\\\" a test.\"").ToArray();
            string s1 = @"This \""is\"" a test.";
            string s2 = "This \\\"is\\\" a test.";
            var result2a = qry.WhereInterpolated($"UserName == {s1}").ToArray();
            var result2b = qry.WhereInterpolated($"UserName == {s2}").ToArray();

            var expected = qry.Where(x => x.UserName == @"This \""is\"" a test.").ToArray();

            // Assert
            Assert.Single(expected);
            // Assert.Equal(expected, result1a);
            Assert.Equal(expected, result1b);
            Assert.Equal(expected, result2a);
            Assert.Equal(expected, result2b);
        }

        [Fact]
        public void Where_Dynamic_SelectNewObjects_FS()
        {
            //Arrange
            var testList = User.GenerateSampleModels(100, allowNullableProfiles: true);
            var qry = testList.AsQueryable();

            //Act
            int value = 4000;
            var expectedResult = testList.Where(x => x.Income > value).Select(x => new { Id = x.Id, Income = x.Income + 1111 });
            var dynamicList = qry.WhereInterpolated($"Income > {value}").ToDynamicList();

            var newUsers = dynamicList.Select(x => new { Id = x.Id, Income = x.Income + 1111 });
            Assert.Equal(newUsers.Cast<object>().ToList(), expectedResult);
        }

        [Fact]
        public void Where_Dynamic_ExpandoObject_As_Dictionary_Is_Null_Should_Throw_InvalidOperationException_FS()
        {
            // Arrange
            var productsQuery = new[] { new ProductDynamic { ProductId = 1 } }.AsQueryable();

            // Act
            string s = "First Product";
            Action action = () => productsQuery.WhereInterpolated($"Properties.Name == {s}" ).ToDynamicList();

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact(Skip = "NP does not work here")]
        public void Where_Dynamic_ExpandoObject_As_Dictionary_Is_Null_With_NullPropagating_FS()
        {
            // Arrange
            var productsQuery = new[] { new ProductDynamic { ProductId = 1 } }.AsQueryable();

            // Act
            string s = "First Product";
            var results = productsQuery.WhereInterpolated($"np(Properties.Name, \"no\") == {s}" ).ToDynamicList();

            // Assert
            results.Should().HaveCount(0);
        }

        [Fact]
        public void Where_Dynamic_ExpandoObject_As_Dictionary_FS()
        {
            // Arrange
            var productsQuery = new[] { new ProductDynamic { ProductId = 1, Properties = new Dictionary<string, object> { { "Name", "test" } } } }.AsQueryable();

            // Act
            string s = "test";
            var results = productsQuery.WhereInterpolated($"Properties.Name == {s}").ToDynamicList();

            // Assert
            results.Should().HaveCount(1);
        }

        [Fact]
        public void Where_Dynamic_Object_As_Dictionary_FS()
        {
            // Arrange
            var productsQuery = new[] { new ProductDynamic { ProductId = 1, PropertiesAsObject = new Dictionary<string, object> { { "Name", "test" } } } }.AsQueryable();

            // Act
            string s = "test";
            var results = productsQuery.WhereInterpolated($"PropertiesAsObject.Name == {s}").ToDynamicList();

            // Assert
            results.Should().HaveCount(1);
        }

        [Fact]
        public void Where_Dynamic_ExpandoObject_As_AnonymousType_FS()
        {
            // Arrange
            var productsQuery = new[] { new ProductDynamic { ProductId = 1, Properties = new { Name = "test" } } }.AsQueryable();

            // Act
            string s = "test";
            var results = productsQuery.WhereInterpolated($"Properties.Name == {s}").ToDynamicList<ProductDynamic>();

            // Assert
            results.Should().HaveCount(1);
        }
    }
#endif
}
