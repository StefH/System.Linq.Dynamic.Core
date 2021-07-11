using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq.Dynamic.Core.Exceptions;
using System.Linq.Dynamic.Core.Tests.Helpers;
using System.Linq.Dynamic.Core.Tests.Helpers.Models;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NFluent;
using Xunit;

namespace System.Linq.Dynamic.Core.Tests
{
    public enum TestEnumPublic : sbyte
    {
        Var1 = 0,
        Var2 = 1,
        Var3 = 2,
        Var4 = 4,
        Var5 = 8,
        Var6 = 16
    }

    public class ExpressionTests
    {
        public enum TestEnum2 : sbyte
        {
            Var1 = 0,
            Var2 = 1,
            Var3 = 2,
            Var4 = 4,
            Var5 = 8,
            Var6 = 16
        }

        public class TestEnumClass
        {
            public TestEnum A { get; set; }

            public TestEnum2 B { get; set; }

            public TestEnum2? C { get; set; }

            public TestEnumPublic E { get; set; }

            public int Id { get; set; }
        }

        public class TestGuidNullClass
        {
            public Guid? GuidNull { get; set; }

            public Guid GuidNormal { get; set; }

            public int Id { get; set; }
        }

        public class TestObjectIdClass
        {
            public int Id { get; set; }

            public long ObjectId { get; set; }
        }

        public class Foo
        {
            public Foo FooValue { get; set; }

            public string Zero() => null;

            public string One(int x) => null;

            public string Two(int x, int y) => null;
        }

        public class SourceClass
        {
            public int A { get; set; }
        }

        public class TargetClass
        {
            public TestEnum A { get; set; }
        }

        [Fact]
        public void ExpressionTests_Add_Number()
        {
            // Arrange
            var values = new[] { -1, 2 }.AsQueryable();

            // Act
            var result = values.Select<int>("it + 1");
            var expected = values.Select(i => i + 1);

            // Assert
            Check.That(result).ContainsExactly(expected);
        }

        [Fact]
        public void ExpressionTests_Add_String()
        {
            // Arrange
            var values = new[] { "a", "b" }.AsQueryable();

            // Act
            var result = values.Select<string>("it + \"z\"");
            var expected = values.Select(i => i + "z");

            // Assert
            Check.That(result).ContainsExactly(expected);
        }


        [Fact]
        public void ExpressionTests_AndAlso()
        {
            // Arrange
            var users = new[] { new User { Income = 1 }, new User { Income = 2 }, new User { Income = 3 } }.AsQueryable();

            // Act
            var result = users.Where(u => u.Income == 1 && u.Income == 2).ToArray();
            var resultDynamic = users.Where("Income == 1 AndAlso Income == 2").ToArray();

            // Assert
            Check.That(resultDynamic).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_ArrayInitializer()
        {
            // Arrange
            var list = new[] { 0, 1, 2, 3, 4 };

            // Act
            var result1 = list.AsQueryable().SelectMany("new[] {}");
            var result2 = list.AsQueryable().SelectMany("new[] { it + 1, it + 2 }");
            var result3 = list.AsQueryable().SelectMany("new long[] { it + 1, byte(it + 2), short(it + 3) }");
            var result4 = list.AsQueryable().SelectMany("new[] { new[] { it + 1, it + 2 }, new[] { it + 3, it + 4 } }");

            // Assert
            Assert.Equal(0, result1.Count());
            Assert.Equal(result2.Cast<int>(), list.SelectMany(item => new[] { item + 1, item + 2 }));
            Assert.Equal(result3.Cast<long>(), list.SelectMany(item => new long[] { item + 1, (byte)(item + 2), (short)(item + 3) }));
            Assert.Equal(result4.SelectMany("it").Cast<int>(), list.SelectMany(item => new[] { new[] { item + 1, item + 2 }, new[] { item + 3, item + 4 } }).SelectMany(item => item));

            Assert.Throws<ParseException>(() => list.AsQueryable().SelectMany("new[ {}"));
            Assert.Throws<ParseException>(() => list.AsQueryable().SelectMany("new] {}"));
        }

        [Fact]
        public void ExpressionTests_BinaryAndNumericConvert()
        {
            // Arrange
            var lst = new List<TestEnumClass>
            {
                new TestEnumClass {A = TestEnum.Var3, B = TestEnum2.Var3, Id = 1},
                new TestEnumClass {A = TestEnum.Var4, B = TestEnum2.Var4, Id = 2},
                new TestEnumClass {A = TestEnum.Var2, B = TestEnum2.Var2, Id = 3}
            };
            var qry = lst.AsQueryable();

            // Act
            var result0 = qry.FirstOrDefault("(it.A & @0) == 1", 1);
            var result1 = qry.FirstOrDefault("(it.A & @0) == 1", (uint)1);
            var result2 = qry.FirstOrDefault("(it.A & @0) == 1", (long)1);
            var result3 = qry.FirstOrDefault("(it.A & @0) == 1", (ulong)1);
            var result4 = qry.FirstOrDefault("(it.A & @0) == 1", (byte)1);
            var result5 = qry.FirstOrDefault("(it.A & @0) == 1", (sbyte)1);
            var result6 = qry.FirstOrDefault("(it.A & @0) == 1", (ushort)1);
            var result7 = qry.FirstOrDefault("(it.A & @0) == 1", (short)1);

            var result10 = qry.FirstOrDefault("(it.B & @0) == 1", 1);
            var result11 = qry.FirstOrDefault("(it.B & @0) == 1", (uint)1);
            var result12 = qry.FirstOrDefault("(it.B & @0) == 1", (long)1);
            var result13 = qry.FirstOrDefault("(it.B & @0) == 1", (ulong)1);
            var result14 = qry.FirstOrDefault("(it.B & @0) == 1", (byte)1);
            var result15 = qry.FirstOrDefault("(it.B & @0) == 1", (sbyte)1);
            var result16 = qry.FirstOrDefault("(it.B & @0) == 1", (ushort)1);
            var result17 = qry.FirstOrDefault("(it.B & @0) == 1", (short)1);

            // Assert
            Assert.Equal(3, result0.Id);
            Assert.Equal(3, result1.Id);
            Assert.Equal(3, result2.Id);
            Assert.Equal(3, result3.Id);
            Assert.Equal(3, result4.Id);
            Assert.Equal(3, result5.Id);
            Assert.Equal(3, result6.Id);
            Assert.Equal(3, result7.Id);

            Assert.Equal(3, result10.Id);
            Assert.Equal(3, result11.Id);
            Assert.Equal(3, result12.Id);
            Assert.Equal(3, result13.Id);
            Assert.Equal(3, result14.Id);
            Assert.Equal(3, result15.Id);
            Assert.Equal(3, result16.Id);
            Assert.Equal(3, result17.Id);
        }

        [Fact]
        public void ExpressionTests_BinaryOrNumericConvert()
        {
            // Arrange
            var lst = new List<TestEnumClass>
            {
                new TestEnumClass {A = TestEnum.Var3, B = TestEnum2.Var3, Id = 1},
                new TestEnumClass {A = TestEnum.Var4, B = TestEnum2.Var4, Id = 2},
                new TestEnumClass {A = TestEnum.Var2, B = TestEnum2.Var2, Id = 3}
            };
            var qry = lst.AsQueryable();

            // Act
            var result0 = qry.FirstOrDefault("(it.A | @0) == 1", 1);
            var result1 = qry.FirstOrDefault("(it.A | @0) == 1", (uint)1);
            var result2 = qry.FirstOrDefault("(it.A | @0) == 1", (long)1);
            var result3 = qry.FirstOrDefault("(it.A | @0) == 1", (ulong)1);
            var result4 = qry.FirstOrDefault("(it.A | @0) == 1", (byte)1);
            var result5 = qry.FirstOrDefault("(it.A | @0) == 1", (sbyte)1);
            var result6 = qry.FirstOrDefault("(it.A | @0) == 1", (ushort)1);
            var result7 = qry.FirstOrDefault("(it.A | @0) == 1", (short)1);

            var result10 = qry.FirstOrDefault("(it.B | @0) == 1", 1);
            var result11 = qry.FirstOrDefault("(it.B | @0) == 1", (uint)1);
            var result12 = qry.FirstOrDefault("(it.B | @0) == 1", (long)1);
            var result13 = qry.FirstOrDefault("(it.B | @0) == 1", (ulong)1);
            var result14 = qry.FirstOrDefault("(it.B | @0) == 1", (byte)1);
            var result15 = qry.FirstOrDefault("(it.B | @0) == 1", (sbyte)1);
            var result16 = qry.FirstOrDefault("(it.B | @0) == 1", (ushort)1);
            var result17 = qry.FirstOrDefault("(it.B | @0) == 1", (short)1);

            // Assert
            Assert.Equal(3, result0.Id);
            Assert.Equal(3, result1.Id);
            Assert.Equal(3, result2.Id);
            Assert.Equal(3, result3.Id);
            Assert.Equal(3, result4.Id);
            Assert.Equal(3, result5.Id);
            Assert.Equal(3, result6.Id);
            Assert.Equal(3, result7.Id);

            Assert.Equal(3, result10.Id);
            Assert.Equal(3, result11.Id);
            Assert.Equal(3, result12.Id);
            Assert.Equal(3, result13.Id);
            Assert.Equal(3, result14.Id);
            Assert.Equal(3, result15.Id);
            Assert.Equal(3, result16.Id);
            Assert.Equal(3, result17.Id);
        }

        [Fact]
        public void ExpressionTests_Cast_To_nullableint()
        {
            // Arrange
            var list = new List<SimpleValuesModel>
            {
                new SimpleValuesModel { IntValue = 5 }
            };

            // Act
            var expectedResult = list.Select(x => (int?)x.IntValue);
            var result = list.AsQueryable().Select("int?(IntValue)");

            // Assert
            Assert.Equal(expectedResult.ToArray(), result.ToDynamicArray<int?>());
        }

        [Fact]
        public void ExpressionTests_Cast_Automatic_To_nullablelong()
        {
            // Arrange
            var q = new[] { null, new UserProfile(), new UserProfile { UserProfileDetails = new UserProfileDetails { Id = 42 } } }.AsQueryable();

            // Act
            var expectedResult = q.Select(x => x != null && x.UserProfileDetails != null ? (long?)x.UserProfileDetails.Id : null);
            var result = q.Select("it != null && it.UserProfileDetails != null ? it.UserProfileDetails.Id : null");

            // Assert
            Assert.Equal(expectedResult.ToArray(), result.ToDynamicArray<long?>());
        }

        [Fact]
        public void ExpressionTests_Cast_To_newnullableint()
        {
            // Arrange
            var list = new List<SimpleValuesModel>
            {
                new SimpleValuesModel { IntValue = 5 }
            };

            // Act
            var expectedResult = list.Select(x => new { i = (int?)x.IntValue });
            var result = list.AsQueryable().Select("new (int?(IntValue) as i)");

            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
        }

        [Fact]
        public void ExpressionTests_Conditional_With_Automatic_Cast_NullableInt()
        {
            // Arrange
            var qry = new[] { 1, 2 }.AsQueryable();

            // Act
            var realResult1 = qry.Select(x => x == 2 ? null : (int?)x).ToList();
            var result1 = qry.Select("it == 2 ? null : it").ToDynamicList<int?>();
            var realResult2 = qry.Select(x => x == 2 ? (int?)x : null).ToList();
            var result2 = qry.Select("it == 2 ? it : null").ToDynamicList<int?>();

            // Assert
            Check.That(result1).ContainsExactly(realResult1);
            Check.That(result2).ContainsExactly(realResult2);
        }

        [Fact]
        public void ExpressionTests_Conditional_With_No_Automatic_Cast()
        {
            // Arrange
            var qry = new[] { null, new SimpleValuesModel() }.AsQueryable();

            // Act
            var realResult1 = qry.Select(x => x != null ? x : null).ToList();
            var result1 = qry.Select("it != null ? it : null").ToDynamicList<SimpleValuesModel>();
            var realResult2 = qry.Select(x => x == null ? null : x).ToList();
            var result2 = qry.Select("it == null ? null : it").ToDynamicList<SimpleValuesModel>();

            // Assert
            Check.That(result1).ContainsExactly(realResult1);
            Check.That(result2).ContainsExactly(realResult2);
        }

        [Fact]
        public void ExpressionTests_Conditional()
        {
            // Arrange
            int[] values = { 1, 2, 3, 4, 5 };
            var qry = values.AsQueryable();

            // Act
            var realResult = values.Select(x => x == 2 ? 99 : 100).ToList();
            var result = qry.Select("it == 2 ? 99 : 100").ToDynamicList<int>();

            // Assert
            Check.That(result).ContainsExactly(realResult);
        }

        [Fact]
        public void ExpressionTests_ConditionalOr1()
        {
            // Arrange
            int[] values = { 1, 2, 3, 4, 5 };
            var qry = values.AsQueryable();

            // Act
            var realResult = values.Where(x => x == 2 || x == 3).ToList();
            var result = qry.Where("it == 2 or it == 3").ToDynamicList<int>();

            // Assert
            Check.That(realResult).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_ConditionalOr2()
        {
            // Arrange
            int[] values = { 1, 2, 3, 4, 5 };
            var qry = values.AsQueryable();

            // Act
            var realResult = values.Where(x => x == 2 || x == 3).ToList();
            var result = qry.Where("it == 2 || it == 3").ToDynamicList<int>();

            // Assert
            Check.That(realResult).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_ConditionalAnd1()
        {
            // Arrange
            var values = new[] { new { s = "s", i = 1 }, new { s = "abc", i = 2 } };
            var qry = values.AsQueryable();

            // Act
            var realResult = values.Where(x => x.s == "s" && x.i == 2).ToList();
            var result = qry.Where("s == \"s\" and i == 2").ToDynamicList();

            // Assert
            Check.That(realResult).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_ConditionalAnd2()
        {
            // Arrange
            var values = new[] { new { s = "s", i = 1 }, new { s = "abc", i = 2 } };
            var qry = values.AsQueryable();

            // Act
            var realResult = values.Where(x => x.s == "s" && x.i == 2).ToList();
            var result = qry.Where("s == \"s\" && i == 2").ToDynamicList();

            // Assert
            Check.That(realResult).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_ContainsGuid()
        {
            // Arrange
            var userList = User.GenerateSampleModels(5);
            var userQry = userList.AsQueryable();

            var failValues = new List<Guid>
            {
                new Guid("{22222222-7651-4045-962A-3D44DEE71398}"),
                new Guid("{33333333-8F80-4497-9125-C96DEE23037D}"),
                new Guid("{44444444-E32D-4DE1-8F1C-A144C2B0424D}")
            };
            var successValues = failValues.Concat(new[] { userList[0].Id }).ToArray();


            // Act
            var found1 = userQry.Where("Id in @0", successValues);
            var found2 = userQry.Where("@0.Contains(Id)", successValues);
            var notFound1 = userQry.Where("Id in @0", failValues);
            var notFound2 = userQry.Where("@0.Contains(Id)", failValues);

            // Assert
#if NET35
            Assert.Equal(userList[0].Id, ((User)found1.Single()).Id);
            Assert.Equal(userList[0].Id, ((User)found2.Single()).Id);
#else
            Assert.Equal(userList[0].Id, found1.Single().Id);
            Assert.Equal(userList[0].Id, found2.Single().Id);
#endif
            Assert.False(notFound1.Any());
            Assert.False(notFound2.Any());
        }

        [Fact]
        public void ExpressionTests_DateTimeString()
        {
            // Arrange
            var lst = new List<DateTime> { DateTime.Today, DateTime.Today.AddDays(1), DateTime.Today.AddDays(2) };
            var qry = lst.AsQueryable();

            // Act
            var testValue = lst[0].ToString(CultureInfo.InvariantCulture);
            var result1 = qry.Where("it = @0", testValue);
            var result2 = qry.Where("@0 = it", testValue);

            // Assert
            Assert.Equal(lst[0], result1.Single());
            Assert.Equal(lst[0], result2.Single());
        }

        [Fact]
        public void ExpressionTests_DecimalQualifiers()
        {
            // Arrange
            var values = new[] { 1m, 2M, 3M }.AsQueryable();
            var resultValues = new[] { 2m, 3m }.AsQueryable();

            // Act
            var result1 = values.Where("it == 2M or it == 3m");
            var result2 = values.Where("it == 2.0M or it == 3.00m");

            // Assert
            Assert.Equal(resultValues.ToArray(), result1.ToArray());
            Assert.Equal(resultValues.ToArray(), result2.ToArray());
        }

        [Fact]
        public void ExpressionTests_DecimalQualifiers_Negative()
        {
            // Arrange
            var values = new[] { -1m, -2M, -3M }.AsQueryable();
            var resultValues = new[] { -2m, -3m }.AsQueryable();

            // Act
            var result1 = values.Where("it == -2M or it == -3m");
            var result2 = values.Where("it == -2.0M or it == -3.0m");

            // Assert
            Assert.Equal(resultValues.ToArray(), result1.ToArray());
            Assert.Equal(resultValues.ToArray(), result2.ToArray());
        }


        [Fact]
        public void ExpressionTests_DistinctBy()
        {
            // Arrange
            //Makes a Distinct By Tuple.Item1 but returns a full Tuple
            var lst = new List<Tuple<int, int, int>>
            {
                new Tuple<int, int, int>(1, 1, 1),
                new Tuple<int, int, int>(1, 1, 2),
                new Tuple<int, int, int>(1, 1, 3),
                new Tuple<int, int, int>(2, 2, 4),
                new Tuple<int, int, int>(2, 2, 5),
                new Tuple<int, int, int>(2, 2, 6),
                new Tuple<int, int, int>(2, 3, 7)
            };

            var p = lst.AsQueryable() as IQueryable;
            var qry = p.GroupBy("Item1", "it").Select("it.Max(it.Item3)");


            // Act
            var qry1 = p.Where("@0.Any(it == parent.Item3)", qry);
            var qry2 = p.Where("@0.Any($ == ^.Item3)", qry);
            var qry3 = p.Where("@0.Any($ == ~.Item3)", qry);

            // Assert
            Assert.Equal(2, qry1.Count());
            Assert.Equal(2, qry2.Count());
            Assert.Equal(2, qry3.Count());
        }

        [Fact]
        public void ExpressionTests_Divide_Number()
        {
            // Arrange
            var values = new[] { -10, 20 }.AsQueryable();

            // Act
            var result = values.Select<int>("it / 10");
            var expected = values.Select(i => i / 10);

            // Assert
            Check.That(result).ContainsExactly(expected);
        }

        [Fact]
        public void ExpressionTests_Double()
        {
            // Arrange
            var values = new[] { 1d, 2D, 3d }.AsQueryable();
            var resultValues = new[] { 2d, 3d }.AsQueryable();

            // Act
            var result1 = values.Where("it == 2 or it == 3");
            var result2 = values.Where("it > 1.99");

            // Assert
            Assert.Equal(resultValues.ToArray(), result1.ToArray());
            Assert.Equal(resultValues.ToArray(), result2.ToArray());
        }

        [Fact]
        public void ExpressionTests_Double_Negative()
        {
            // Arrange
            var values = new[] { -1d, -2D, -3d }.AsQueryable();
            var resultValues = new[] { -2d, -3d }.AsQueryable();

            // Act
            var result1 = values.Where("it == -2 or it == -3");
            var result2 = values.Where("it == -2.00 or it == -3.0");

            // Assert
            Assert.Equal(resultValues.ToArray(), result1.ToArray());
            Assert.Equal(resultValues.ToArray(), result2.ToArray());
        }

        //[Fact]
        //public void ExpressionTests_Double2()
        //{
        //    GlobalConfig.NumberDecimalSeparator = ',';

        //    // Arrange
        //    var values = new[] { 1d, 2D, 3d }.AsQueryable();
        //    var resultValues = new[] { 2d, 3d }.AsQueryable();

        //    // Act
        //    var result1 = values.Where("it == 2 or it == 3");
        //    var result2 = values.Where("it > 1,99");

        //    // Assert
        //    Assert.Equal(resultValues.ToArray(), result1.ToArray());
        //    Assert.Equal(resultValues.ToArray(), result2.ToArray());

        //    GlobalConfig.NumberDecimalSeparator = default(char);
        //}

        [Fact]
        public void ExpressionTests_DoubleQualifiers()
        {
            // Arrange
            var values = new[] { 1d, 2D, 3d }.AsQueryable();
            var resultValues = new[] { 2d, 3d }.AsQueryable();

            // Act
            var result1 = values.Where("it == 2d or it == 3D");
            var result2 = values.Where("it == 2.00d or it == 3.0D");

            // Assert
            Assert.Equal(resultValues.ToArray(), result1.ToArray());
            Assert.Equal(resultValues.ToArray(), result2.ToArray());
        }

        [Fact]
        public void ExpressionTests_DoubleQualifiers_Negative()
        {
            // Arrange
            var values = new[] { -1d, -2D, -3d }.AsQueryable();
            var resultValues = new[] { -2d, -3d }.AsQueryable();

            // Act
            var result1 = values.Where("it == -2d or it == -3D");
            var result2 = values.Where("it == -2.00d or it == -3.0D");

            // Assert
            Assert.Equal(resultValues.ToArray(), result1.ToArray());
            Assert.Equal(resultValues.ToArray(), result2.ToArray());
        }

        [Fact]
        public void ExpressionTests_Enum_Cast_Int_To_Enum()
        {
            // Arrange
            var enumvalue = TestEnum.Var1;
            var qry = new List<SourceClass> { new SourceClass { A = (int)enumvalue } }.AsQueryable();

            // Act
            var result = qry.Select<TargetClass>("new(A)").ToArray();

            // Assert
            result.Should().ContainEquivalentOf(new TargetClass { A = enumvalue });
        }

        [Fact]
        public void ExpressionTests_Enum()
        {
            var config = new ParsingConfig();

            // Arrange
            var lst = new List<TestEnum> { TestEnum.Var1, TestEnum.Var2, TestEnum.Var3, TestEnum.Var4, TestEnum.Var5, TestEnum.Var6 };
            var qry = lst.AsQueryable();

            // Act
            var resultLessThan = qry.Where(config, "it < TestEnum.Var4");
            var resultLessThanEqual = qry.Where(config, "it <= TestEnum.Var4");

            var resultGreaterThan = qry.Where(config, "TestEnum.Var4 > it");
            var resultGreaterThanEqual = qry.Where(config, "TestEnum.Var4 >= it");

            var resultEqualItLeft = qry.Where(config, "it = Var5");
            var resultEqualItRight = qry.Where(config, "Var5 = it");

            var resultEqualEnumParamLeft = qry.Where("@0 = it", TestEnum.Var5);
            var resultEqualEnumParamRight = qry.Where("it = @0", TestEnum.Var5);

            var resultEqualIntParamLeft = qry.Where("@0 = it", 8);
            var resultEqualIntParamRight = qry.Where("it = @0", 8);

            var resultEqualStringParamRight = qry.Where(config, "it = @0", "Var5");
            var resultEqualStringParamLeft = qry.Where(config, "@0 = it", "Var5");

            var resultEqualStringMixedCaseParamLeft = qry.Where(config, "@0 = it", "vAR5");
            var resultEqualStringMixedCaseParamRight = qry.Where(config, "it = @0", "vAR5");


            // Assert
            Check.That(resultLessThan.ToArray()).ContainsExactly(TestEnum.Var1, TestEnum.Var2, TestEnum.Var3);
            Check.That(resultLessThanEqual.ToArray()).ContainsExactly(TestEnum.Var1, TestEnum.Var2, TestEnum.Var3, TestEnum.Var4);
            Check.That(resultGreaterThan.ToArray()).ContainsExactly(TestEnum.Var1, TestEnum.Var2, TestEnum.Var3);
            Check.That(resultGreaterThanEqual.ToArray()).ContainsExactly(TestEnum.Var1, TestEnum.Var2, TestEnum.Var3, TestEnum.Var4);

            Check.That(resultEqualItLeft.Single()).Equals(TestEnum.Var5);
            Check.That(resultEqualItRight.Single()).Equals(TestEnum.Var5);

            Check.That(resultEqualEnumParamLeft.Single()).Equals(TestEnum.Var5);
            Check.That(resultEqualEnumParamRight.Single()).Equals(TestEnum.Var5);

            Check.That(resultEqualIntParamLeft.Single()).Equals(TestEnum.Var5);
            Check.That(resultEqualIntParamRight.Single()).Equals(TestEnum.Var5);

            Check.That(resultEqualStringParamLeft.Single()).Equals(TestEnum.Var5);
            Check.That(resultEqualStringParamRight.Single()).Equals(TestEnum.Var5);

            Check.That(resultEqualStringMixedCaseParamLeft.Single()).Equals(TestEnum.Var5);
            Check.That(resultEqualStringMixedCaseParamRight.Single()).Equals(TestEnum.Var5);
        }

        [Fact]
        public void ExpressionTests_Enum_Property_Equality_Using_Argument()
        {
            // Arrange
            var qry = new List<TestEnumClass> { new TestEnumClass { B = TestEnum2.Var2 } }.AsQueryable();

            // Act
            var resultEqualEnumParamLeft = qry.Where("@0 == it.B", TestEnum2.Var2).ToDynamicArray();
            var resultEqualEnumParamRight = qry.Where("it.B == @0", TestEnum2.Var2).ToDynamicArray();

            var resultEqualIntParamLeft = qry.Where("@0 == it.B", 1).ToDynamicArray();
            var resultEqualIntParamRight = qry.Where("it.B == @0", 1).ToDynamicArray();

            // Assert
            Check.That(resultEqualEnumParamLeft.Single()).Equals(TestEnum2.Var2);
            Check.That(resultEqualEnumParamRight.Single()).Equals(TestEnum2.Var2);

            Check.That(resultEqualIntParamLeft.Single()).Equals(TestEnum2.Var2);
            Check.That(resultEqualIntParamRight.Single()).Equals(TestEnum2.Var2);
        }

        [Fact]
        public void ExpressionTests_Enum_Property_Equality_With_Integer_Inline()
        {
            // Arrange
            var qry = new List<TestEnumClass> { new TestEnumClass { E = TestEnumPublic.Var2 } }.AsQueryable();

            // Act
            var resultEqualIntParamLeft = qry.Where("1 == it.E").ToDynamicArray();
            var resultEqualIntParamRight = qry.Where("it.E == 1").ToDynamicArray();

            // Assert
            Check.That(resultEqualIntParamLeft.Single()).Equals(TestEnumPublic.Var2);
            Check.That(resultEqualIntParamRight.Single()).Equals(TestEnumPublic.Var2);
        }

        [Fact]
        public void ExpressionTests_Enum_Property_Equality_Using_PublicEnum_And_FullName_Inline()
        {
            // Arrange
            var qry = new List<TestEnumClass> { new TestEnumClass { E = TestEnumPublic.Var2 } }.AsQueryable();
            string enumType = typeof(TestEnumPublic).FullName;

            // Act
            var resultEqualEnumParamLeft = qry.Where($"{enumType}.Var2 == it.E").ToDynamicArray();
            var resultEqualEnumParamRight = qry.Where($"it.E == {enumType}.Var2").ToDynamicArray();

            // Assert
            Check.That(resultEqualEnumParamLeft.Single()).Equals(TestEnumPublic.Var2);
            Check.That(resultEqualEnumParamRight.Single()).Equals(TestEnumPublic.Var2);
        }

        [Fact]
        public void ExpressionTests_Enum_Property_Equality_Using_Enum_And_FullName_Inline()
        {
            // Arrange
            var qry = new List<TestEnumClass> { new TestEnumClass { B = TestEnum2.Var2 } }.AsQueryable();
            string enumType = typeof(TestEnum2).FullName;

            // Act
            var resultEqualEnumParamLeft = qry.Where($"{enumType}.Var2 == it.B").ToDynamicArray();
            var resultEqualEnumParamRight = qry.Where($"it.B == {enumType}.Var2").ToDynamicArray();

            // Assert
            Check.That(resultEqualEnumParamLeft.Single()).Equals(TestEnum2.Var2);
            Check.That(resultEqualEnumParamRight.Single()).Equals(TestEnum2.Var2);
        }

        [Fact]
        public void ExpressionTests_Enum_Property_Equality_Using_PublicEnum_Name_Inline()
        {
            // Arrange
            var qry = new List<TestEnumClass> { new TestEnumClass { E = TestEnumPublic.Var2 } }.AsQueryable();
            string enumType = typeof(TestEnumPublic).FullName;

            // Act
            var resultEqualEnumParamLeft = qry.Where($"{enumType}.Var2 == it.E").ToDynamicArray();
            var resultEqualEnumParamRight = qry.Where($"it.E == {enumType}.Var2").ToDynamicArray();

            // Assert
            Check.That(resultEqualEnumParamLeft.Single()).Equals(TestEnumPublic.Var2);
            Check.That(resultEqualEnumParamRight.Single()).Equals(TestEnumPublic.Var2);
        }

        [Fact]
        public void ExpressionTests_Enum_Property_Equality_Using_Enum_Name_Inline_Should_Throw_Exception()
        {
            // Arrange
            var config = new ParsingConfig
            {
                ResolveTypesBySimpleName = true
            };
            var qry = new List<TestEnumClass> { new TestEnumClass { B = TestEnum2.Var2 } }.AsQueryable();
            string enumType = typeof(TestEnum2).Name;

            // Act
            Action a = () => qry.Where(config, $"{enumType}.Var2 == it.B").ToDynamicArray();

            // Assert
            a.Should().Throw<Exception>();
        }

        [Fact]
        public void ExpressionTests_Enum_Property_Equality_Using_PublicEnum_Invalid_Inline_Should_Throw_ParseException()
        {
            // Arrange
            var qry = new List<TestEnumClass> { new TestEnumClass { E = TestEnumPublic.Var2 } }.AsQueryable();
            string enumType = "a.b.c";

            // Act
            Action a = () => qry.Where($"{enumType}.Var2 == it.E").ToDynamicArray();

            // Assert
            a.Should().Throw<ParseException>().WithMessage("Enum type 'b.c' not found");
        }

        [Fact]
        public void ExpressionTests_Enum_Property_Equality_Using_PublicEnum_InvalidValue_Inline_Should_Throw_ParseException()
        {
            // Arrange
            var qry = new List<TestEnumClass> { new TestEnumClass { E = TestEnumPublic.Var2 } }.AsQueryable();
            string enumType = typeof(TestEnumPublic).FullName;

            // Act
            Action a = () => qry.Where($"{enumType}.VarInvalid == it.E").ToDynamicArray();

            // Assert
            a.Should().Throw<ParseException>().WithMessage("Enum value 'VarInvalid' is not defined in enum type 'System.Linq.Dynamic.Core.Tests.TestEnumPublic'");
        }

        [Fact]
        public void ExpressionTests_Enum_Property_Equality_Using_PublicEnum_MissingValue_Inline_Should_Throw_ParseException()
        {
            // Arrange
            var qry = new List<TestEnumClass> { new TestEnumClass { E = TestEnumPublic.Var2 } }.AsQueryable();
            string enumType = typeof(TestEnumPublic).FullName;

            // Act
            Action a = () => qry.Where($"{enumType}. == it.E").ToDynamicArray();

            // Assert
            a.Should().Throw<ParseException>().WithMessage("Enum type 'System.Linq.Dynamic.Core.Tests.' not found");
        }

        [Fact]
        public void ExpressionTests_Enum_NullableProperty()
        {
            // Arrange
            var qry = new List<TestEnumClass> { new TestEnumClass { C = TestEnum2.Var2 } }.AsQueryable();

            // Act
            var resultEqualEnumParamLeft = qry.Where("@0 == it.C", TestEnum2.Var2).ToDynamicArray();
            var resultEqualEnumParamRight = qry.Where("it.C == @0", TestEnum2.Var2).ToDynamicArray();

            var resultEqualIntParamLeft = qry.Where("@0 == it.C", 1).ToDynamicArray();
            var resultEqualIntParamRight = qry.Where("it.C == @0", 1).ToDynamicArray();

            // Assert
            Check.That(resultEqualEnumParamLeft.Single()).Equals(TestEnum2.Var2);
            Check.That(resultEqualEnumParamRight.Single()).Equals(TestEnum2.Var2);

            Check.That(resultEqualIntParamLeft.Single()).Equals(TestEnum2.Var2);
            Check.That(resultEqualIntParamRight.Single()).Equals(TestEnum2.Var2);
        }

        [Fact]
        public void ExpressionTests_Enum_MoreTests()
        {
            var config = new ParsingConfig();

            // Arrange
            var lst = new List<TestEnum> { TestEnum.Var1, TestEnum.Var2, TestEnum.Var3, TestEnum.Var4, TestEnum.Var5, TestEnum.Var6 };
            var qry = lst.AsQueryable();

            // Act
            var r1 = qry.Any(config, "it < TestEnum.Var4");
            var r2 = qry.First(config, "TestEnum.Var4 > it");
            var r3 = qry.FirstOrDefault(config, "it = Var5");
            var r4 = qry.Last(config, "@0 = it", TestEnum.Var5);
            var r5 = qry.LastOrDefault("@0 = it", 8);
            var r6 = qry.Single(config, "it = @0", "Var5");
            var r7 = qry.SingleOrDefault(config, "@0 = it", "vAR5");
            var r8 = qry.SkipWhile(config, "it < TestEnum.Var4").FirstOrDefault();
            var r9 = qry.TakeWhile(config, "it < TestEnum.Var4").FirstOrDefault();
            var r10 = qry.GroupBy(config, "it").FirstOrDefault();
        }

        [Fact]
        public void ExpressionTests_Enum_Nullable()
        {
            // Act
            var result1a = new[] { TestEnum.Var1 }.AsQueryable().Where("it = @0", (TestEnum?)TestEnum.Var1);
            var result1b = new[] { TestEnum.Var1 }.AsQueryable().Where("@0 = it", (TestEnum?)TestEnum.Var1);
            var result2a = new[] { (TestEnum?)TestEnum.Var1, null }.AsQueryable().Where("it = @0", TestEnum.Var1);
            var result2b = new[] { (TestEnum?)TestEnum.Var1, null }.AsQueryable().Where("@0 = it", TestEnum.Var1);
            var result3a = new[] { (TestEnum?)TestEnum.Var1, null }.AsQueryable().Where("it = @0", (TestEnum?)TestEnum.Var1);
            var result3b = new[] { (TestEnum?)TestEnum.Var1, null }.AsQueryable().Where("@0 = it", (TestEnum?)TestEnum.Var1);

            var result10a = new[] { TestEnum.Var1 }.AsQueryable().Where("it = @0", "Var1");
            var result10b = new[] { TestEnum.Var1 }.AsQueryable().Where("@0 = it", "Var1");
            var result11a = new[] { (TestEnum?)TestEnum.Var1, null }.AsQueryable().Where("it = @0", "Var1");
            var result11b = new[] { (TestEnum?)TestEnum.Var1, null }.AsQueryable().Where("@0 = it", "Var1");

            // Assert
            Assert.Equal(TestEnum.Var1, result1a.Single());
            Assert.Equal(TestEnum.Var1, result1b.Single());
            Assert.Equal(TestEnum.Var1, result2a.Single());
            Assert.Equal(TestEnum.Var1, result2b.Single());
            Assert.Equal((TestEnum?)TestEnum.Var1, result3a.Single());
            Assert.Equal((TestEnum?)TestEnum.Var1, result3b.Single());

            Assert.Equal(TestEnum.Var1, result10a.Single());
            Assert.Equal(TestEnum.Var1, result10b.Single());
            Assert.Equal((TestEnum?)TestEnum.Var1, result11a.Single());
            Assert.Equal((TestEnum?)TestEnum.Var1, result11b.Single());
        }

        [Fact]
        public void ExpressionTests_FirstOrDefault()
        {
            // Arrange
            var testList = User.GenerateSampleModels(2);
            testList[0].Roles.Clear();

            var testListQry = testList.AsQueryable();

            // Act : find first user that has the role of admin
            var realSingleResult = testListQry.FirstOrDefault(x => x.Roles.FirstOrDefault(y => y.Name == "Admin") != null);
            var testSingleResult = testListQry.Where("Roles.FirstOrDefault(Name = \"Admin\") != null").FirstOrDefault();

            testList[1].Roles.Clear(); //remove roles so the next set fails
            var realSingleFailResult = testListQry.FirstOrDefault(x => x.Roles.FirstOrDefault(y => y.Name == "Admin") != null);
            var testSingleFailResult = testListQry.Where("Roles.FirstOrDefault(Name = \"Admin\") != null").FirstOrDefault();

            // Assert
            Assert.Equal(realSingleResult, testSingleResult);
            Assert.Equal(realSingleFailResult, (User)testSingleFailResult);
        }

        [Fact]
        public void ExpressionTests_FloatQualifiers()
        {
            // Arrange
            var values = new[] { 1f, 2F, 3F }.AsQueryable();
            var resultValues = new[] { 2f, 3f }.AsQueryable();

            // Act
            var result1 = values.Where("it == 2F or it == 3f");
            var result2 = values.Where("it == 2.0F or it == 3.00f");

            // Assert
            Assert.Equal(resultValues.ToArray(), result1.ToArray());
            Assert.Equal(resultValues.ToArray(), result2.ToArray());
        }

        [Fact]
        public void ExpressionTests_FloatQualifiers_Negative()
        {
            // Arrange
            var values = new[] { -1f, -2F, -3F }.AsQueryable();
            var resultValues = new[] { -2f, -3f }.AsQueryable();

            // Act
            var result1 = values.Where("it == -2F or it == -3f");
            var result2 = values.Where("it == -2.0F or it == -3.0f");

            // Assert
            Assert.Equal(resultValues.ToArray(), result1.ToArray());
            Assert.Equal(resultValues.ToArray(), result2.ToArray());
        }

        [Fact]
        public void ExpressionTests_Guid_CompareTo_String()
        {
            var config = new ParsingConfig();
#if NETSTANDARD
            // config.CustomTypeProvider = new NetStandardCustomTypeProvider();
#endif

            // Arrange
            var lst = new List<Guid> { new Guid("{0A191E77-E32D-4DE1-8F1C-A144C2B0424D}"), Guid.NewGuid(), Guid.NewGuid() };
            var qry = lst.AsQueryable();

            // Act
            string testValue = lst[0].ToString();
            var result1a = qry.Where(config, "it = \"0A191E77-E32D-4DE1-8F1C-A144C2B0424D\"");
            var result1b = qry.Where(config, "it = @0", testValue);

            var result2a = qry.Where(config, "\"0A191E77-E32D-4DE1-8F1C-A144C2B0424D\" = it");
            var result2b = qry.Where(config, "@0 = it", testValue);

            // Assert
            Assert.Equal(lst[0], result1a.Single());
            Assert.Equal(lst[0], result1b.Single());
            Assert.Equal(lst[0], result2a.Single());
            Assert.Equal(lst[0], result2b.Single());
        }

        [Fact]
        public void ExpressionTests_Guid_CompareTo_Guid()
        {
            // Arrange
            var lst = new List<Guid> { new Guid("{0A191E77-E32D-4DE1-8F1C-A144C2B0424D}"), Guid.NewGuid(), Guid.NewGuid() };
            var qry = lst.AsQueryable();

            // Act
            Guid testValue = lst[0];
            var resulta = qry.Where("it = @0", testValue);
            var resultb = qry.Where("@0 = it", testValue);

            // Assert
            Assert.Equal(testValue, resulta.Single());
            Assert.Equal(testValue, resultb.Single());
        }

        [Fact]
        public void ExpressionTests_GuidNullable_CompareTo_Guid()
        {
            // Arrange
            var lst = new List<Guid?> { new Guid("{0A191E77-E32D-4DE1-8F1C-A144C2B0424D}"), Guid.NewGuid(), Guid.NewGuid() };
            var qry = lst.AsQueryable();

            // Act
            Guid testValue = new Guid("{0A191E77-E32D-4DE1-8F1C-A144C2B0424D}");
            var resulta = qry.Where("it = @0", testValue);
            var resultb = qry.Where("@0 = it", testValue);

            // Assert
            Assert.Equal(testValue, resulta.Single());
            Assert.Equal(testValue, resultb.Single());
        }

        [Fact]
        public void ExpressionTests_GuidNullable_CompareTo_GuidNullable()
        {
            // Arrange
            var lst = new List<Guid?> { new Guid("{0A191E77-E32D-4DE1-8F1C-A144C2B0424D}"), Guid.NewGuid(), Guid.NewGuid() };
            var qry = lst.AsQueryable();

            // Act
            Guid? testValue = lst[0];
            var resulta = qry.Where("it = @0", testValue);
            var resultb = qry.Where("@0 = it", testValue);

            // Assert
            Assert.Equal(testValue, resulta.Single());
            Assert.Equal(testValue, resultb.Single());
        }

        [Fact]
        public void ExpressionTests_Guid_CompareTo_Null()
        {
            // Arrange
            var lst = new List<TestGuidNullClass> { new TestGuidNullClass { GuidNull = null, Id = 1 }, new TestGuidNullClass { GuidNull = new Guid("{0A191E77-E32D-4DE1-8F1C-A144C2B0424D}"), Id = 2 } };
            var qry = lst.AsQueryable();

            // Act
            var result2a = qry.FirstOrDefault("it.GuidNull = null");
            var result2b = qry.FirstOrDefault("null = it.GuidNull");
            var result1a = qry.FirstOrDefault("it.GuidNull = @0", new object[] { null });
            var result1b = qry.FirstOrDefault("@0 = it.GuidNull", new object[] { null });

            // Assert
            Assert.Equal(1, result1a.Id);
            Assert.Equal(1, result1b.Id);
            Assert.Equal(1, result2a.Id);
            Assert.Equal(1, result2b.Id);
        }

        [Fact]
        public void ExpressionTests_HexadecimalInteger()
        {
            // Arrange
            var values = new[] { 1, 2, 3 };

            // Act
            var result = values.AsQueryable().Select("it * 0x1000abEF").Cast<int>();
            var resultNeg = values.AsQueryable().Select("it * -0xaBcDeF").Cast<int>();
            var resultU = values.AsQueryable().Select("uint(it) * 0x12345678U").Cast<uint>();
            var resultL = values.AsQueryable().Select("it * 0x1234567890abcdefL").Cast<long>();
            var resultLNeg = values.AsQueryable().Select("it * -0x0ABCDEF987654321L").Cast<long>();
            var resultUL = values.AsQueryable().Select("ulong(it) * 0x1000abEFUL").Cast<ulong>();

            // Assert
            Assert.Equal(values.Select(it => it * 0x1000abEF), result);
            Assert.Equal(values.Select(it => it * -0xaBcDeF), resultNeg);
            Assert.Equal(values.Select(it => (uint)it * 0x12345678U), resultU);
            Assert.Equal(values.Select(it => it * 0x1234567890abcdefL), resultL);
            Assert.Equal(values.Select(it => it * -0x0ABCDEF987654321L), resultLNeg);
            Assert.Equal(values.Select(it => (ulong)it * 0x1000abEFUL), resultUL);

            Assert.Throws<ParseException>(() => values.AsQueryable().Where("it < 0x 11a"));
            Assert.Throws<ParseException>(() => values.AsQueryable().Where("it < 11a"));
        }

        [Fact]
        public void ExpressionTests_In_Enum()
        {
            var config = new ParsingConfig();
#if NETSTANDARD
            // config.CustomTypeProvider = new NetStandardCustomTypeProvider();
#endif
            // Arrange
            var model1 = new ModelWithEnum { TestEnum = TestEnum.Var1 };
            var model2 = new ModelWithEnum { TestEnum = TestEnum.Var2 };
            var model3 = new ModelWithEnum { TestEnum = TestEnum.Var3 };
            var qry = new[] { model1, model2, model3 }.AsQueryable();

            // Act
            var expected = qry.Where(x => new[] { TestEnum.Var1, TestEnum.Var2 }.Contains(x.TestEnum)).ToArray();
            var result1 = qry.Where("it.TestEnum in (\"Var1\", \"Var2\")", config).ToArray();
            var result2 = qry.Where("it.TestEnum in (0, 1)", config).ToArray();

            // Assert
            Check.That(result1).ContainsExactly(expected);
            Check.That(result2).ContainsExactly(expected);
        }

        [Fact]
        public void ExpressionTests_In_Short()
        {
            // Arrange
            var qry = new short[] { 1, 2, 3, 4, 5, 6, 7 }.AsQueryable();

            // Act
            var result = qry.Where("it in (1, 2)");
            var resultExpected = qry.Where(x => x == 1 || x == 2);

            // Assert
            Check.That(resultExpected.ToArray()).ContainsExactly(result.ToDynamicArray<short>());
        }

        [Fact]
        public void ExpressionTests_In_String()
        {
            // Arrange
            var testRange = Enumerable.Range(1, 100).ToArray();
            var testModels = User.GenerateSampleModels(10);
            var testModelByUsername = string.Format("Username in (\"{0}\",\"{1}\",\"{2}\")", testModels[0].UserName, testModels[1].UserName, testModels[2].UserName);
            var testInExpression = new[] { 2, 4, 6, 8 };

            // Act
            var result1a = testRange.AsQueryable().Where("it in (2,4,6,8)").ToArray();
            var result1b = testRange.AsQueryable().Where("it in (2, 4,  6, 8)").ToArray();
            // https://github.com/NArnott/System.Linq.Dynamic/issues/52
            var result2 = testModels.AsQueryable().Where(testModelByUsername).ToArray();
            var result3 =
                testModels.AsQueryable()
                    .Where("Id in (@0, @1, @2)", testModels[0].Id, testModels[1].Id, testModels[2].Id)
                    .ToArray();
            var result4 = testRange.AsQueryable().Where("it in @0", testInExpression).ToArray();

            // Assert
            Assert.Equal(new[] { 2, 4, 6, 8 }, result1a);
            Assert.Equal(new[] { 2, 4, 6, 8 }, result1b);
            Assert.Equal(testModels.Take(3).ToArray(), result2);
            Assert.Equal(testModels.Take(3).ToArray(), result3);
            Assert.Equal(new[] { 2, 4, 6, 8 }, result4);
        }

        [Fact]
        public void ExpressionTests_IsNull_Simple()
        {
            // Arrange
            var baseQuery = new int?[] { 1, 2, null, 3, 4 }.AsQueryable();
            var expectedResult = new[] { 1, 2, 0, 3, 4 };

            // Act
            var result1 = baseQuery.Select("isnull(it, 0)");

            // Assert
            Assert.Equal(expectedResult, result1.ToDynamicArray<int>());
        }

        [Fact]
        public void ExpressionTests_IsNull_Complex()
        {
            // Arrange
            var testModels = User.GenerateSampleModels(3, true);
            testModels[0].NullableInt = null;
            testModels[1].NullableInt = null;
            testModels[2].NullableInt = 5;

            var expectedResult1 = testModels.AsQueryable().Select(u => new { UserName = u.UserName, X = u.NullableInt ?? (3 * u.Income) }).Cast<object>().ToArray();
            var expectedResult2 = testModels.AsQueryable().Where(u => (u.NullableInt ?? 10) == 10).ToArray();
            var expectedResult3 = testModels.Select(m => m.NullableInt ?? 10).ToArray();

            // Act
            var result1 = testModels.AsQueryable().Select("new (UserName, isnull(NullableInt, (3 * Income)) as X)");
            var result2 = testModels.AsQueryable().Where("isnull(NullableInt, 10) == 10");
            var result3a = testModels.AsQueryable().Select("isnull(NullableInt, @0)", 10);
            var result3b = testModels.AsQueryable().Select<int>("isnull(NullableInt, @0)", 10);

            // Assert
            Assert.Equal(expectedResult1.ToString(), result1.ToDynamicArray().ToString());
            Assert.Equal(expectedResult2, result2.ToArray());
            Assert.Equal(expectedResult3, result3a.ToDynamicArray<int>());
            Assert.Equal(expectedResult3, result3b.ToDynamicArray<int>());
        }

        [Fact]
        public void ExpressionTests_IsNull_ThrowsException()
        {
            // Arrange
            var baseQuery = new int?[] { 1, 2, null, 3, 4 }.AsQueryable();

            // Act + Assert
            Check.ThatCode(() => baseQuery.Select("isnull(it, 0, 4)")).Throws<ParseException>();
        }

        [Fact]
        public void ExpressionTests_Indexer_Issue57()
        {
            var rows = new List<JObject>
            {
                new JObject {["Column1"] = "B", ["Column2"] = 1},
                new JObject {["Column1"] = "B", ["Column2"] = 2},
                new JObject {["Column1"] = "A", ["Column2"] = 1},
                new JObject {["Column1"] = "A", ["Column2"] = 2}
            };

            var expected = rows.OrderBy(x => x["Column1"]).ToList();
            var result = rows.AsQueryable().OrderBy(@"it[""Column1""]").ToList();

            Assert.Equal(expected, result);
        }

        // [Fact]
        // public void ExpressionTests_IComparable_GreaterThan()
        // {
        //     // Assign
        //     const string id = "111111111111111111111111";
        //     var queryable = new[] { new TestObjectIdClass { Id = 1, ObjectId = id }, new TestObjectIdClass { Id = 2, ObjectId = "221111111111111111111111" } }.AsQueryable();

        //     // Act
        //     var result = queryable.Where(x => x.ObjectId > id).ToArray();
        //     var dynamicResult = queryable.Where("it.ObjectId > @0", id).ToArray();

        //     // Assert
        //     Check.That(dynamicResult).ContainsExactly(result);
        // }

        // [Fact]
        // public void ExpressionTests_IEquatable_Equal()
        // {
        //     // Assign
        //     const string id = "111111111111111111111111";
        //     var queryable = new[] { new TestObjectIdClass { Id = 1, ObjectId = id }, new TestObjectIdClass { Id = 2, ObjectId = ObjectId.Parse("221111111111111111111111") } }.AsQueryable();

        //     // Act
        //     var result = queryable.First(x => x.ObjectId == ObjectId.Parse(id));
        //     var dynamicResult = queryable.First("it.ObjectId == @0", ObjectId.Parse(id));

        //     // Assert
        //     Check.That(dynamicResult.Id).Equals(result.Id);
        // }

        // [Fact]
        // public void ExpressionTests_IEquatable_NotEqual()
        // {
        //     // Assign
        //     const string id = "111111111111111111111111";
        //     var queryable = new[] { new TestObjectIdClass { Id = 1, ObjectId = ObjectId.Parse(id) }, new TestObjectIdClass { Id = 2, ObjectId = ObjectId.Parse("221111111111111111111111") } }.AsQueryable();

        //     // Act
        //     var result = queryable.First(x => x.ObjectId != ObjectId.Parse(id));
        //     var dynamicResult = queryable.First("it.ObjectId != @0", ObjectId.Parse(id));

        //     // Assert
        //     Check.That(dynamicResult.Id).Equals(result.Id);
        // }

        [Fact]
        public void ExpressionTests_LogicalAndOr()
        {
            // Arrange
            var lst = new List<int> { 0x20, 0x21, 0x30, 0x31, 0x41 };
            var qry = lst.AsQueryable();

            // Act
            var result1 = qry.Where("(it & 1) > 0");
            var result2 = qry.Where("(it & 32) > 0");

            // Assert
            Assert.Equal(new[] { 0x21, 0x31, 0x41 }, result1.ToArray());
            Assert.Equal(qry.Where(x => (x & 32) > 0).ToArray(), result2.ToArray());
        }

        [Fact]
        public void ExpressionTests_Method_NoParams()
        {
            // Arrange
            var users = User.GenerateSampleModels(3);

            // Act
            var expected = users.Where(u => u.TestMethod1());
            var result = users.AsQueryable().Where("TestMethod1()");

            // Assert
            Assert.Equal(expected.Count(), result.Count());
        }

        [Fact]
        public void ExpressionTests_Method_OneParam_With_it()
        {
            // Arrange
            var users = User.GenerateSampleModels(3);

            // Act
            var expected = users.Where(u => u.TestMethod2(u));
            var result = users.AsQueryable().Where("TestMethod2(it)");

            // Assert
            Assert.Equal(expected.Count(), result.Count());
        }

        public class DefaultDynamicLinqCustomTypeProviderForStaticTesting : CustomTypeProviders.DefaultDynamicLinqCustomTypeProvider
        {
            public override HashSet<Type> GetCustomTypes() => new HashSet<Type>(base.GetCustomTypes()) { typeof(Methods), typeof(MethodsItemExtension) };
        }

        [Fact]
        public void ExpressionTests_MethodCall_GenericStatic()
        {
            var config = new ParsingConfig
            {
                CustomTypeProvider = new DefaultDynamicLinqCustomTypeProviderForStaticTesting()
            };

            // Arrange
            var list = new[] { 0, 1, 2, 3, 4 }.Select(value => new Methods.Item { Value = value }).ToArray();

            // Act
            var expectedResult = list.Where(x => Methods.StaticGenericMethod(x));
            var result = list.AsQueryable().Where(config, "Methods.StaticGenericMethod(it)");

            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
        }

        [Fact]
        public void ExpressionTests_MethodCall_Generic()
        {
            var config = new ParsingConfig
            {
                CustomTypeProvider = new DefaultDynamicLinqCustomTypeProviderForStaticTesting()
            };

            // Arrange
            var list = new[] { 0, 1, 2, 3, 4 }.Select(value => new Methods.Item { Value = value }).ToArray();

            // Act
            var methods = new Methods();
            var expectedResult = list.Where(x => methods.GenericMethod(x));
            var result = list.AsQueryable().Where("@0.GenericMethod(it)", methods);

            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
        }

        [Fact]
        public void ExpressionTests_MethodCall_GenericExtension()
        {
            var config = new ParsingConfig
            {
                CustomTypeProvider = new DefaultDynamicLinqCustomTypeProviderForStaticTesting()
            };

            // Arrange
            var list = new[] { 0, 1, 2, 3, 4 }.Select(value => new Methods.Item { Value = value }).ToArray();

            // Act
            var methods = new Methods();
            var expectedResult = list.Where(x => MethodsItemExtension.Functions.EfCoreCollate(x.Value, "tlh-KX") == 2);
            var result = list.AsQueryable().Where(config, "MethodsItemExtension.Functions.EfCoreCollate(it.Value,\"tlh-KX\")==2");

            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
        }

        [Fact]
        public void ExpressionTests_MethodCall_ValueTypeToValueTypeParameter()
        {
            // Arrange
            var list = new[] { 0, 1, 2, 3, 4 };

            // Act
            var methods = new Methods();
            var expectedResult = list.Where(x => methods.Method1(x));
            var result = list.AsQueryable().Where("@0.Method1(it)", methods);

            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
        }

        [Fact]
        public void ExpressionTests_MethodCall_ValueTypeToObjectParameterWithCast()
        {
            // Arrange
            var list = new[] { 0, 1, 2, 3, 4 };

            // Act
            var methods = new Methods();
            var expectedResult = list.Where(x => methods.Method2(x));
            var result = list.AsQueryable().Where("@0.Method2(object(it))", methods);

            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
        }

        [Fact]
        public void ExpressionTests_MethodCall_ValueTypeToObjectParameterWithoutCast()
        {
            // Arrange
            var list = new[] { 0, 1, 2, 3, 4 };

            // Act
            var methods = new Methods();
            var expectedResult = list.Where(x => methods.Method2(x));
            var result = list.AsQueryable().Where("@0.Method2(it)", methods);

            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
        }

        [Fact]
        public void ExpressionTests_MethodCall_NullableValueTypeToObjectParameter()
        {
            // Arrange
            var list = new int?[] { 0, 1, 2, 3, 4, null };

            // Act
            var methods = new Methods();
            var expectedResult = list.Where(x => methods.Method2(x));
            var result = list.AsQueryable().Where("@0.Method2(it)", methods);

            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
        }

        [Fact]
        public void ExpressionTests_MethodCall_ReferenceTypeToObjectParameter()
        {
            // Arrange
            var list = new[] { 0, 1, 2, 3, 4 }.Select(value => new Methods.Item { Value = value }).ToArray();

            // Act
            var methods = new Methods();
            var expectedResult = list.Where(x => methods.Method3(x));
            var result = list.AsQueryable().Where("@0.Method3(it)", methods);

            // Assert
            Assert.Equal(expectedResult.Count(), result.Count());
        }

        [Fact]
        public void ExpressionTests_NewAnonymousType_Paren()
        {
            // Arrange
            var users = User.GenerateSampleModels(1);

            // Act
            var expectedResult = users.Select(x => new { x.Id, I = x.Income }).FirstOrDefault();
            var result = users.AsQueryable().Select("new (Id, Income as I)").FirstOrDefault();

            // Assert
            Check.That(result).Equals(expectedResult);
        }

        [Fact]
        public void ExpressionTests_NewAnonymousType_CurlyParen()
        {
            // Arrange
            var users = User.GenerateSampleModels(1);

            // Act
            var expectedResult = users.Select(x => new { x.Id, I = x.Income }).FirstOrDefault();
            var result = users.AsQueryable().Select("new { Id, Income as I }").FirstOrDefault();

            // Assert
            Check.That(result).Equals(expectedResult);
        }

        [Fact]
        public void ExpressionTests_Method_OneParam_With_user()
        {
            // Arrange
            var users = User.GenerateSampleModels(10);
            var testUser = users[2];

            // Act
            var expected = users.Where(u => u.TestMethod3(testUser));
            var result = users.AsQueryable().Where("TestMethod3(@0)", testUser);

            // Assert
            Assert.Equal(expected.Count(), result.Count());
        }

        [Fact]
        public void ExpressionTests_Modulo_Number()
        {
            // Arrange
            var values = new[] { -10, 20 }.AsQueryable();

            // Act
            var result = values.Select<int>("it % 3");
            var expected = values.Select(i => i % 3);

            // Assert
            Check.That(result).ContainsExactly(expected);
        }

        [Fact]
        public void ExpressionTests_Multiply_Number()
        {
            // Arrange
            var values = new[] { -1, 2 }.AsQueryable();

            // Act
            var result = values.Select<int>("it * 10");
            var expected = values.Select(i => i * 10);

            // Assert
            Check.That(result).ContainsExactly(expected);
        }

        [Fact]
        public void ExpressionTests_NullCoalescing()
        {
            // Arrange
            var testModels = User.GenerateSampleModels(3, true);
            testModels[0].NullableInt = null;
            testModels[1].NullableInt = null;
            testModels[2].NullableInt = 5;

            var expectedResult1 = testModels.AsQueryable().Select(u => new { UserName = u.UserName, X = u.NullableInt ?? (3 * u.Income) }).Cast<object>().ToArray();
            var expectedResult2 = testModels.AsQueryable().Where(u => (u.NullableInt ?? 10) == 10).ToArray();
            var expectedResult3 = testModels.Select(m => m.NullableInt ?? 10).ToArray();

            // Act
            var result1 = testModels.AsQueryable().Select("new (UserName, NullableInt ?? (3 * Income) as X)");
            var result2 = testModels.AsQueryable().Where("(NullableInt ?? 10) == 10");
            var result3a = testModels.AsQueryable().Select("NullableInt ?? @0", 10);
            var result3b = testModels.AsQueryable().Select<int>("NullableInt ?? @0", 10);

            // Assert
            Assert.Equal(expectedResult1.ToString(), result1.ToDynamicArray().ToString());
            Assert.Equal(expectedResult2, result2.ToDynamicArray<User>());
            Assert.Equal(expectedResult3, result3a.ToDynamicArray<int>());
            Assert.Equal(expectedResult3, result3b.ToDynamicArray<int>());
        }

        [Theory]
        [InlineData("np(str)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.str != null)), Param_0.str, null))")]
        [InlineData("np(str, \"x\")", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.str != null)), Param_0.str, \"x\"))")]
        [InlineData("np(strNull)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.strNull != null)), Param_0.strNull, null))")]
        [InlineData("np(strNull, \"x\")", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.strNull != null)), Param_0.strNull, \"x\"))")]
        [InlineData("np(gnullable)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.gnullable != null)), Param_0.gnullable, null))")]
        [InlineData("np(dtnullable)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.dtnullable != null)), Param_0.dtnullable, null))")]
        [InlineData("np(number, 42)", "Select(Param_0 => IIF((Param_0 != null), Param_0.number, 42))")]
        [InlineData("np(nullablenumber)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.nullablenumber != null)), Param_0.nullablenumber, null))")]
        [InlineData("np(_enumnullable)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0._enumnullable != null)), Param_0._enumnullable, null))")]

#if NET452
        [InlineData("np(dt)", "Select(Param_0 => IIF((Param_0 != null), Convert(Param_0.dt), null))")]
        [InlineData("np(_enum)", "Select(Param_0 => IIF((Param_0 != null), Convert(Param_0._enum), null))")]
        [InlineData("np(g)", "Select(Param_0 => IIF((Param_0 != null), Convert(Param_0.g), null))")]
        [InlineData("np(number)", "Select(Param_0 => IIF((Param_0 != null), Convert(Param_0.number), null))")]
        [InlineData("np(nested.g)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.nested != null)), Convert(Param_0.nested.g), null))")]
        [InlineData("np(nested.dt)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.nested != null)), Convert(Param_0.nested.dt), null))")]
        [InlineData("np(nested.number)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.nested != null)), Convert(Param_0.nested.number), null))")]
        [InlineData("np(nested._enum)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.nested != null)), Convert(Param_0.nested._enum), null))")]
        [InlineData("np(item.Id)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.item != null)), Convert(Param_0.item.Id), null))")]
        [InlineData("np(item.GuidNormal)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.item != null)), Convert(Param_0.item.GuidNormal), null))")]
        [InlineData("np(nested.dtnullable.Value.Year)", "Select(Param_0 => IIF((((Param_0 != null) AndAlso (Param_0.nested != null)) AndAlso (Param_0.nested.dtnullable != null)), Convert(Param_0.nested.dtnullable.Value.Year), null))")]
#else
        [InlineData("np(dt)", "Select(Param_0 => IIF((Param_0 != null), Convert(Param_0.dt, Nullable`1), null))")]
        [InlineData("np(_enum)", "Select(Param_0 => IIF((Param_0 != null), Convert(Param_0._enum, Nullable`1), null))")]
        [InlineData("np(g)", "Select(Param_0 => IIF((Param_0 != null), Convert(Param_0.g, Nullable`1), null))")]
        [InlineData("np(number)", "Select(Param_0 => IIF((Param_0 != null), Convert(Param_0.number, Nullable`1), null))")]
        [InlineData("np(nested.g)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.nested != null)), Convert(Param_0.nested.g, Nullable`1), null))")]
        [InlineData("np(nested.dt)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.nested != null)), Convert(Param_0.nested.dt, Nullable`1), null))")]
        [InlineData("np(nested.number)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.nested != null)), Convert(Param_0.nested.number, Nullable`1), null))")]
        [InlineData("np(nested.number, 42)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.nested != null)), Param_0.nested.number, 42))")]
        [InlineData("np(nested._enum)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.nested != null)), Convert(Param_0.nested._enum, Nullable`1), null))")]
        [InlineData("np(item.Id)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.item != null)), Convert(Param_0.item.Id, Nullable`1), null))")]
        [InlineData("np(item.GuidNormal)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.item != null)), Convert(Param_0.item.GuidNormal, Nullable`1), null))")]
        [InlineData("np(nested.dtnullable.Value.Year)", "Select(Param_0 => IIF((((Param_0 != null) AndAlso (Param_0.nested != null)) AndAlso (Param_0.nested.dtnullable != null)), Convert(Param_0.nested.dtnullable.Value.Year, Nullable`1), null))")]
#endif
        [InlineData("np(nested.strNull)", "Select(Param_0 => IIF((((Param_0 != null) AndAlso (Param_0.nested != null)) AndAlso (Param_0.nested.strNull != null)), Param_0.nested.strNull, null))")]
        [InlineData("np(nested.strNull, \"x\")", "Select(Param_0 => IIF((((Param_0 != null) AndAlso (Param_0.nested != null)) AndAlso (Param_0.nested.strNull != null)), Param_0.nested.strNull, \"x\"))")]
        [InlineData("np(nested.gnullable)", "Select(Param_0 => IIF((((Param_0 != null) AndAlso (Param_0.nested != null)) AndAlso (Param_0.nested.gnullable != null)), Param_0.nested.gnullable, null))")]
        [InlineData("np(nested.dtnullable)", "Select(Param_0 => IIF((((Param_0 != null) AndAlso (Param_0.nested != null)) AndAlso (Param_0.nested.dtnullable != null)), Param_0.nested.dtnullable, null))")]
        [InlineData("np(nested.nullablenumber)", "Select(Param_0 => IIF((((Param_0 != null) AndAlso (Param_0.nested != null)) AndAlso (Param_0.nested.nullablenumber != null)), Param_0.nested.nullablenumber, null))")]
        [InlineData("np(nested.nullablenumber, 42)", "Select(Param_0 => IIF((((Param_0 != null) AndAlso (Param_0.nested != null)) AndAlso (Param_0.nested.nullablenumber != null)), Param_0.nested.nullablenumber, 42))")]
        [InlineData("np(nested._enumnullable)", "Select(Param_0 => IIF((((Param_0 != null) AndAlso (Param_0.nested != null)) AndAlso (Param_0.nested._enumnullable != null)), Param_0.nested._enumnullable, null))")]
        [InlineData("np(item.GuidNull)", "Select(Param_0 => IIF((((Param_0 != null) AndAlso (Param_0.item != null)) AndAlso (Param_0.item.GuidNull != null)), Param_0.item.GuidNull, null))")]
        [InlineData("np(items.FirstOrDefault())", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.items != null)), Param_0.items.FirstOrDefault(), null))")]
        [InlineData("np(items.FirstOrDefault(it != \"x\"))", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.items != null)), Param_0.items.FirstOrDefault(Param_1 => (Param_1 != \"x\")), null))")]
        public void ExpressionTests_NullPropagating(string test, string query)
        {
            // Arrange
            var q = new[]
            {
                new
                {
                    g = Guid.NewGuid(),
                    gnullable = (Guid?) Guid.NewGuid(),
                    dt = DateTime.Now,
                    dtnullable = (DateTime?) DateTime.Now,
                    _enum = TestEnum2.Var1,
                    _enumnullable = (TestEnum2?) TestEnum2.Var2,
                    number = 1,
                    nullablenumber = (int?) 2,
                    str = "str",
                    strNull = (string) null,
                    nested = new
                    {
                        g = Guid.NewGuid(),
                        gnullable = (Guid?) Guid.NewGuid(),
                        dt = DateTime.Now,
                        dtnullable = (DateTime?) DateTime.Now,
                        _enum = TestEnum2.Var1,
                        _enumnullable = (TestEnum2?) TestEnum2.Var2,
                        number = 1,
                        nullablenumber = (int?) 2,
                        str = "str",
                        strNull = (string) null
                    },
                    item = new TestGuidNullClass
                    {
                        Id = 100,
                        GuidNormal = Guid.NewGuid(),
                        GuidNull = Guid.NewGuid()
                    },
                    items = new [] { "a", "b" }
                }
            }.AsQueryable();

            // Act
            var resultDynamic = q.Select(test);

            // Assert
            string queryAsString = resultDynamic.ToString();
            queryAsString = queryAsString.Substring(queryAsString.IndexOf(".Select") + 1).TrimEnd(']');
            Check.That(queryAsString).Equals(query);
        }

        [Theory]
        [InlineData("np(number)", "Select(Param_0 => IIF((Param_0 != null), Param_0.number, default(Int32)))")]
        [InlineData("np(number, 42)", "Select(Param_0 => IIF((Param_0 != null), Param_0.number, 42))")]
        [InlineData("np(nested.dtnullable.Value.Year)", "Select(Param_0 => IIF((((Param_0 != null) AndAlso (Param_0.nested != null)) AndAlso (Param_0.nested.dtnullable != null)), Param_0.nested.dtnullable.Value.Year, default(Int32)))")]
        [InlineData("np(nested.number)", "Select(Param_0 => IIF(((Param_0 != null) AndAlso (Param_0.nested != null)), Param_0.nested.number, default(Int32)))")]
        public void ExpressionTests_NullPropagating_Config_Has_UseDefault(string test, string query)
        {
            // Arrange
            var config = new ParsingConfig
            {
                NullPropagatingUseDefaultValueForNonNullableValueTypes = true
            };

            var q = new[]
            {
                new
                {
                    g = Guid.NewGuid(),
                    gnullable = (Guid?) Guid.NewGuid(),
                    dt = DateTime.Now,
                    dtnullable = (DateTime?) DateTime.Now,
                    _enum = TestEnum2.Var1,
                    _enumnullable = (TestEnum2?) TestEnum2.Var2,
                    number = 1,
                    nullablenumber = (int?) 2,
                    str = "str",
                    strNull = (string) null,
                    nested = new
                    {
                        g = Guid.NewGuid(),
                        gnullable = (Guid?) Guid.NewGuid(),
                        dt = DateTime.Now,
                        dtnullable = (DateTime?) DateTime.Now,
                        _enum = TestEnum2.Var1,
                        _enumnullable = (TestEnum2?) TestEnum2.Var2,
                        number = 1,
                        nullablenumber = (int?) 2,
                        str = "str",
                        strNull = (string) null
                    },
                    item = new TestGuidNullClass
                    {
                        Id = 100,
                        GuidNormal = Guid.NewGuid(),
                        GuidNull = Guid.NewGuid()
                    },
                    items = new [] { "a", "b" }
                }
            }.AsQueryable();

            // Act
            var resultDynamic = q.Select(config, test);

            // Assert
            string queryAsString = resultDynamic.ToString();
            queryAsString = queryAsString.Substring(queryAsString.IndexOf(".Select") + 1).TrimEnd(']');
            Check.That(queryAsString).Equals(query);
        }


        [Fact]
        public void ExpressionTests_NullPropagating_InstanceMethod_Zero_Arguments()
        {
            // Arrange 1
            var expression = "np(FooValue.Zero().Length)";
            var q = new[] { new Foo { FooValue = new Foo() } }.AsQueryable();

            // Act 2
            var result = q.Select(expression).FirstOrDefault() as int?;

            // Assert 2
            result.Should().BeNull();
        }

        [Fact]
        public void ExpressionTests_NullPropagating_InstanceMethod_One_Argument()
        {
            // Arrange
            var expression = "np(FooValue.One(1).Length)";
            var q = new[] { new Foo { FooValue = new Foo() } }.AsQueryable();

            // Act
            var result = q.Select(expression).FirstOrDefault() as int?;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ExpressionTests_NullPropagating_InstanceMethod_Two_Arguments()
        {
            // Arrange
            var expression = "np(FooValue.Two(1, 42).Length)";
            var q = new[] { new Foo { FooValue = new Foo() } }.AsQueryable();

            // Act
            var result = q.Select(expression).FirstOrDefault() as int?;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ExpressionTests_NullPropagation_Method()
        {
            // Arrange
            var users = new[] { new User { Roles = new List<Role>() } }.AsQueryable();

            // Act
            var resultDynamic = users.Select("np(Roles.FirstOrDefault(false).Name)").ToDynamicArray();

            // Assert
            Assert.True(resultDynamic[0] == null);
        }

        [Fact]
        public void ExpressionTests_NullPropagation_Method_WithDefaultValue()
        {
            // Arrange
            var defaultRoleName = "x";
            var users = new[] { new User { Roles = new List<Role>() } }.AsQueryable();

            // Act
            var resultDynamic = users.Select("np(Roles.FirstOrDefault(false).Name, @0)", defaultRoleName).ToDynamicArray();

            // Assert
            Assert.True(resultDynamic[0] == "x");
        }

        [Fact]
        public void ExpressionTests_NullPropagating_DateTime()
        {
            // Arrange
            var q = new[]
            {
                new { id = 1, date1 = (DateTime?) DateTime.Now, date2 = DateTime.Now.AddDays(-1) }
            }.AsQueryable();

            // Act
            var result = q.OrderBy(x => x.date2).Select(x => x.id).ToArray();
            var resultDynamic = q.OrderBy("np(date1)").Select("id").ToDynamicArray<int>();

            // Assert
            Check.That(resultDynamic).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_NullPropagation_NullableDateTime()
        {
            // Arrange
            var q = new[]
            {
                new { id = 1, date1 = (DateTime?) DateTime.Now, date2 = DateTime.Now.AddDays(-1)}
            }.AsQueryable();

            // Act
            var result = q.OrderBy(x => x.date1.Value.Year).Select(x => x.id).ToArray();
            var resultDynamic = q.OrderBy("np(date1.Value.Year)").Select("id").ToDynamicArray<int>();

            // Assert
            Check.That(resultDynamic).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_NullPropagating_Nested1Integer_WithoutDefaultValue()
        {
            // Arrange
            var testModels = User.GenerateSampleModels(2, true).ToList();
            testModels.Add(null); // Add null User
            testModels[0].NullableInt = null; // Set the NullableInt to null for first User

            // Act
            var result = testModels.AsQueryable().Select(t => t != null && t.NullableInt != null ? t.NullableInt : null).ToArray();
            var resultDynamic = testModels.AsQueryable().Select("np(it.NullableInt)").ToDynamicArray<int?>();

            // Assert
            Check.That(resultDynamic).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_NullPropagating_Nested3Integer_WithoutDefaultValue()
        {
            // Arrange
            var testModels = User.GenerateSampleModels(2, true).ToList();
            testModels.Add(null); // Add null User
            testModels[0].Profile = null; // Set the Profile to null for first User

            // Act
            var result = testModels.AsQueryable().Select(t => t != null && t.Profile != null && t.Profile.UserProfileDetails != null ? (long?)t.Profile.UserProfileDetails.Id : null).ToArray();
            var resultDynamic = testModels.AsQueryable().Select("np(it.Profile.UserProfileDetails.Id)").ToDynamicArray<long?>();

            // Assert
            Check.That(resultDynamic).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_NullPropagating_Nested3Integer_WithoutDefaultValue_Config_Has_UseDefault()
        {
            // Arrange
            var config = new ParsingConfig
            {
                NullPropagatingUseDefaultValueForNonNullableValueTypes = true
            };

            var testModels = User.GenerateSampleModels(2, true).ToList();
            testModels.Add(null); // Add null User
            testModels[0].Profile = null; // Set the Profile to null for first User

            // Act
            var result = testModels.AsQueryable().Select(t => t != null && t.Profile != null && t.Profile.UserProfileDetails != null ? t.Profile.UserProfileDetails.Id : default(long)).ToArray();
            var resultDynamic = testModels.AsQueryable().Select(config, "np(it.Profile.UserProfileDetails.Id)").ToDynamicArray<long>();

            // Assert
            Check.That(resultDynamic).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_NullPropagating_NullableInteger_WithDefaultValue()
        {
            // Arrange
            var testModels = User.GenerateSampleModels(2, true).ToList();

            // Act
            var result = testModels.AsQueryable().Select(t => t.NullableInt != null ? t.NullableInt : 100).ToArray();
            var resultDynamic = testModels.AsQueryable().Select("np(NullableInt, 100)").ToDynamicArray<int>();

            // Assert
            Check.That(resultDynamic).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_NullPropagating_String_WithDefaultValue()
        {
            // Arrange
            var testModels = User.GenerateSampleModels(2, true).ToList();

            // Act
            var result = testModels.AsQueryable().Select(t => t.UserName != null ? t.UserName : "x").ToArray();
            var resultDynamic = testModels.AsQueryable().Select("np(UserName, \"x\")").ToDynamicArray<string>();

            // Assert
            Check.That(resultDynamic).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_NullPropagatingNested1_WithDefaultValue()
        {
            // Arrange
            var testModels = User.GenerateSampleModels(2, true).ToList();
            testModels.Add(null); // Add null User
            testModels[0].UserName = null; // Set the UserName to null for first User

            // Act
            var result = testModels.AsQueryable().Select(t => t != null && t.UserName != null ? t.UserName : "x").ToArray();
            var resultDynamic = testModels.AsQueryable().Select("np(it.UserName, \"x\")").ToDynamicArray<string>();

            // Assert
            Check.That(resultDynamic).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_NullPropagatingNested3_WithDefaultValue()
        {
            // Arrange
            var testModels = User.GenerateSampleModels(2, true).ToList();
            testModels.Add(null); // Add null User
            testModels[0].Profile = null; // Set the Profile to null for first User

            // Act
            var result = testModels.AsQueryable().Select(t => t != null && t.Profile != null && t.Profile.UserProfileDetails != null ? t.Profile.UserProfileDetails.Id : 100).ToArray();
            var resultDynamic = testModels.AsQueryable().Select("np(it.Profile.UserProfileDetails.Id, 100)").ToDynamicArray<long>();

            // Assert
            Check.That(resultDynamic).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_NullPropagation_ThrowsException()
        {
            // Arrange
            var q = User.GenerateSampleModels(1, true).AsQueryable();

            // Act
            Check.ThatCode(() => q.Select("np()")).Throws<ParseException>();
            Check.ThatCode(() => q.Select("np(it.Profile.UserProfileDetails.Id, 1, 2)")).Throws<ParseException>();
            Check.ThatCode(() => q.Select("np(1)")).Throws<ParseException>();
        }

        [Fact]
        public void ExpressionTests_OrElse()
        {
            // Arrange
            var users = new[] { new User { Income = 1 }, new User { Income = 2 }, new User { Income = 3 } }.AsQueryable();

            // Act
            var result = users.Where(u => u.Income == 1 || u.Income == 2).ToArray();
            var resultDynamic = users.Where("Income == 1 OrElse Income == 2").ToArray();

            // Assert
            Check.That(resultDynamic).ContainsExactly(result);
        }

        [Fact]
        public void ExpressionTests_OrderBy()
        {
            // Arrange
            var qry = User.GenerateSampleModels(100).AsQueryable();

            // Act
            var orderBy = qry.OrderBy("Id asc, Profile.Age desc");

            // Assert
            Check.That(qry.OrderBy(x => x.Id).ThenByDescending(x => x.Profile.Age).ToArray()).ContainsExactly(orderBy.ToArray());
        }

        [Fact]
        public void ExpressionTests_Select_DynamicObjects()
        {
            // Arrange
            dynamic a1 = new { Name = "a", BlogId = 100 };
            dynamic a2 = new { BlogId = 200, Name = "b" };
            var list = new List<dynamic> { a1, a2 };
            IQueryable qry = list.AsQueryable();

            var result1 = qry.Select("new (it as x)");
            var result = result1.Select("x.BlogId");

            // Assert
            Assert.Equal(new[] { 100, 200 }, result.ToDynamicArray<int>());
        }

        [Fact]
        [Trait("Issue", "136")]
        public void ExpressionTests_Select_ExpandoObjects()
        {
            // Arrange
            dynamic a = new ExpandoObject();
            a.Name = "a";
            a.BlogId = 100;

            dynamic b = new ExpandoObject();
            b.Name = "b";
            b.BlogId = 200;

            var list = new List<dynamic> { a, b };
            IQueryable qry = list.AsQueryable();

            // Act
            var result = qry.Select("it").Select("BlogId");

            // Assert
            Assert.Equal(new[] { 100, 200 }, result.ToDynamicArray<int>());
        }

        [Fact]
        public void ExpressionTests_Shift()
        {
            ExpressionTests_ShiftInternal<sbyte, int>();
            ExpressionTests_ShiftInternal<byte, int>();
            ExpressionTests_ShiftInternal<short, int>();
            ExpressionTests_ShiftInternal<ushort, int>();
            ExpressionTests_ShiftInternal<int, int>();
            ExpressionTests_ShiftInternal<uint, uint>();
            ExpressionTests_ShiftInternal<long, long>();
            ExpressionTests_ShiftInternal<ulong, ulong>();
        }

        [Fact]
        public void ExpressionTests_Shift_Exception()
        {
            // Assign
            var qry = new[] { 10, 20, 30 }.AsQueryable();

            // Act and Assert
            Check.ThatCode(() => qry.Select("it <<< 1")).Throws<ParseException>();
        }

        private static void ExpressionTests_ShiftInternal<TItemType, TResult>()
        {
            // Arrange
            var lst = new[] { 10, 20, 30 }.Select(item => (TItemType)Convert.ChangeType(item, typeof(TItemType)));
            var qry = lst.AsQueryable();
            int? nullableShift = 2;

            // Act
            var result1 = qry.Select("it << 1");
            var result2 = qry.Select("it >> 1");
            var result3 = qry.Where("it << 2 = 80");
            var result4 = qry.Where("it << @0 = 80", nullableShift);

            // Assert
            Assert.Equal(new[] { 20, 40, 60 }.Select(item => Convert.ChangeType(item, typeof(TResult))).ToArray(), result1.Cast<object>().ToArray());
            Assert.Equal(new[] { 5, 10, 15 }.Select(item => Convert.ChangeType(item, typeof(TResult))).ToArray(), result2.Cast<object>().ToArray());
            Assert.Equal(Convert.ChangeType(20, typeof(TItemType)), result3.Single());
            Assert.Equal(Convert.ChangeType(20, typeof(TItemType)), result4.Single());
        }

        [Fact]
        public void ExpressionTests_SkipAndTake()
        {
            // Arrange
            var samples = User.GenerateSampleModels(3);
            samples[0].Roles = null;
            samples[1].Roles = new List<Role> { new Role(), new Role() };

            var sampleQuery = samples.AsQueryable();

            // Act
            var expectedResult = sampleQuery.Select(x => new { SecondRole = x.Roles != null ? x.Roles.Skip(1).Take(1) : null }).ToArray();

            var result = sampleQuery.Select("new ( iif(Roles != null, Roles.Skip(1).Take(1), null) as SecondRole )");

            // Assert
            var resultArray = result.ToDynamicArray();
            for (int i = 0; i < expectedResult.Count(); i++)
            {
                var expectedEntry = expectedResult[i];
                var entry = resultArray[i];
                if (expectedEntry.SecondRole == null)
                    Assert.Null(entry.SecondRole);
                else
                    Assert.Equal(expectedEntry.SecondRole.ToString(), entry.SecondRole.ToString());
            }
        }

        [Fact]
        public void ExpressionTests_ImplicitCast()
        {
            // Arrange
            Guid code1 = new Guid("651E08E3-85B1-42D1-80AF-68E28E2B7DA6");
            Guid code2 = new Guid("6451FEB2-3226-41D0-961C-B72B7B5A0157");

            var samples = User.GenerateSampleModels(3);
            samples[0].State = new UserState() { StatusCode = code1, Description = "alive" };
            samples[1].State = new UserState() { StatusCode = code2, Description = "deceased" };

            // Act
            string queryString = "State == @0";
            IList<User> result = samples.AsQueryable().Where(queryString, code1).ToList();

            string queryString2 = "@0 == State";
            IList<User> result2 = samples.AsQueryable().Where(queryString2, code1).ToList();

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Equal(code1, result[0].State.StatusCode);
            Assert.Equal("alive", result[0].State.Description);

            Assert.Equal(1, result2.Count);
            Assert.Equal(code1, result2[0].State.StatusCode);
            Assert.Equal("alive", result2[0].State.Description);
        }

        [Fact]
        public void ExpressionTests_StringCompare()
        {
            // Arrange
            var baseQuery = new[] { "1", "2", "3", "4" }.AsQueryable();

            // Act
            var resultGreaterThan = baseQuery.Where("it > @0", "2");
            var resultGreaterThanEqual = baseQuery.Where("it >= @0", "2");

            var resultLessThan = baseQuery.Where("it < @0", "3");
            var resultLessThanEqual = baseQuery.Where("it <= @0", "3");

            // Assert
            Check.That(resultGreaterThan.ToArray()).ContainsExactly("3", "4");
            Check.That(resultGreaterThanEqual.ToArray()).ContainsExactly("2", "3", "4");

            Check.That(resultLessThan.ToArray()).ContainsExactly("1", "2");
            Check.That(resultLessThanEqual.ToArray()).ContainsExactly("1", "2", "3");
        }

        [Fact]
        public void ExpressionTests_StringConcatenation()
        {
            // Arrange
            var baseQuery = new[] { new { First = "FirstName", Last = "LastName" } }.AsQueryable();

            // Act
            var resultAddWithPlus = baseQuery.Select<string>("it.First + \" \" + it.Last");
            var resultAddWithAmp = baseQuery.Select<string>("it.First & \" \" & it.Last");

            var resultAddWithPlusAndParams = baseQuery.Select<string>("it.First + @0 + \" \" + @1", "x", "y");
            var resultAddWithAmpAndParams = baseQuery.Select<string>("it.First & @0 & \" \" & @1", "x", "y");

            // Assert
            Assert.Equal("FirstName LastName", resultAddWithPlus.First());
            Assert.Equal("FirstName LastName", resultAddWithAmp.First());

            Assert.Equal("FirstNamex y", resultAddWithPlusAndParams.First());
            Assert.Equal("FirstNamex y", resultAddWithAmpAndParams.First());
        }

        [Fact]
        public void ExpressionTests_StringEscaping()
        {
            // Arrange
            var baseQuery = new[] { new { Value = "ab\"cd" }, new { Value = "a \\ b" } }.AsQueryable();

            // Act
            var result1 = baseQuery.Where("it.Value == \"ab\\\"cd\"").ToList();
            var result2 = baseQuery.Where("it.Value.IndexOf('\\\\') != -1").ToList();

            // Assert
            Assert.Single(result1);
            Assert.Equal("ab\"cd", result1[0].Value);

            Assert.Single(result2);
            Assert.Equal("a \\ b", result2[0].Value);
        }

        [Fact]
        public void ExpressionTests_BinaryAnd()
        {
            // Arrange
            var baseQuery = new[] { new { Int = 5 } }.AsQueryable();

            // Act
            var resultAddWithAmp = baseQuery.Select<int>("it.Int & @0", "4");
            var resultAddWithAmp2 = baseQuery.Select<int>("it.Int & @0", "2");
            var resultAddWithAmp3 = baseQuery.Select<int>("@0 & it.Int", "4");

            // Assert
            Assert.Equal(4, resultAddWithAmp.First());
            Assert.Equal(0, resultAddWithAmp2.First());
            Assert.Equal(4, resultAddWithAmp3.First());
        }

        [Fact]
        public void ExpressionTests_Subtract_Number()
        {
            // Arrange
            var values = new[] { -1, 2 }.AsQueryable();

            // Act
            var result = values.Select<int>("it - 10");
            var expected = values.Select(i => i - 10);

            // Assert
            Check.That(result).ContainsExactly(expected);
        }

        [Fact]
        public void ExpressionTests_Sum()
        {
            // Arrange
            int[] initValues = { 1, 2, 3, 4, 5 };
            var qry = initValues.AsQueryable().Select(x => new { strValue = "str", intValue = x }).GroupBy(x => x.strValue);

            // Act
            var result = qry.Select("Sum(intValue)").AsEnumerable().ToArray()[0];

            // Assert
            Assert.Equal(15, result);
        }

        [Fact]
        public void ExpressionTests_Sum_LowerCase()
        {
            // Arrange
            int[] initValues = { 1, 2, 3, 4, 5 };
            var qry = initValues.AsQueryable().Select(x => new { strValue = "str", intValue = x }).GroupBy(x => x.strValue);

            // Act
            var result = qry.Select("sum(intValue)").AsEnumerable().ToArray()[0];

            // Assert
            Assert.Equal(15, result);
        }

        [Fact]
        public void ExpressionTests_Sum2()
        {
            // Arrange
            var initValues = new[]
            {
                new SimpleValuesModel { FloatValue = 1 },
                new SimpleValuesModel { FloatValue = 2 },
                new SimpleValuesModel { FloatValue = 3 },
            };

            var qry = initValues.AsQueryable();

            // Act
            var result = qry.Select("FloatValue").Sum();
            var result2 = ((IQueryable<float>)qry.Select("FloatValue")).Sum();

            // Assert
            Assert.Equal(6.0f, result);
            Assert.Equal(6.0f, result2);
        }


        [Fact]
        public void ExpressionTests_Type_Integer()
        {
            // Arrange
            var qry = new[] { 1, 2, 3, 4, 5, 6 }.AsQueryable();

            // Act
            var resultLessThan = qry.Where("it < 4");
            var resultLessThanEqual = qry.Where("it <= 4");

            var resultGreaterThan = qry.Where("4 > it");
            var resultGreaterThanEqual = qry.Where("4 >= it");

            var resultEqualItLeft = qry.Where("it = 5");
            var resultEqualItRight = qry.Where("5 = it");

            var resultEqualIntParamLeft = qry.Where("@0 = it", 5);
            var resultEqualIntParamRight = qry.Where("it = @0", 5);

            // Assert
            Check.That(resultLessThan.ToArray()).ContainsExactly(1, 2, 3);
            Check.That(resultLessThanEqual.ToArray()).ContainsExactly(1, 2, 3, 4);
            Check.That(resultGreaterThan.ToArray()).ContainsExactly(1, 2, 3);
            Check.That(resultGreaterThanEqual.ToArray()).ContainsExactly(1, 2, 3, 4);

            Check.That(resultEqualItLeft.Single()).Equals(5);
            Check.That(resultEqualItRight.Single()).Equals(5);

            Check.That(resultEqualIntParamLeft.Single()).Equals(5);
            Check.That(resultEqualIntParamRight.Single()).Equals(5);
        }

        [Fact]
        public void ExpressionTests_Type_Integer_Qualifiers()
        {
            // Arrange
            var valuesL = new[] { 1L, 2L, 3L }.AsQueryable();
            var resultValuesL = new[] { 2L, 3L }.AsQueryable();

            var valuesU = new[] { 1U, 2U, 3U }.AsQueryable();
            var resultValuesU = new[] { 2U, 3U }.AsQueryable();

            var valuesUL = new[] { 1UL, 2UL, 3UL }.AsQueryable();
            var resultValuesUL = new[] { 2UL, 3UL }.AsQueryable();

            // Act
            var resultL = valuesL.Where("it in (2L, 3L)");
            var resultU = valuesU.Where("it in (2U, 3U)");
            var resultUL = valuesUL.Where("it in (2UL, 3UL)");

            // Assert
            Assert.Equal(resultValuesL.ToArray(), resultL.ToArray());
            Assert.Equal(resultValuesU.ToArray(), resultU.ToArray());
            Assert.Equal(resultValuesUL.ToArray(), resultUL.ToArray());
        }

        [Fact]
        public void ExpressionTests_Type_StructWithIntegerEquality()
        {
            // Arrange
            var valuesL = new[] { new SnowflakeId(1L), new SnowflakeId(100), new SnowflakeId(5) }.AsQueryable();
            var resultValuesL = new[] { new SnowflakeId(100) }.AsQueryable();

            // Act
            var resultL = valuesL.Where("it == 100");
            var resultLs = valuesL.Where("it = 100");
            var resultNL = valuesL.Where("it != 1 && it != 5");
            var resultArg = valuesL.Where("it == @0", 100);
            var resultArgs = valuesL.Where("it = @0", 100);
            var resultIn = valuesL.Where("it in (100)");

            // Assert
            Assert.Equal(resultValuesL.ToArray(), resultL);
            Assert.Equal(resultValuesL.ToArray(), resultLs);
            Assert.Equal(resultValuesL.ToArray(), resultNL);
            Assert.Equal(resultValuesL.ToArray(), resultArg);
            Assert.Equal(resultValuesL.ToArray(), resultArgs);
            Assert.Equal(resultValuesL.ToArray(), resultIn);
        }

        [Fact]
        public void ExpressionTests_Type_StructWithIntegerEquality_BothVariablesInStruct()
        {
            // Arrange
            var valuesL = new[]
            {
                new
                {
                    Id = new SnowflakeId(1L),
                    Var = 1
                },
                new
                {
                    Id = new SnowflakeId(2L),
                    Var = 1
                },
                new
                {
                    Id = new SnowflakeId(1L),
                    Var = 2
                }
            }.AsQueryable();

            var resultValuesL = new[] {
                new {
                    Id = new SnowflakeId(1L),
                    Var = 1
                }
            }.AsQueryable().ToArray();

            // Act
            var resultL = valuesL.Where("it.Id == it.Var");
            var resultLs = valuesL.Where("it.Id = it.Var");

            // Assert
            Assert.Equal(resultValuesL.ToArray(), resultL);
            Assert.Equal(resultValuesL.ToArray(), resultLs);
        }

        [Fact]
        public void ExpressionTests_Type_Integer_Qualifiers_Negative()
        {
            // Arrange
            var valuesL = new[] { -1L, -2L, -3L }.AsQueryable();
            var resultValuesL = new[] { -2L, -3L }.AsQueryable();

            // Act
            var resultL = valuesL.Where("it <= -2L");
            var resultIn = valuesL.Where("it in (-2L, -3L)");

            // Assert
            Assert.Equal(resultValuesL.ToArray(), resultL);
            Assert.Equal(resultValuesL.ToArray(), resultIn);
        }

        [Fact]
        public void ExpressionTests_Type_Integer_Qualifiers_Exceptions()
        {
            // Arrange
            var values = new[] { 1L, 2L, 3L }.AsQueryable();

            // Assert
            Assert.Throws<ParseException>(() => values.Where("it in (2LL)"));
            Assert.Throws<ParseException>(() => values.Where("it in (2UU)"));
            Assert.Throws<ParseException>(() => values.Where("it in (2LU)"));
            Assert.Throws<ParseException>(() => values.Where("it in (2B)"));

            Assert.Throws<ParseException>(() => values.Where("it < -2LL"));
            Assert.Throws<ParseException>(() => values.Where("it < -2UL"));
            Assert.Throws<ParseException>(() => values.Where("it < -2UU"));
            Assert.Throws<ParseException>(() => values.Where("it < -2LU"));
            Assert.Throws<ParseException>(() => values.Where("it < -2B"));
        }

        [Fact]
        public void ExpressionTests_Uri()
        {
            // Arrange
            var lst = new List<Uri>
            {
                new Uri("http://127.0.0.1"),
                new Uri("http://192.168.1.1"),
                new Uri("http://127.0.0.1")
            };

            var qry = lst.AsQueryable();

            // Act
            var result1 = qry.AsQueryable().Where("it = @0", new Uri("http://127.0.0.1"));

            // Assert
            Assert.Equal(2, result1.Count());
        }

        /// <summary>
        /// https://github.com/kahanu/System.Linq.Dynamic/issues/56
        /// </summary>
        [Fact]
        public void ExpressionTests_Where_DoubleDecimalCompare()
        {
            double d = 1000.0;

            var list = new List<SimpleValuesModel>
            {
                new SimpleValuesModel { DecimalValue = 123423.234M },
                new SimpleValuesModel { DecimalValue = 123423423423.2342M },
                new SimpleValuesModel { DecimalValue = 2342342433423.23423423M },
                new SimpleValuesModel { DecimalValue = 123.234M },
                new SimpleValuesModel { DecimalValue = 100000000000.232423423434M },
                new SimpleValuesModel { DecimalValue = 100.232423423434M }
            };
            var expected = list.Where(x => (double)x.DecimalValue > d).ToList();
            var result = list.AsQueryable().Where("double(DecimalValue) > @0", d).ToList();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ExpressionTests_Where_WithCachedLambda()
        {
            var list = new List<SimpleValuesModel>
            {
                new SimpleValuesModel { IntValue = 1 },
                new SimpleValuesModel { IntValue = 3 },
                new SimpleValuesModel { IntValue = 2 },
                new SimpleValuesModel { IntValue = 3 }
            };

            var lambda = DynamicExpressionParser.ParseLambda(typeof(SimpleValuesModel), typeof(bool), "IntValue == 3");
            var res = DynamicQueryableExtensions.Where(list.AsQueryable(), lambda);
            Assert.Equal(2, res.Count());

            var res2 = DynamicQueryableExtensions.Any(list.AsQueryable(), lambda);
            Assert.True(res2);

            var res3 = DynamicQueryableExtensions.Count(list.AsQueryable(), lambda);
            Assert.Equal(2, res3);

            var res4 = DynamicQueryableExtensions.First(list.AsQueryable(), lambda);
            Assert.Equal(res4, list[1]);

            var res5 = DynamicQueryableExtensions.FirstOrDefault(list.AsQueryable(), lambda);
            Assert.Equal(res5, list[1]);

            var res6 = DynamicQueryableExtensions.Last(list.AsQueryable(), lambda);
            Assert.Equal(res6, list[3]);

            var res7 = DynamicQueryableExtensions.LastOrDefault(list.AsQueryable(), lambda);
            Assert.Equal(res7, list[3]);

            var res8 = DynamicQueryableExtensions.Single(list.AsQueryable().Take(2), lambda);
            Assert.Equal(res8, list[1]);

            var res9 = DynamicQueryableExtensions.SingleOrDefault(list.AsQueryable().Take(2), lambda);
            Assert.Equal(res9, list[1]);
        }
    }
}
