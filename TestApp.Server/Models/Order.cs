public record Order(int Id, string SenderCity, string SenderAddress, string RecieverCity, string RecieverAddress, decimal Weight, DateTimeOffset DateOfPicking);
