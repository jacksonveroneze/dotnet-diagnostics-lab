using System.Globalization;
using System.Text;
using Bogus;
using Bogus.Extensions.Brazil;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;
using Person = JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models.Person;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Memory;

public class StringAllocationService : IStringAllocationService
{
    private const int MinIterations = 1;
    private const int MaxIterations = 50;

    private static readonly DateTime MinBirthDate = DateTime.UtcNow.AddYears(-80);
    private static readonly DateTime MaxBirthDate = DateTime.UtcNow.AddYears(-18);

    private static readonly Faker<Person> PersonFaker = new Faker<Person>()
        .CustomInstantiator(faker =>
        {
            var firstName = faker.Name.FirstName();
            var lastName = faker.Name.LastName();
            var name = string.Concat(firstName, " ", lastName);

            var id = faker.Random.Guid();
            var birthDate = DateOnly.FromDateTime(
                faker.Date.Between(MinBirthDate, MaxBirthDate));
            var email = faker.Internet.Email(firstName, lastName);
            var phone = faker.Phone.PhoneNumber("+55##########");
            var cpf = faker.Person.Cpf(includeFormatSymbols: true);

            return new Person(
                id,
                name,
                birthDate,
                email,
                phone,
                cpf);
        });

    public SimulationResult Run(
        int iterations)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(iterations, MinIterations);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(iterations, MaxIterations);

        return SimulationRunner.Run(()
            => InternalRun(iterations));
    }

    private static void InternalRun(
        int iterations)
    {
        var persons = Enumerable.Range(0, iterations)
            .Select(_ => CreatePerson())
            .Where(person => CpfValidator.IsValid(person.Document))
            .ToArray();

        var csv = BuildCsvHeader() + Environment.NewLine;

        foreach (var person in persons)
        {
            var line = BuildCsvLine(person) + Environment.NewLine;

            csv += line;
        }
    }

    private static Person CreatePerson()
    {
        return PersonFaker.Generate();
    }

    private static string BuildCsvHeader()
    {
        return string.Join(',', "Id", "Name", "BirthDate", "Email", "Phone");
    }

    private static string BuildCsvLine(Person person)
    {
        var id = person.Id.ToString();
        var name = person.Name.ToUpper(CultureInfo.InvariantCulture);
        var birthDate = person.BirthDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var email = person.Email.ToLower(CultureInfo.InvariantCulture);
        var phone = person.Phone;

        return string.Join(',', id, name, birthDate, email, phone);
    }
}
