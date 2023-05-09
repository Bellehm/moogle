# Moogle!

Moogle!, es una aplicación web desarrollada con la tecnología .NetCore 6.0, la cual cuenta con una parte no visual, que es la lógica detrás de la aplicación y se encuentra escrita en lenguaje C#, y una interfaz visual diseñada usando Blazor como framework de desarrollo. El objetivo de esta aplicación es, dada una palabra o frase escrita por el usuario, esta realice una búsqueda de dicha frase en un conjunto de documentos y muestre aquellos que sean más afines con la consulta realizada.

Para lograr dicho objetivo se ha utilizado el modelo vectorial, que es uno de los modelos de recuperación de información más utilizados en la actualidad. Este modelo considera un vector consulta (v) y los vectores documentos (di, i un número natural), para luego hallar la similitud de cada uno de estos vectores con el vector v.

Cabe destacar que Moogle! cuenta con varias funcionalidades que permiten que la búsqueda realizada sea más exahustiva y específica.

> Soporte operadores de búsqueda para hacer más específica la consulta.

> Se consideran aquellos documentos que aunque no contengan las palabras específicas de la consulta, si contienen palabras con el mismo significado o que provienen de la misma raiz.

> Se brinda una sugerencia de búsqueda a realizar en caso de que la consulta dada muestre muy pocos resultados.

La lógica de la aplicación cuenta con 7 clases que se relacionan entre sí para cumplir con el propósito de la aplicación. Dichas clases son:

> # DataFolder:
    Clase encargada de almacenar toda la información relativa a los documentos en los que se realizará la búsqueda, además de realizar todas las operaciones que involucran interactuar directamente con los documentos.
> # Dictionary:
    Clase que almacena un conjunto de palabras agrupadas por su significado de manera que si dos palabras significan lo mismo (son sinónimos) entonces estarán en el mismo grupo.
> # Egine:
    Es la clase que realiza todo el proceso de búsqueda de la consulta dada.
> # Utils:
    Es donde se definen un conjunto de métodos que se utilizarán por las demas clases y facilitan el proceso de búsqueda.
> # SearchItem:
    Clase usada para acomodar de manera organizada la información referente a un documento que sea afín con la consulta dada.
> # SearchResult:
    Se encarga de organizar todos los resultados de la búsqueda realizada.
> # Moogle:
    Clase que se comunica con el servidor y que recibe la consulta para luego preparar y ejecutar la búsqueda, una vez obtenidos los resultados, esta se los envía al servidor.


## Flujo de la aplicación ##    
En el momento en que se inicia la aplicación, automáticamente se inician las clases DataFolder y Dictionary, las cuales cargan un diccionario de sinónimos y los datos de los documentos respectivamente. Luego la aplicación está lista para recibir consultas.

Cuando una consulta es introducida en la aplicación, esta es recibida inmediatamente por el método Query de la clase Moogle, seguidamente esta es filtrada para obtener así una consulta válida, se separan los operadores de las palabras y se pasan por separado esta información a la clase Egine para que ejecute la búsqueda. Después que la clase Egine recibe estos datos, le solicita a la clase DataFolder todos los datos necesarios para ejecutar la búsqueda y apoyándose en la clase Dictionary realiza una búsqueda de mayor profundidad. Una vez terminada la búsqueda, los resultados, que son organizados usando las clases "SearchItem" y "SearchResult", son organizados por orden prioridad con respecto a la consulta y son enviados a la clase Moogle, para luego ser enviados al servidor y mostrados al usuario. En caso de obtener muy pocos resultados, también se muestra una sugerencia de búsqueda que probablemente obtenga mayor cantidad de resultados.