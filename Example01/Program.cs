

using Bogus;
using Microsoft.EntityFrameworkCore;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;

Console.WriteLine("Welcome to Ardalis Specification Test");
Console.WriteLine("Please select an option:");

var personDbContext = new PersonDbContext();
personDbContext.Database.EnsureCreated();


Console.WriteLine("1. Without Specification");
Console.WriteLine("2. With Specification");
Console.WriteLine("3. With Specification InMemoryList");
var option = Console.ReadLine();

switch (option)
{
    case "1":
        WithoutSpecification();
        break;
    case "2":
        WithSpecification();
        break;
    case "3":
        WithSpecificationInMemoryList();
        break;
    default:
        throw new ArgumentOutOfRangeException();
}


personDbContext.Database.EnsureDeleted();

static void WithoutSpecification()
{
    var personDbContext = new PersonDbContext();
    var people = personDbContext.People.ToList();
    Console.WriteLine($"Total people: {people.Count}");
}
static void WithSpecification()
{

    var personDbContext = new PersonDbContext();
    var people = personDbContext.People.WithSpecification(new PeopleGreaterThan18YearSpecification()).ToList();
    Console.WriteLine($"Total people: {people.Count}");
}
static void WithSpecificationInMemoryList()
{
    var faker = new Faker<Person>().RuleFor(x => x.Id, f => Guid.NewGuid())
                                      .RuleFor(x => x.Name, f => f.Name.FirstName())
                                      .RuleFor(x => x.LastName, f => f.Name.LastName())
                                      .RuleFor(x => x.Age, f => f.Random.Number(1, 100));

    var people = new PeopleGreaterThan18YearSpecification().Evaluate(faker.Generate(100)).ToList();
    Console.WriteLine($"Total people: {people.Count}");
}
public class PeopleGreaterThan18YearSpecification : Specification<Person>, ISingleResultSpecification
{
    public PeopleGreaterThan18YearSpecification()
    {
        Query.Where(x => x.Age > 18);
    }
}

public class PersonDbContext : DbContext
{
    public DbSet<Person> People { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("People");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>().HasKey(p => p.Id);
        modelBuilder.Entity<Person>().Property(p => p.Name).HasMaxLength(50);
        modelBuilder.Entity<Person>().Property(p => p.LastName).HasMaxLength(50);

        var faker = new Faker<Person>().RuleFor(x => x.Id, f => Guid.NewGuid())
                                        .RuleFor(x => x.Name, f => f.Name.FirstName())
                                        .RuleFor(x => x.LastName, f => f.Name.LastName())
                                        .RuleFor(x => x.Age, f => f.Random.Number(1, 100));


        modelBuilder.Entity<Person>().HasData(faker.Generate(100));
        base.OnModelCreating(modelBuilder);
    }
}
public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}