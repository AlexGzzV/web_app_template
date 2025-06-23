using web_app_template.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace web_app_template.Data.Settings
{
    public class PokemonSetting : IEntityTypeConfiguration<Pokemon>
    {
        public void Configure(EntityTypeBuilder<Pokemon> builder)
        {
            builder.HasData(
                new Pokemon
                {
                    Id = 1,
                    Name = "Pikachu",
                    Hability = "Impactrueno",
                    Owner = "Ash Ketchum",
                    IsAvtive = true
                },
                new Pokemon
                {
                    Id = 2,
                    Name = "Charmander",
                    Hability = "Mar Llamas",
                    Owner = "Misty",
                    IsAvtive = true
                },
                new Pokemon
                {
                    Id = 3,
                    Name = "Bulbasaur",
                    Hability = "Espesura",
                    Owner = "Brock",
                    IsAvtive = false
                },
                new Pokemon
                {
                    Id = 4,
                    Name = "Squirtle",
                    Hability = "Torrente",
                    Owner = "Ash Ketchum",
                    IsAvtive = true
                },
                new Pokemon
                {
                    Id = 5,
                    Name = "Butterfree",
                    Hability = "Ojo Ccompuesto",
                    Owner = "Misty",
                    IsAvtive = false
                }
            );
        }
    }
}
