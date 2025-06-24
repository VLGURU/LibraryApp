using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibraryManagementSystem
{
    public class Book
    {
        public string Title { get; }
        public string Author { get; }
        public bool IsAvailable { get; set; }

        public Book(string title, string author)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Author = author ?? throw new ArgumentNullException(nameof(author));
            IsAvailable = true;
        }

        public override string ToString() => 
            $"{Title} by {Author} ({(IsAvailable ? "Available" : "Borrowed"})";
    }

    public abstract class User
    {
        public string Name { get; }
        public List<Book> BorrowedBooks { get; } = new List<Book>();

        protected User(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public void AddBorrowedBook(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (!BorrowedBooks.Contains(book))
                BorrowedBooks.Add(book);
        }

        public void RemoveBorrowedBook(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            BorrowedBooks.Remove(book);
        }
    }

    public class Librarian : User
    {
        public Librarian(string name) : base(name) { }

        public void AddBook(Library library, string title, string author)
        {
            library.AddBook(title, author);
        }

        public void DeleteBook(Library library, string title)
        {
            library.RemoveBook(title);
        }

        public void RegisterUser(Library library, string userName)
        {
            library.RegisterUser(userName);
        }

        public void ViewAllUsers(Library library)
        {
            library.DisplayAllUsers();
        }

        public void ViewAllBooks(Library library)
        {
            library.DisplayAllBooks();
        }
    }

    public class Reader : User
    {
        public Reader(string name) : base(name) { }

        public void BorrowBook(Library library, string title)
        {
            var book = library.FindBook(title);
            
            if (book == null)
            {
                Console.WriteLine("Book not found.");
                return;
            }

            if (!book.IsAvailable)
            {
                Console.WriteLine("Book is already borrowed.");
                return;
            }

            book.IsAvailable = false;
            AddBorrowedBook(book);
            Console.WriteLine($"Book '{title}' borrowed successfully.");
        }

        public void ReturnBook(Library library, string title)
        {
            var book = BorrowedBooks.FirstOrDefault(b => b.Title.Equals(title));
            
            if (book == null)
            {
                Console.WriteLine("You don't have this book.");
                return;
            }

            book.IsAvailable = true;
            RemoveBorrowedBook(book);
            Console.WriteLine($"Book '{title}' returned successfully.");
        }

        public void ShowMyBooks()
        {
            if (!BorrowedBooks.Any())
            {
                Console.WriteLine("You don't have any books.");
                return;
            }

            Console.WriteLine($"\nYour books ({Name}):");
            foreach (var book in BorrowedBooks)
            {
                Console.WriteLine(book);
            }
        }
    }

    public class Library
    {
        public List<Book> Books { get; } = new List<Book>();
        public List<User> Users { get; } = new List<User>();

        private readonly string _booksFilePath = "books.txt";
        private readonly string _usersFilePath = "users.txt";

        public Library()
        {
            LoadData();
        }

        public void AddBook(string title, string author)
        {
            if (Books.Any(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Book already exists.");
                return;
            }

            Books.Add(new Book(title, author));
            SaveBooks();
            Console.WriteLine("Book added successfully.");
        }

        public void RemoveBook(string title)
        {
            var book = FindBook(title);
            if (book == null)
            {
                Console.WriteLine("Book not found.");
                return;
            }

            if (!book.IsAvailable)
            {
                Console.WriteLine("Cannot remove a borrowed book.");
                return;
            }

            Books.Remove(book);
            SaveBooks();
            Console.WriteLine("Book removed successfully.");
        }

        public Book FindBook(string title)
        {
            return Books.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        }

        public void RegisterUser(string userName)
        {
            if (Users.Any(u => u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("User already exists.");
                return;
            }

            Users.Add(new Reader(userName));
            SaveUsers();
            Console.WriteLine("User registered successfully.");
        }

        public User FindUser(string name)
        {
            return Users.FirstOrDefault(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void DisplayAllBooks()
        {
            if (!Books.Any())
            {
                Console.WriteLine("No books available.");
                return;
            }

            Console.WriteLine("\nAll books in library:");
            foreach (var book in Books)
            {
                Console.WriteLine(book);
            }
        }

        public void DisplayAllUsers()
        {
            if (!Users.Any())
            {
                Console.WriteLine("No users registered.");
                return;
            }

            Console.WriteLine("\nRegistered users:");
            foreach (var user in Users)
            {
                Console.WriteLine(user.Name);
            }
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(_booksFilePath))
                {
                    foreach (string line in File.ReadAllLines(_booksFilePath))
                    {
                        string[] parts = line.Split('|');
                        if (parts.Length >= 3)
                        {
                            Books.Add(new Book(parts[0], parts[1]) { IsAvailable = bool.Parse(parts[2]) });
                        }
                    }
                }

                if (File.Exists(_usersFilePath))
                {
                    foreach (string line in File.ReadAllLines(_usersFilePath))
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            Users.Add(new Reader(line));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading data: {e.Message}");
            }
        }

        private void SaveBooks()
        {
            try
            {
                File.WriteAllLines(_booksFilePath, 
                    Books.Select(b => $"{b.Title}|{b.Author}|{b.IsAvailable}"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error saving books: {e.Message}");
            }
        }

        private void SaveUsers()
        {
            try
            {
                File.WriteAllLines(_usersFilePath, Users.Select(u => u.Name));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error saving users: {e.Message}");
            }
        }
    }

    internal static class Program
    {
        private static readonly Library Library = new Library();

        static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Library Management System");
                Console.WriteLine("1. Librarian Login");
                Console.WriteLine("2. Reader Login");
                Console.WriteLine("3. Exit");
                Console.Write("Select option: ");

                if (!int.TryParse(Console.ReadLine(), out var choice))
                {
                    Console.WriteLine("Invalid input. Please try again.");
                    Console.ReadKey();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        LibrarianMenu();
                        break;
                    case 2:
                        ReaderMenu();
                        break;
                    case 3:
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        private static void LibrarianMenu()
        {
            var librarian = new Librarian("Admin");

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Librarian Menu");
                Console.WriteLine("1. Add Book");
                Console.WriteLine("2. Remove Book");
                Console.WriteLine("3. Register New User");
                Console.WriteLine("4. View All Books");
                Console.WriteLine("5. View All Users");
                Console.WriteLine("6. Back to Main Menu");
                Console.Write("Select option: ");

                if (!int.TryParse(Console.ReadLine(), out var choice))
                {
                    Console.WriteLine("Invalid input. Please try again.");
                    Console.ReadKey();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        Console.Write("Enter book title: ");
                        string title = Console.ReadLine();
                        Console.Write("Enter book author: ");
                        librarian.AddBook(Library, title, Console.ReadLine());
                        break;
                    case 2:
                        Console.Write("Enter book title to remove: ");
                        librarian.DeleteBook(Library, Console.ReadLine());
                        break;
                    case 3:
                        Console.Write("Enter new username: ");
                        librarian.RegisterUser(Library, Console.ReadLine());
                        break;
                    case 4:
                        librarian.ViewAllBooks(Library);
                        break;
                    case 5:
                        librarian.ViewAllUsers(Library);
                        break;
                    case 6:
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        private static void ReaderMenu()
        {
            Console.Write("Enter your name: ");
            string name = Console.ReadLine();

            var user = Library.FindUser(name) as Reader;
            if (user == null)
            {
                Console.WriteLine("User not found. Creating new account...");
                Library.RegisterUser(name);
                user = Library.FindUser(name) as Reader;
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Reader Menu - {user.Name}");
                Console.WriteLine("1. Borrow Book");
                Console.WriteLine("2. Return Book");
                Console.WriteLine("3. View My Books");
                Console.WriteLine("4. View All Available Books");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("Select option: ");

                if (!int.TryParse(Console.ReadLine(), out var choice))
                {
                    Console.WriteLine("Invalid input. Please try again.");
                    Console.ReadKey();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        Console.Write("Enter book title to borrow: ");
                        user.BorrowBook(Library, Console.ReadLine());
                        break;
                    case 2:
                        Console.Write("Enter book title to return: ");
                        user.ReturnBook(Library, Console.ReadLine());
                        break;
                    case 3:
                        user.ShowMyBooks();
                        break;
                    case 4:
                        Library.DisplayAllBooks();
                        break;
                    case 5:
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }
    }
