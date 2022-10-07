using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Samousse.Modules.LimiteLimite.Datas
{
    public class Cards
    {
        public readonly List<string> Proposition;

        public readonly List<List<string>> Sentences;

        private Cards(List<string> proposition, List<List<string>> sentences)
        {
            Proposition = proposition;
            Sentences = sentences;
        }

        public async static Task<Cards> BuildCards()
        {
            var proposition = new List<string>();
            var sentences = new List<List<string>>();

            proposition.Add("ptdr");

            sentences.Add(new List<string>() { "un", "deux" });

            var ret = new Cards(proposition, sentences);
            var jsonString = JsonSerializer.Serialize(ret, new JsonSerializerOptions() { WriteIndented = true });
            await File.WriteAllTextAsync("prout.json", jsonString);
            return ret;
        }
    }
}
