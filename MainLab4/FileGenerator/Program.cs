using OfficeOpenXml;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace FileGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Пример использования
            GenerateCountriesFile("Europe", "countries.xlsx");
            GenerateChemicalsFile("Organic", "chemicals.xlsx");
            GenerateRussianWordsFile("words.txt", "sorted_words.xlsx");
        }

        public static void GenerateCountriesFile(string continent, string outputFilePath)
        {
            var countries = new List<Country>
            {
                new Country("Russia", "Europe", "Moscow", 17098242, 144100000),
                new Country("Germany", "Europe", "Berlin", 357022, 83166711),
                new Country("France", "Europe", "Paris", 640679, 67022000),
                new Country("Italy", "Europe", "Rome", 301338, 60461826),
                new Country("Spain", "Europe", "Madrid", 505992, 46754778),
                new Country("Ukraine", "Europe", "Kyiv", 603628, 44134693),
                new Country("Poland", "Europe", "Warsaw", 312696, 38386000),
                new Country("Romania", "Europe", "Bucharest", 238397, 19286123),
                new Country("Netherlands", "Europe", "Amsterdam", 41543, 17443699),
                new Country("Belgium", "Europe", "Brussels", 30528, 11582039),
                new Country("Czech Republic", "Europe", "Prague", 78867, 10708981),
                new Country("Greece", "Europe", "Athens", 131957, 10423054),
                new Country("Portugal", "Europe", "Lisbon", 92090, 10276617),
                new Country("Sweden", "Europe", "Stockholm", 450295, 10099265),
                new Country("Hungary", "Europe", "Budapest", 93030, 9660351),
                new Country("Belarus", "Europe", "Minsk", 207600, 9449323),
                new Country("Austria", "Europe", "Vienna", 83879, 8902642),
                new Country("Serbia", "Europe", "Belgrade", 88361, 8737371),
                new Country("Switzerland", "Europe", "Bern", 41285, 8654622),
                new Country("Bulgaria", "Europe", "Sofia", 110994, 6948445),
                new Country("Denmark", "Europe", "Copenhagen", 43094, 5840045),
                new Country("Finland", "Europe", "Helsinki", 338424, 5540720),
                new Country("Slovakia", "Europe", "Bratislava", 49035, 5459642),
                new Country("Norway", "Europe", "Oslo", 385207, 5421241),
                new Country("Ireland", "Europe", "Dublin", 70273, 4937786),
                new Country("Croatia", "Europe", "Zagreb", 56594, 4047200),
                new Country("Bosnia and Herzegovina", "Europe", "Sarajevo", 51197, 3507017),
                new Country("Albania", "Europe", "Tirana", 28748, 2877797),
                new Country("Lithuania", "Europe", "Vilnius", 65300, 2794700),
                new Country("Slovenia", "Europe", "Ljubljana", 20273, 2078938),
                new Country("Latvia", "Europe", "Riga", 64589, 1919968),
                new Country("Estonia", "Europe", "Tallinn", 45227, 1326535)
            };

            var filteredCountries = countries
                .Where(c => c.Continent.Equals(continent, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.Population)
                .Take(30)
                .ToList();

            using (var package = new ExcelPackage(new FileInfo(outputFilePath)))
            {
                var worksheet = package.Workbook.Worksheets.Add("Countries");
                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "Continent";
                worksheet.Cells[1, 3].Value = "Capital";
                worksheet.Cells[1, 4].Value = "Area";
                worksheet.Cells[1, 5].Value = "Population";

                for (int i = 0; i < filteredCountries.Count; i++)
                {
                    var country = filteredCountries[i];
                    worksheet.Cells[i + 2, 1].Value = country.Name;
                    worksheet.Cells[i + 2, 2].Value = country.Continent;
                    worksheet.Cells[i + 2, 3].Value = country.Capital;
                    worksheet.Cells[i + 2, 4].Value = country.Area;
                    worksheet.Cells[i + 2, 5].Value = country.Population;
                }

                package.Save();
            }
        }

        public static void GenerateChemicalsFile(string chemicalClass, string outputFilePath)
        {
            var chemicals = new List<Chemical>
            {
                new Chemical("Organic", "Methane", 16.04),
                new Chemical("Organic", "Ethane", 30.07),
                new Chemical("Organic", "Propane", 44.10),
                new Chemical("Organic", "Butane", 58.12),
                new Chemical("Organic", "Pentane", 72.15),
                new Chemical("Organic", "Hexane", 86.18),
                new Chemical("Organic", "Heptane", 100.20),
                new Chemical("Organic", "Octane", 114.23),
                new Chemical("Organic", "Nonane", 128.26),
                new Chemical("Organic", "Decane", 142.28),
                new Chemical("Organic", "Undecane", 156.31),
                new Chemical("Organic", "Dodecane", 170.34),
                new Chemical("Organic", "Tridecane", 184.36),
                new Chemical("Organic", "Tetradecane", 198.39),
                new Chemical("Organic", "Pentadecane", 212.41),
                new Chemical("Organic", "Hexadecane", 226.44),
                new Chemical("Organic", "Heptadecane", 240.47),
                new Chemical("Organic", "Octadecane", 254.49),
                new Chemical("Organic", "Nonadecane", 268.52),
                new Chemical("Organic", "Eicosane", 282.55),
                new Chemical("Organic", "Heneicosane", 296.57),
                new Chemical("Organic", "Docosane", 310.60),
                new Chemical("Organic", "Tricosane", 324.63),
                new Chemical("Organic", "Tetracosane", 338.65),
                new Chemical("Organic", "Pentacosane", 352.68),
                new Chemical("Organic", "Hexacosane", 366.71),
                new Chemical("Organic", "Heptacosane", 380.73),
                new Chemical("Organic", "Octacosane", 394.76),
                new Chemical("Organic", "Nonacosane", 408.79),
                new Chemical("Organic", "Triacontane", 422.81)
            };

            var filteredChemicals = chemicals
                .Where(c => c.Class.Equals(chemicalClass, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.MolecularMass)
                .Take(30)
                .ToList();

            using (var package = new ExcelPackage(new FileInfo(outputFilePath)))
            {
                var worksheet = package.Workbook.Worksheets.Add("Chemicals");
                worksheet.Cells[1, 1].Value = "Class";
                worksheet.Cells[1, 2].Value = "Name";
                worksheet.Cells[1, 3].Value = "Molecular Mass";

                for (int i = 0; i < filteredChemicals.Count; i++)
                {
                    var chemical = filteredChemicals[i];
                    worksheet.Cells[i + 2, 1].Value = chemical.Class;
                    worksheet.Cells[i + 2, 2].Value = chemical.Name;
                    worksheet.Cells[i + 2, 3].Value = chemical.MolecularMass;
                }

                package.Save();
            }
        }

        public static void GenerateRussianWordsFile(string inputFilePath, string outputFilePath)
        {
            var words = File.ReadAllLines(inputFilePath)
                .OrderBy(word => word, StringComparer.OrdinalIgnoreCase)
                .Take(30)
                .ToList();

            using (var package = new ExcelPackage(new FileInfo(outputFilePath)))
            {
                var worksheet = package.Workbook.Worksheets.Add("Words");
                worksheet.Cells[1, 1].Value = "Word";

                for (int i = 0; i < words.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = words[i];
                }

                package.Save();
            }
        }
    }

    public class Country
    {
        public string Name { get; set; }
        public string Continent { get; set; }
        public string Capital { get; set; }
        public double Area { get; set; }
        public int Population { get; set; }

        public Country(string name, string continent, string capital, double area, int population)
        {
            Name = name;
            Continent = continent;
            Capital = capital;
            Area = area;
            Population = population;
        }
    }

    public class Chemical
    {
        public string Class { get; set; }
        public string Name { get; set; }
        public double MolecularMass { get; set; }

        public Chemical(string chemicalClass, string name, double molecularMass)
        {
            Class = chemicalClass;
            Name = name;
            MolecularMass = molecularMass;
        }
    }
}


