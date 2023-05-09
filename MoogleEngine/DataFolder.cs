namespace MoogleEngine;

class DataFolder
{
    Dictionary<string,Dictionary<string,float>> Data;//diccionario donde se guardará toda la información de los documentos cargados
    string[] Files;//array donde se guardarán las rutas de los archivos
    float[] MaxFreq;
    public int Count{get;private set;}//cantidad de documentos cargados
    public DataFolder(string folder)
    {
        Data=new Dictionary<string, Dictionary<string,float>>();
        Files=Utils.EnumerateFiles(folder);//cargamos los archivos
        Count=Files.Length;
        MaxFreq=new float[Count];
        System.Console.WriteLine("Cargando archivos");
        foreach (string file in Files)//cargamos toda la informacion necesaria
        {
            System.Console.WriteLine("Cargando archivo "+file);
            StreamReader reader=new StreamReader(file);
            string content=reader.ReadToEnd().ToLower();//leemos todo el archivo y lo llevamos a minúscula para facilitar la búsqueda
            reader.Close();
            content=Utils.Transform(content);//eliminamos caracteres indeseados
            string[]words=content.Split(' ');//separamos el contenido por palabras
            int position=0;
            foreach(string word in words)
            {
                if(word!="")
                {
                    if(!Data.Keys.Contains(word))//si la palabra no ha sido cargada
                    {
                        Data[word]=new Dictionary<string,float>();//la cargamos 
                        Data[word][file]=1;//la palabra aparece una vez
                    }
                    else if(!Data[word].Keys.Contains(file))//la palabra ha sido cargada pero no en este archivo
                    {
                        Data[word][file]=1;//aparece una vez en este archivo
                    }
                    else
                    {
                        Data[word][file]++;//ha sido vista más de una vez   
                    }
                }
                position++;
            }
        }
        MaxFreqFiles();
        System.Console.WriteLine("Se han cargado "+Files.Length+" archivos");
    }
    void MaxFreqFiles()
    {
        foreach(string word in Data.Keys)
        {
            foreach(string file in Data[word].Keys)
            {
                MaxFreq[Utils.IndexOf(file,Files,0,Files.Length)]=Math.Max(MaxFreq[Utils.IndexOf(file,Files,0,Files.Length)],Data[word][file]);
            }
        }
    }
    public float[] this[string word]//devuelve un array con los tfs de cada palabra en cada archivo cargado
    {
        get
        {
            if(!Data.Keys.Contains(word))//si la palabra no fue encontrada en ningún documento devolvemos un array nulo
                return new float[Files.Length];
            float[]result=new float[Files.Length];
            foreach(string file in Data[word].Keys)
            {
                result[Utils.IndexOf(file,Files,0,Files.Length)]=Data[word][file]/MaxFreq[Utils.IndexOf(file,Files,0,Files.Length)];//devolvemos los tfs de cada palabra según el archivo en que fue encontrada
            }
            return result;
        }
        set
        {
            throw new InvalidDataException("Los datos no son modificables");
        }
    }
    string[] Partition(string[]words,int stratindex,int endindex)//método que devuelve una partición de un array de palabras
    {
        string[]result=new string[endindex-stratindex];
        int position=0;
        for(int i=stratindex;i<endindex;i++)
        {
            result[position]=words[i];
            position++;
        }
        return result;
    }
    string[] WordsImportant(string[] words, float[] words_idfs)//metodo que nos devuelve un array con las palabras que tengan relevancia
    {
        int count = 0;
        for (int i = 0; i < words.Length; i++)
            if (words_idfs[i] == 0 || words[i].Length < 4)//contamos cuantas tienen relevancia 0 o son muy cortas
                count++;
        string[] result = new string[words.Length - count];//creamos el array con el tamaño adecuado
        count = 0;
        for (int i = 0; i < words.Length; i++)//colocamos las palabras que son relevantes
            if (words_idfs[i] != 0 && words[i].Length > 3)
            {
                result[count] = words[i];
                count++;
            }
        return result;
    }
    public string FragmentWithWords(string[] query,string file, float[] words_idfs)//metodo que devuelve el fragmento de texto de un archivo que contiene la mayor cantidad de palabras de las que nos piden
    {
        StreamReader reader = new StreamReader(file);
        string content = reader.ReadToEnd().ToLower();
        reader.Close();
        content = Utils.Transform(content);
        string[] words = content.Split(' ');
        int part1 = 0;//contadores para comparar
        int part2 = 0;
        string[] Query = WordsImportant(query, words_idfs);//extraemos las palabras que nos interesan
        while (words.Length > 100)//solo devolveremos un maximo de 100 palabras
        {
            //partimos a la mitad el array
            string[] Part1 = Partition(words, 0, words.Length / 2);
            string[] Part2 = Partition(words, words.Length / 2, words.Length);
            int count = 0;
            for (int i = 0; i < Math.Min(Part1.Length, Part2.Length); i++)
            {
                count++;
                if (Query.Contains(Part1[i]))
                {
                    part1++;
                }
                if (Query.Contains(Part2[i]))
                {
                    part2++;
                }
            }
            if (Part1.Length > Part2.Length)
            {
                for (int i = count; i < Part1.Length; i++)
                {
                    if (Query.Contains(Part1[i]))
                    {
                        part1++;
                    }
                }
            }
            if (Part2.Length > Part1.Length)
            {
                for (int i = count; i < Part2.Length; i++)
                {
                    if (Query.Contains(Part2[i]))
                    {
                        part2++;
                    }
                }
            }
            if (part1 >= part2)//nos quedamos con la que mas resultados tuvo
                words = Part1;
            else
                words = Part2;
            part1 = 0;
            part2 = 0;
        }
        string result = "";
        foreach (string a in words)//generamos el texto a devolver
        {
            if (Query.Contains(a))
                result += "**" + a + "** ";
            else
                result += a + " ";
        }
        return "........" + result + "........";
    }
    public string SearchInFolder(string word)//método que busca la palabra más cercana a la que se nos pide
    {
        string result = "";
        float max_index_coincidence = 0f;
        foreach (string Word in Data.Keys)
        {
            if(Math.Abs(Word.Length-word.Length) < 4)
            {
                float coincidence = MaxIndexCoincidences(Word, word);
                if (coincidence > max_index_coincidence)//si tiene un índice de coincidencia mayor
                {
                    result = Word;
                    max_index_coincidence = coincidence;//actualizamos la palabra a devolver
                }
            }
        }
        return result;
    }
    float MaxIndexCoincidences(string word1, string word2)//metodo q devuelve el indice maximo de coincidencias entre dos palabras
    {
        //primero acomodamos las palabras de manera q la palabra mas larga sea el primer parametro
        string longerword = "";
        string shorterword = "";
        if (word1.Length > word2.Length)
        {
            longerword = word1;
            shorterword = word2;
        }
        else
        {
            longerword = word2;
            shorterword = word1;
        }
        //array que será usado posteriormente
        bool[] matched = new bool[longerword.Length];
        //convertimos ambas palabras a minusculas para facilitar la comparacion
        return MaxIndexCoincidence(longerword.ToLower(), shorterword.ToLower(), matched, longerword.Length - shorterword.Length, 0f);
    }
    float MaxIndexCoincidence(string word1, string word2, bool[] matched, int remain, float maxindex)
    {
        //mathced: array que se usa de apoyo en la comparacion
        //remain: diferencia entre las longitudes de ambas palabras
        //maxindex: maximo indice de coincidencias q se ha obtenido
        if (remain == 0)
        {
            int count = 0;//cantidad de coincidencias
            int position = 0;
            for (int i = 0; i < word2.Length; i++)
            {
                //por cada letra que coincida en posicion con la reordenacion dada de la segunda palabra
                //aumentan las coincidencias
                if (!matched[i] && word1[i + position] == word2[i])
                    count++;
                if (matched[i])
                    position++;
            }
            //devolvemos el maximo entre el cociente de las coincidencias de ambas palabras y la longitud de la primera;
            //y el maximo indice de coincidencias obtenido anteriormente 
            return Math.Max((float)count / (float)word1.Length, maxindex);
        }
        //generamos todas las posibles palabras que se forman tachando letras de la segunda palabra
        //hasta que ambas palabras tengan la misma longitud
        for (int i = 0; i < matched.Length; i++)
        {
            matched[i] = true;
            maxindex = MaxIndexCoincidence(word1, word2, matched, remain - 1, maxindex);
            matched[i] = false;
        }
        return maxindex;
    }
    public string[] Words
    {
        get
        {
            return Data.Keys.ToArray();
        }
    }
    public bool IsInFile(string word,string file)//método que nos dice si una palabra está en el archivo dado
    {
        if(Data.Keys.Contains(word))
            return Data[word].Keys.Contains(file);
        return false;
    }
    public int FilePosition(string file)//devuelve la posición de un archivo en el array interno
    {
        return Utils.IndexOf(file, Files, 0, Files.Length);
    }
    public string File(int position)//devuelve el archivo en la posición especificada dentro del array interno
    {
        return Files[position];
    }
    public int FilesWithWord(string word)//cantidad de archivos que contienen a la palabra dada
    {
        if(Data.Keys.Contains(word))
        {
            return Data[word].Keys.Count;
        }
        return 0;
    }
    public int MinDistance(string word1, string word2,int file_position)//metodo que devuelve la minima distancia entre dos palabras en un documento dado
    {
        StreamReader reader = new StreamReader(Files[file_position]);
        string content = reader.ReadToEnd().ToLower();
        content = Utils.Transform(content);
        string[] text = content.Split(' ');
        int position = 0;
        int min_distance = int.MaxValue;
        int current_distance = -1;
        bool active = false;//variable con la cual sbremos si debemos contar o no
        while (position < text.Length)
        {
            for (int i = position; i < text.Length; i++)
            {
                if (active)
                    current_distance++;
                if (text[i] == word1.ToLower())//pasamos las palabras a minusculas para facilitar la comparacion
                {
                    active = true;
                    position = i + 1;
                }
                if ((text[i] == word2.ToLower()) && (active))
                {
                    if (current_distance == -1)
                        current_distance = 0;
                    min_distance = Math.Min(min_distance, current_distance);
                    current_distance = -1;
                    active = false;
                    break;
                }
            }
            break;
        }
        if(min_distance==0)
            min_distance=1;
        return min_distance;
    }
}
