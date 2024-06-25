using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.Data
{
    public static class DataSeeder
    {
        public static void SeedData<T>(this DbContext context, string filePath) where T : class
        {
            var jsonData = File.ReadAllText(filePath);
            var entities = JsonSerializer.Deserialize<List<T>>(jsonData);
            if (entities != null && entities.Count > 0)
            {
                var dbSet = context.Set<T>();
                if (!dbSet.Any())
                {
                    dbSet.AddRange(entities);
                    context.SaveChanges();
                }
                else
                {
                    dbSet.UpdateRange(entities);
                    context.SaveChanges();
                }
            }
            else
            {
                Console.WriteLine($"No data found for {typeof(T).Name}.");
            }
        }
    }

}
