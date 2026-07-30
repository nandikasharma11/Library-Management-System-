using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSystem.Controllers;
using LMSystem.Models;
using Xunit;

namespace LMSystem.Tests
{
    public class BooksControllerInMemoryTests
    {
        private DbContextOptions<LibraryContext> CreateNewContextOptions()
        {
            // Use a unique database name per test to prevent data leak between tests
            return new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private async Task SeedDatabaseAsync(LibraryContext context)
        {
            var books = new List<Book>
            {
                new Book { BookId = 1, Title = "The Pragmatic Programmer", Author = "Andrew Hunt", ISBN = "978-0201616224", PublishedDate = new DateTime(2021, 10, 30), IsAvailable = true },
                new Book { BookId = 2, Title = "Design Pattern using C#", Author = "Robert C. Martin", ISBN = "978-0132350884", PublishedDate = new DateTime(2023, 8, 1), IsAvailable = true },
                new Book { BookId = 3, Title = "Mastering ASP.NET Core", Author = "Pranaya Kumar", ISBN = "978-0451616235", PublishedDate = new DateTime(2022, 11, 22), IsAvailable = true },
                new Book { BookId = 4, Title = "SQL Server with DBA", Author = "Rakesh Kumat", ISBN = "978-4562350123", PublishedDate = new DateTime(2020, 8, 15), IsAvailable = true },
                new Book { BookId = 5, Title = "C# Programming Basics", Author = "John Doe", ISBN = "978-0123456789", PublishedDate = new DateTime(2025, 1, 1), IsAvailable = true },
                new Book { BookId = 6, Title = "Introduction to SQL", Author = "Jane Doe", ISBN = "978-9876543210", PublishedDate = new DateTime(2026, 2, 2), IsAvailable = true }
            };

            context.Books12.AddRange(books);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Index_FiltersBooks_WhenSearchStringIsProvided()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using (var context = new LibraryContext(options))
            {
                await SeedDatabaseAsync(context);
            }

            using (var context = new LibraryContext(options))
            {
                var controller = new BooksController(context);

                // Act
                var result = await controller.Index(searchQuery: "SQL", page: 1);

                // Assert
                var viewResult = result.Should().BeOfType<ViewResult>().Subject;
                var model = viewResult.Model.Should().BeOfType<BookListViewModel>().Subject;
                
                // Assert filters correct books
                model.Books.Should().HaveCount(2);
                model.Books.Select(b => b.Title).Should().Contain(new[] { "SQL Server with DBA", "Introduction to SQL" });
            }
        }

        [Fact]
        public async Task Index_ReturnsCorrectPageSize_ForPaginatedRequests()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using (var context = new LibraryContext(options))
            {
                await SeedDatabaseAsync(context); // 6 books
            }

            using (var context = new LibraryContext(options))
            {
                var controller = new BooksController(context);

                // Act - Requesting Page 1 (Page size is 5)
                var result = await controller.Index(searchQuery: null, page: 1);

                // Assert
                var viewResult = result.Should().BeOfType<ViewResult>().Subject;
                var model = viewResult.Model.Should().BeOfType<BookListViewModel>().Subject;

                model.Books.Should().HaveCount(5); // page size is 5
                model.TotalPages.Should().Be(2); // 6 books total, page size 5 => 2 pages
                model.CurrentPage.Should().Be(1);
            }
        }
    }
}
