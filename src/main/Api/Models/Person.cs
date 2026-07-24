namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

public sealed record Person(
    Guid Id, 
    string Name, 
    DateOnly BirthDate, 
    string Email, 
    string Phone, 
    string Document);
