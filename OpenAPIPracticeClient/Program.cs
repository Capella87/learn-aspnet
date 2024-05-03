using System.Net;
using City;

var client = new CityClient(
    new HttpClient()
    {
        BaseAddress = new Uri("https://localhost:20621"),
    });

City.City createdCityEntry1 = await client.CityPOSTAsync("13", new City.City
{
    Name = "Gwacheon",
    Province = "Gyeonggi-do",
    Population = 82_860
});

City.City createdCityEntry2 = await client.CityPOSTAsync("14", new City.City
{
    Name = "Seongnam",
    Province = "Gyeonggi-do",
    Population = 919_464
});

City.City createdCityEntry3 = await client.CityPOSTAsync("15", new City.City
{
    Name = "Chungju",
    Province = "Chungcheongbuk-do",
    Population = 207_550
});

City.City createdCityEntry4 = await client.CityPOSTAsync("16", new City.City
{
    Name = "Gyeongju",
    Province = "Gyeongsangbuk-do",
    Population = 246_371
});

City.City fetchedCityEntry = await client.CityGETAsync("13");
Console.WriteLine($"Fetched a city entry: {fetchedCityEntry.Name}\nProvince: {fetchedCityEntry.Province}\nPopulation: {fetchedCityEntry.Population}");
