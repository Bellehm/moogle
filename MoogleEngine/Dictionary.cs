namespace MoogleEngine;

public class Dictionary
{
    List<string[]> Sinonymous;//Lista donde se guardarán los sinónimos 
    public Dictionary(string root)
    {
        Sinonymous = new List<string[]>();
        StreamReader reader = new StreamReader(root);
        string line = reader.ReadLine();
        while (line != null)
        {
            line = line.ToLower();
            line = Utils.Transform(line);
            line = line.Replace('.', ' ');
            line = line.Replace(',', ' ');
            line = Utils.ClearSpaces(line);
            string[] words = line.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Contains('-') && words[i].IndexOf('-') > words[i].Length / 2)
                {
                    //arreglamos los sufijos
                    string word = words[i];
                    int pos = word.IndexOf('-');
                    string part1 = word.Substring(0, pos);
                    string part2 = word.Substring(pos + 1);
                    word = part1 + " - " + part2;
                    string[] aux = new string[words.Length];
                    string[] temp = word.Split(' ');
                    aux[0] = temp[0];
                    aux[1] = temp[0].Substring(0, temp[0].Length - temp[2].Length) + temp[2];
                    for (int k = 2; k < aux.Length; k++)
                        aux[k] = words[k - 1];
                    words = aux;
                }
            }
            Sinonymous.Add(words);
            line = reader.ReadLine();
        }            
    }
    public string[] this[string word]//devuelve el array que contenga los sinónimos de la palabra dada
    {
        get
        {
            foreach(string[] words in Sinonymous)
            {
                if (words.Contains(word.ToLower()))
                {
                    return words;
                }
            }
            return null;
        }
        set
        {
            throw new InvalidOperationException("Los datos nos son modificables");
        }
    }
}

