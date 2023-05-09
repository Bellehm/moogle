namespace MoogleEngine;

public static class Egine
{
    public static bool Initialized=false;//variable usada para saber si la clase ha sido inicializada o no
    static string root = "";//ruta del diccionario
    static DataFolder Data;//Cargamos los datos
    static Dictionary dictyionary;//diccionario que usaremos
    public static Dictionary<string, string[]> WordsToSearch{ get; private set;}//lo usaremos para realizar una busqueda mas exacta 
    //en cada documento, ya que guardaremos cuales son las palabras especificas a buscar en cada documento para devolver un fragmento de el
    static float[] Words_idf;//array con la frecuencia inversa de cada palabra
    public static void Init()//metodo utilizado para inicializar la clase
    {
        Initialized=true;
        Data = new DataFolder("Content");//cargamos los archivos
        WordsToSearch= new Dictionary<string, string[]>();
        for(int i = 0; i < Data.Count; i++)
        {
            WordsToSearch[Data.File(i)] = null;
        }
        root =Directory.GetCurrentDirectory();
        root=root.Substring(0,root.LastIndexOf('\\'));
        root+="\\Diccionario\\Diccionario.txt";
        dictyionary = new Dictionary(root);//cargamos el diccionario
    }
    //en revision este metodo
    static float[,] TFS(string[] words)//metodo que devuelve una matriz con los tf's de cada palabra en cada documento
    {
        float[,] result = new float[words.Length, Data.Count];
        for(int i = 0; i < words.Length; i++)
        {
            float[] tfs = Data[words[i]];
            float max=Utils.Max(tfs); 
            if(max != 0)
            {
                for(int j = 0; j < Data.Count; j++)
                {
                    result[i, j] = tfs[j] / max;
                }
            }           
        }
        return result;
    }
    static SearchItem[] Sort(SearchItem[] items)//metodo que ordena el array de SearchItem segun sus scores
    {
        for (int i = 0; i < items.Length; i++)
        {
            for (int j = i; j < items.Length; j++)
            {    if (items[j].Score > items[i].Score)
                {
                    SearchItem temp = items[j];
                    items[j] = items[i];
                    items[i] = temp;
                }
            }
        }
        return items;
    }
    static float[] IDFS(float[,] tfs)//metodo que calcula el idf de cada palabra
    {
        float[] idfs = new float[tfs.GetLength(0)];
        for (int i = 0; i < tfs.GetLength(0); i++)
        {
            int docs_with_word = 0;
            for (int j = 0; j < tfs.GetLength(1); j++)
            {
                if (tfs[i, j] > 0)
                    docs_with_word++;
            }
            if ((float)docs_with_word / (float)tfs.GetLength(1) > 0.9f)//si más del 90% de los documentos contienen a la palabra
                idfs[i] = 0;//no tiene relevancia
            else if (docs_with_word == 0)//si ningun documento la contiene
                idfs[i] = -1;//para saber que la palabra no fue hallada
            else
                idfs[i] = (float)Math.Log10((float)tfs.GetLength(1) / (float)docs_with_word) + 1;//devolvemos su idf
        }
        Words_idf = idfs;
        return idfs;
    }
    static float[,] Query_TFS_IDFS(float[] idfs,float[,] tfs)//metodo que calcula el tf-idf de cada palabra en cada documento y lo almacena en una matriz
    {
        for(int i=0;i<tfs.GetLength(1);i++)
        {
            for(int j=0;j<tfs.GetLength(0);j++)
            {
                tfs[j,i]=Math.Abs(idfs[j]*tfs[j,i]);
            }
        }
        return tfs;
    }
    static float[] Query_tf_idf(string[] query)//metodo que devuelve el vector tf-idf de una consulta 
    {
        string[] words=Utils.DeleteDuplicated(query);//eliminamos todas las palabras repetidas
        float[] vector=new float[words.Length];
        for(int i=0;i<vector.Length;i++)
        {
            for(int j=0;j<query.Length;j++)
            {
                if(words[i]==query[j])
                    vector[i]++;
            }
        }//aplicamos la fórmula para obtener los pesos de cada palabra y construimos el vector consulta
        float max=Utils.Max(vector);
        for(int i=0;i<vector.Length;i++)
            vector[i]/=max;
        for(int i=0;i<vector.Length;i++)
        {
            vector[i]=(float)Math.Log10((float)Data.Count/(float)Data.FilesWithWord(words[i]))*(0.5f+(1-0.5f)*vector[i]);
        }
        return vector;
    }
    static float[] Scores(float[] query_tf_idf, float[,] tfs_idfs)//metodo que calcula el coseno del angulo entre el vector query_tf_idf y cada uno de los vectores de tfs_idfs
    {//aplicamos la fórmula del coseno del ángulo entre dos vectores
        float[] result=new float[tfs_idfs.GetLength(1)];
        for(int i=0;i<tfs_idfs.GetLength(1);i++)
        {
            float query_square=0f;
            float squares=0f;
            float sum=0f;
            for(int j=0;j<tfs_idfs.GetLength(0);j++)
            {
                query_square+=(float)Math.Pow(query_tf_idf[j],2);
                squares+=(float)Math.Pow(tfs_idfs[j,i],2);
                sum+=query_tf_idf[j]*tfs_idfs[j,i];
            }
            if(query_square*squares==0)
                result[i]=0;
            else
                result[i]=sum/((float)Math.Sqrt(query_square*squares));
        }   
        return result;
    }
    static string SearchParentIn(string word, string file)//metodo que busca un sinonimo en el archivo o la palabra derivada de ella en caso de no encontrar uno
    {
        string parent = "";
        string[] sinonumous = dictyionary[word];
        int file_position = Data.FilePosition(file);
        if (sinonumous != null)
        {
            float tf = 0;
            foreach(string Word in sinonumous)
            {
                if (Data.IsInFile(Word, file))
                {
                    float[] tfs = Data[Word];
                    if (tf > tfs[file_position])
                    {
                        parent = Word;
                        tf = tfs[file_position];
                    }
                }
            }
            return parent;
        }
        int max_parent_index = 0;
        foreach(string Word in Data.Words)
        {
            if ((Math.Abs(Word.Length-word.Length) < 4) && Data.IsInFile(Word, file))
            {
                int parent_index = ParentIndex(Word, word);
                if (parent_index > max_parent_index)
                {
                    parent = Word;
                    max_parent_index = parent_index;
                }
            }
        }
        if (max_parent_index < word.Length - 2)
            return null;
        return parent;
    }
    static int ParentIndex(string root, string word)//metodo que devuelve un valor entero mas grande entre mas se derive la palabra "word" de la palabra "root"
    {
        string Root = "";
        string Word = "";
        if (Math.Abs(word.Length - root.Length) < 3)
        {
            if (word.Length < root.Length)
            {
                Root = word;
                Word = root;
            }
            else
            {
                Root = root;
                Word = word;
            }
        }
        //convertimos ambas palabras a minusculas para facilitar la comparacion
        Root = Root.ToLower();
        Word = Word.ToLower();
        int count = 0;
        for (int i = 0; i < Root.Length; i++)
        {
            if (Root[i] == Word[i])
                count++;
            else
                break;
        }
        return count;
    }
    static float[] TF_IDF_With_Operators(string[] Query,string[] Words,string[] Operators)//metodo que aplica los operadores a la consulta
    {
        float[,] tfs = TFS(Words);//calculamos los tf's
        float[] idfs = IDFS(tfs);//calculamos los idf's
        float[] query_tf_idf=Query_tf_idf(Words);//obtenemos el vector consulta
        for(int i = 0; i < tfs.GetLength(1); i++)
        {
            string[] Words_Copy = new string[Words.Length];
            Array.Copy(Words,Words_Copy, Words.Length);//hacemos una copia de las palabras
            for(int j = 0; j < tfs.GetLength(0); j++)
            {
                if (tfs[j, i] == 0)//si no encontramos la palabra
                {
                    string parent = SearchParentIn(Words[j], Data.File(i));//buscamos otra relacionada
                    if (parent != null)
                    {
                        Words_Copy[j] = parent;//la guardamos en la copia que hicimos
                        tfs[j, i] = -1 * Data[parent][i] /(float)(Math.Log10(Math.Abs(parent.Length - Words[j].Length)) + 1f);
                        //se guardan valores negativos para no alterar los resultados a la hora de aplicar los operadores de busqueda
                    }
                }
            }
            WordsToSearch[Data.File(i)] = Words_Copy;//almacenamos las palabras que debemos buscar en el fragmento de texto a devolver específico de ese documento
        }
        for(int i = 0; i < Operators.Length; i++)//aplicamos los operadores
        {
            if (Operators[i].Contains("!"))
            {
                int position = Utils.IndexOf(Query[i + 1],Words,0,Words.Length);
                for(int j = 0; j < tfs.GetLength(1); j++)
                {
                    if (tfs[position, j] > 0)//si la plabra originalmente buscada fue encontrada
                    {
                        for (int k = 0; k < tfs.GetLength(0); k++)//hacemos nulo el vector del documento j
                            tfs[k, j] = 0;
                    }
                }
            }
            else if (Operators[i].Contains("^"))
            {
                int position = Utils.IndexOf(Query[i + 1],Words,0,Words.Length);
                for(int j = 0; j < tfs.GetLength(1); j++)
                {
                    if (tfs[position, j] <= 0)//si la palabra originalmente buscada no fue hallada en el documento j
                    {
                        for (int k = 0; k < tfs.GetLength(0); k++)//hacemos nulo el vector del documento j
                            tfs[k, j] = 0;
                    }
                }
            }
            else if (Operators[i].Contains("*"))
            {
                int position = Utils.IndexOf(Query[i + 1],Words,0,Words.Length);
                //aumentamos el peso de la palabra correspondiente
                idfs[position] *= Operators[i].Length + 1;
                query_tf_idf[position]*=Operators[i].Length+1;
            }
            else if(Operators[i].Contains("~"))
            {
                int position1 = Utils.IndexOf(Query[i - 1],Words,0,Words.Length);
                int position2 = Utils.IndexOf(Query[i + 1],Words,0,Words.Length);
                //disminuimos el peso de las palabras correspondientes según la mínima distancia entre ellas en el documento respectivo
                for (int j = 0; j < tfs.GetLength(1); j++)
                {
                    if (tfs[position1, j] > 0)
                    {
                        int distance = Data.MinDistance(Query[i - 1], Query[i + 1], j);
                        tfs[position1, j] /= (float)Math.Log10(distance);
                    }
                    if (tfs[position2, j] > 0)
                    {
                        int distance = Data.MinDistance(Query[i - 1], Query[i + 1], j);
                        tfs[position2, j] /= (float)Math.Log10(distance);
                    }
                }
            }
        }
        //calculamos los vectores documentos
        float[,] tf_idf = Query_TFS_IDFS(idfs, tfs);
        float[] scores=Scores(query_tf_idf,tf_idf);//hallamos la similitud entre los vectores documentos y el vector consulta
        return scores;
    }
    public static SearchResult Query(string[] query, string[] Words, string[] Operators)
    {
        string suggestion = "";
        float[] documents_score = TF_IDF_With_Operators(query, Words, Operators);//obtenemos el score de cada documento
        if (Utils.IsNull(documents_score))
        {
            //generamos los SearchItem's
            SearchItem[] items = new SearchItem[0];
            if(Operators.Contains("!") && !Operators.Contains("~") && !Operators.Contains("^") && !Operators.Contains("*"))
            {
                //solo se han utilizado los operadores de negacion
                //devolvemos todos los documentos que no contienen dichas palabras
                items=new SearchItem[documents_score.Length];
                for(int i = 0; i < items.Length; i++)
                {
                    string snippet = Data.FragmentWithWords(Words, Data.File(i), Words_idf);
                    string title = Data.File(i).Substring(Data.File(i).LastIndexOf('\\'));
                    title = title.Substring(1, title.IndexOf('.') - 1);//extraemos el titulo
                    SearchItem item = new SearchItem(title, snippet, documents_score[i]);
                    items[i]=item;
                }
            }
            bool nulo=true;
            foreach(var a in items)
                if(a!=null)
                    nulo=false;
            if(nulo)
            {
                for(int i=0;i<Words_idf.Length;i++)
                {
                    if(Words_idf[i].Equals(-1f))
                    {
                        Words[i]=Data.SearchInFolder(Words[i]);
                    }
                    else
                    {
                        Words[i]=Words[i];
                    }
                }
                foreach(string word in Words)
                {
                    suggestion+=word+" ";
                }
            }
            WordsToSearch=new Dictionary<string, string[]>();
            return new SearchResult(items,suggestion);
        }
        else
        {
            //generamos los resultados
            List<SearchItem> items = new List<SearchItem>();
            for(int i = 0; i < documents_score.Length; i++)
            {
                if (documents_score[i] != 0)
                {
                    string snippet = Data.FragmentWithWords(WordsToSearch[Data.File(i)], Data.File(i), Words_idf);
                    string title = Data.File(i).Substring(Data.File(i).LastIndexOf('\\'));
                    title = title.Substring(1, title.IndexOf('.') - 1);//extraemos el titulo
                    SearchItem item = new SearchItem(title, snippet, documents_score[i]);
                    items.Add(item);
                }
            }
            SearchItem[] Items = items.ToArray();
            if(items.Count==0){
                Items=new SearchItem[0];
            }
            Items = Sort(Items);
            //lo ordenamos
            if ((float)Items.Length / (float)Data.Count < 0.1f)
            {
                //si obtenemos pocos resultados
                //calculamos la sugerencia
                for(int i = 0; i < Words_idf.Length; i++)
                {
                    if (Words_idf[i] == -1)
                    {
                        Words[i] = Data.SearchInFolder(Words[i]);
                    }
                    else
                    {
                        Words[i] = Words[i];
                    }
                }
                foreach(string word in Words)
                {
                    suggestion += word + " ";
                }
                return new SearchResult(Items,suggestion);
            }
            WordsToSearch=new Dictionary<string, string[]>();
            return new SearchResult(Items, suggestion);
        }
    }
}
