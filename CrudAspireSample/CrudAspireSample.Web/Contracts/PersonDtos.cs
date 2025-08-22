namespace Web.Contracts;

public record PersonCreateDto(string FirstName, string LastName);
public record PersonUpdateDto(string FirstName, string LastName);
public record PersonReadDto(int Id, string FirstName, string LastName);







