namespace MoogleEngine;

public static class Moogle
{
    static string[] ExtractOperators(string[] query, char[] operators)//metodo que devuelve un array que contiene solo los operadores
    {
        string[] result = new string[query.Length];
        for (int i = 0; i < query.Length; i++)
        {
            if (!operators.Contains(query[i][0]))
            {
                result[i] = query[i].Remove(0);
            }
            else
                result[i] = query[i];
        }
        return result;
    }
    static string[] WordsExtractor(string[] Query, char[] operators)//metodo para separar las palabras de los operadores 
    {
        int diference = 0;//variable q nos ayudara a calculare el tamaño del array resultado
        foreach (string word in Query)
        {
            foreach (char a in operators)
            {
                if (word.Contains(a))
                    diference++;
            }
        }
        string[] result = new string[Query.Length - diference];
        int count = 0;
        for (int i = 0; i < Query.Length; i++)
        {
            bool Is = false;
            foreach (char a in operators)
            {
                if (Query[i].Contains(a))
                {
                    Is = true;
                    break;
                }
            }
            if (Is)
                continue;
            result[count] = Query[i];
            count++;
        }
        return result;
    }
    static string SetValidQuery(string query, char[] operators)//metodo que elimina los operadores mal usados
    {
        while(query.StartsWith(' '))
        {
            query=query.Substring(query.IndexOf(' ')+1);
        }
        while(query.EndsWith(' '))
        {
            query=query.Substring(0,query.LastIndexOf(' '));
        }
        while(query.StartsWith('~'))
        {
            query=query.Substring(1);
        }
        while(operators.Contains(query[query.Length-1]))
        {
            query=query.Substring(0,query.Length-1);
        }
        for (int i = query.Length - 1; i > -1; i--)
        {
            if (operators.Contains(query[i]))
            {
                if (operators.Contains(query[0]) && query[0]=='~')
                {
                    int count = 0;
                    while (query[count] == '~')
                    {
                        count++;
                    }
                    query = query.Substring(count, i);
                }
                else
                    query = query.Substring(0, i);
            }
            else
                break;
        }
        bool active=false;
        char init=' ';
        string new_query="";
        bool next_position=false;
        for(int i=0;i<query.Length;i++)
        {
            if(operators.Contains(query[i]) && !active)
            {
                active=true;
                init=query[i];
                new_query+=query[i];
                continue;
            }
            if(next_position && query[i]==init && query[i]=='*')
            {
                new_query+=query[i];
                continue;
            }
            if(active && operators.Contains(query[i]) && query[i] != init)
            {
                new_query+="";
                next_position=false;
                continue;
            }
            if(active && !operators.Contains(query[i]))
            {
                new_query+=query[i];
                active=false;
                continue;
            }
            new_query+=query[i];
        }
        System.Console.WriteLine(new_query);
        return new_query;
    }
    static string SetQueryWithOperators(string query, char[] operators)//metodo que separa los operadores especiales en una consulta
    {
        for(int i=0;i<query.Length;i++)
        {
            if(operators.Contains(query[i]))
            {
                int position=i;
                char init=query[i];
                do
                {
                    i++;
                }while(query[i]==init);
                
                string part1=query.Substring(0,position);
                string oper=query.Substring(position,i-position);
                string part2=query.Substring(i);
                query=part1+' '+oper+' '+part2;
                i+=2;
                continue;
            }
        }
        if(query[0]==' ')
            query=query.Substring(1);
        return Utils.ClearSpaces(query);
    }
    public static SearchResult Query(string query) 
    {
        if(!string.IsNullOrEmpty(query))
        {
            query = query.ToLower();
            char[] operators = new[] { '!', '^', '*', '~' };
            query = SetValidQuery(query, operators);
            query=Utils.ClearSpaces(query);
            query = SetQueryWithOperators(query, operators);
            System.Console.WriteLine(query);
            System.Console.WriteLine("Aqui cambio");
            string[] Query = query.Split(' ');
            //Query=CorrectQuery(Query,operators);
            // foreach(string a in Query){
            //     System.Console.WriteLine(a);
            // }
            string[]words = WordsExtractor(Query, operators);
            words = Utils.DeleteDuplicated(words);
            string[] Operators = ExtractOperators(Query, operators);
            SearchResult result = Egine.Query(Query, words, Operators);
            return result;
        }
        return new SearchResult();
    }
}


