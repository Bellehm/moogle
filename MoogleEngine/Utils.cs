namespace MoogleEngine;

public static class Utils
{
    public static string[] EnumerateFiles(string location="")
    {
        //location: carpeta donde se buscara, si esta no es dada, 
        //se buscara dentro de la carpeta superior a la carpeta raiz del programa
        //string[] CurrentLocation = Directory.GetCurrentDirectory().Split('\\');
        string FileLocation = Directory.GetCurrentDirectory();
        FileLocation=FileLocation.Substring(0,FileLocation.LastIndexOf('\\'));
        // for (int i = 0; i < CurrentLocation.Length - 1; i++)
        //     FileLocation += CurrentLocation[i] + '/';
        FileLocation += '\\'+location;
        List<string> Files = new List<string>();
        return Directory.EnumerateFiles(FileLocation).ToArray();
    }
    public static float Max(float[] vector)//devuelve el maximo valor del array dado
    {
        float result=vector[0];
        for(int i=1;i<vector.Length;i++)
        {
            if(vector[i]>result)
                result=vector[i];
        }
        return result;
    }
    public static string Transform(string value)//metodo que elimina los caracteres incomodos al leer un txt
    {
        char[] guide = { '\r', '\n', '(', ')', '*', '{', '}', '´', '`' ,',','.',':'};
        foreach (char a in guide)
        {
            value = value.Replace(a, ' ');
        }
        return value;
    }
    public static int IndexOf(string word, string[] words, int start_position, int end_position)//nos devuelve el indice de una palabra en un array en un intervalo dado
    {
        for (int i = end_position - 1; i >= start_position; i--)
        {
            if (words[i] == word)
                return i;
        }
        return -1;
    }
    public static bool IsNull(float[] values)//metodo que nos dice si en un array todos susu valores son nulos 
    {
        foreach (float value in values)
            {
                if(value.Equals(0f))
                    continue;
                if(value.Equals(float.NaN))
                    continue;
                return false;
            }
        return true;
    }
    public static string[] DeleteDuplicated(string[] words)//metodo q elimina las palabras duplicadas
    {
        bool[] aux = new bool[words.Length];
        int count = 0;
        for (int i = 1; i < words.Length; i++)
        {
            if (Contains(words[i], words, 0, i) || words[i] == "")
            {
                aux[i] = true;
                count++;
            }
        }
        string[] result = new string[words.Length - count];
        count = 0;
        for (int i = 0; i < words.Length; i++)
        {
            if (!aux[i])
            {
                result[count] = words[i];
                count++;
            }
        }
        return result;
    }
    public static bool Contains(string word, string[] words, int start, int end)//devuelve si un array contiene o no una palabra dada en un intervalo dado
    {
        for (int i = start; i < end; i++)
            if (words[i] == word)
                return true;
        return false;
    }
    public static string ClearSpaces(string text)//metodo que elimina los espacios en blanco innecesarios
    {
        while (text.IndexOf("  ") >= 0)
        {
            text = text.Replace("  ", " ");
        }
        return text;
    }
    public static int[] Merge(int[] a, int[] b)//metodo que mezcla dos array's de manera ordenada
    {
        int[] result=new int[a.Length+b.Length];
        int pos=0;
        int pos_b=0;
        if(a.Length==0)
        {
            for(int i=0;i<b.Length;i++)
            {
                int temp=b[i];
                result[i]=temp;
            }
        }
        else if(b.Length==0)
        {
            for(int i=0;i<a.Length;i++)
            {
                int temp=a[i];
                result[i]=temp;
            }
        }
        else
        {
            for(int i=0;i<a.Length;i++)
            {
                if(a[i]<b[pos_b])
                {
                    int temp=a[i];
                    result[pos]=temp;
                    pos++;
                }
                else
                {
                    while(pos_b<b.Length && b[pos_b]<a[i])
                    {
                        int temp=b[pos_b];
                        result[pos]=temp;
                        pos++;
                        pos_b++;
                    }
                    if(pos_b==b.Length)
                    {
                        for(int j=i;j<a.Length;j++)
                        {
                            int temp=a[i];
                            result[pos]=temp;
                            pos++;
                            i=j;
                        }
                    }
                }
            }
        }
        return result;
    }
}