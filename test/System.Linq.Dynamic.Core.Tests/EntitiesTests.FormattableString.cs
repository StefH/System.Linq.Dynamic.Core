#if EFCORE
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
#else
using System.Data.Entity;
using EntityFramework.DynamicLinq;
#endif
using System.Threading.Tasks;
using Xunit;
using System.Linq.Dynamic.Core.Tests.Helpers.Entities;
using System.Linq.Expressions;

namespace System.Linq.Dynamic.Core.Tests
{
#if EFCORE || (NET46_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NETSTANDARD1_3_OR_GREATER || UAP10_0)
    public partial class EntitiesTests
    {
        [Fact]
        public void Entities_All_FS()
        {
            //Arrange
            PopulateTestData(1, 0);
            int value = 2000;
            var expected = _context.Blogs.All(b => b.BlogId > value);

            //Act
            var actual = _context.Blogs.AllInterpolated($"BlogId > {value}");

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Entities_AllAsync_FS()
        {
            //Arrange
            PopulateTestData(1, 0);
            int value = 2000;
            var expected = await _context.Blogs.AllAsync(b => b.BlogId > value);

            //Act
            var actual = await _context.Blogs.AllInterpolatedAsync($"BlogId > {value}");

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Entities_Count_Predicate_FS()
        {
            const string search = "a";

            //Arrange
            var blog1 = new Blog { Name = "blog a", BlogId = 1000, Created = DateTime.Now };
            var blog2 = new Blog { Name = "blog b", BlogId = 3000, Created = DateTime.Now };
            _context.Blogs.Add(blog1);
            _context.Blogs.Add(blog2);
            _context.SaveChanges();

            //Act
            int expected = _context.Blogs.Count(b => b.Name.Contains(search));
            int result = _context.Blogs.CountInterpolated($"Name.Contains({search})");

            //Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Entities_CountAsync_Predicate_Args_FS()
        {
            const string search = "a";

            //Arrange
            var blog1 = new Blog { Name = "blog a", BlogId = 1000, Created = DateTime.Now };
            var blog2 = new Blog { Name = "blog b", BlogId = 3000, Created = DateTime.Now };
            _context.Blogs.Add(blog1);
            _context.Blogs.Add(blog2);
            _context.SaveChanges();

            Expression<Func<Blog, bool>> predicate = b => b.Name.Contains(search);

#if EFCORE
            var expected = await EntityFrameworkQueryableExtensions.CountAsync(_context.Blogs, predicate);
#else
            var expected = await QueryableExtensions.CountAsync(_context.Blogs, predicate);
#endif

            //Act
            int result = await (_context.Blogs as IQueryable).CountInterpolatedAsync($"Name.Contains({search})");

            //Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Entities_LongCount_Predicate_FS()
        {
            const string search = "a";

            //Arrange
            var blog1 = new Blog { Name = "blog a", BlogId = 1000, Created = DateTime.Now };
            var blog2 = new Blog { Name = "blog b", BlogId = 3000, Created = DateTime.Now };
            _context.Blogs.Add(blog1);
            _context.Blogs.Add(blog2);
            _context.SaveChanges();

            //Act
            long expected = _context.Blogs.LongCount(b => b.Name.Contains(search));
            long result = _context.Blogs.LongCountInterpolated($"Name.Contains({search})");

            //Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Entities_LongCountAsync_Predicate_Args_FS()
        {
            const string search = "a";

            //Arrange
            var blog1 = new Blog { Name = "blog a", BlogId = 1000, Created = DateTime.Now };
            var blog2 = new Blog { Name = "blog b", BlogId = 3000, Created = DateTime.Now };
            _context.Blogs.Add(blog1);
            _context.Blogs.Add(blog2);
            _context.SaveChanges();

            Expression<Func<Blog, bool>> predicate = b => b.Name.Contains(search);

#if EFCORE
            var expected = await EntityFrameworkQueryableExtensions.LongCountAsync(_context.Blogs, predicate);
#else
            var expected = await QueryableExtensions.LongCountAsync(_context.Blogs, predicate);
#endif

            //Act
            long result = (_context.Blogs as IQueryable).LongCountInterpolated($"Name.Contains({search})");

            //Assert
            Assert.Equal(expected, result);
        }

        public void Entities_TakeWhile_FS()
        {
            //Arrange
            const int total = 33;
            PopulateTestData(total, 0);

            //Act
            int value = 5;
            var expected = _context.Blogs.OrderBy(b => b.BlogId).TakeWhile(b => b.BlogId > value).ToArray();
            var result = _context.Blogs.OrderByInterpolated($"BlogId").TakeWhileInterpolated($"b.BlogId > {value}").ToDynamicArray<Blog>();

            //Assert
            Assert.Equal(expected, result);
        }
    }
#endif
}
