//1- Bibliotecas
using Models;
using Newtonsoft.Json; //dependencia para o JsonConvert
using RestSharp;

//2- NameSpace
namespace Pet;

//3- Classe
public class PetTest
{
    //3.1- Atributos
    //Endereço da API
    private const string BASE_URL = "https://petstore.swagger.io/v2/";
    //public String Token. * Seria uma forma de fazer ( extrair o token)

    //3.2- Funções e Métodos 


    //Função de Leitura de dados a partir de um arquivo CSV
    public static IEnumerable<TestCaseData> getTestData()
    {
        String caminhoMassa = @"/home/jessica/Documents/Iterasys/PETSTORE139/fixtures/pets.csv";

        using var reader = new StreamReader(caminhoMassa);

        //pula a primeira linha com os cabeçalhos

        reader.ReadLine();

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(",");

            yield return new TestCaseData(int.Parse(values[0]), int.Parse(values[1]),values[2], values[3],values[4],values[5],values[6],values[7]);
        }
    }

    [Test, Order(1)]
    //void - método
    public void PostPetTest()
    {
        //Configura
        // Instancia o objeto do ThreadPriority RestClient com o endereço da API
        var client = new RestClient(BASE_URL);

        //Instancia o objeto do tipo RestRequest com o complemento de endereço
        //Como "pet" e configurando o método para ser um post(inclusão)
        var request = new RestRequest("pet", Method.Post);

        //armazena o conteúdo do arquivo pet.json na memória (variável)
        String jsonBody = File.ReadAllText(@"/home/jessica/Documents/Iterasys/PETSTORE139/fixtures/pet1.json");

        //Adiciona na requisição o conteúdo do arquivo pet1.json
        request.AddBody(jsonBody);

        //Executa
        //Executa a requisição conforme a configuração realizada
        //Guarda o json retornado no objeto response
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        //Exibe o responseBody no console
        Console.WriteLine(responseBody);

        // Valide que na resposta, o status code , é igual ao resultado esperado
        //Status 200 significa que a comunicação deu certo, precisa validar o conteúdo da resposta
        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        //Valida o petId
        int petId = responseBody.id;
        Assert.That(petId, Is.EqualTo(688977));

        //Valida o nome do pet na resposta
        String name = responseBody.name.ToString();
        Assert.That(name, Is.EqualTo("Sansa"));

        // ou Assert.That(responseBody.name.ToString(), Is.EqualTo("Sansa"));

        //Valida o Status do pet na resposta
        String status = responseBody.status.ToString();
        Assert.That(status, Is.EqualTo("available"));

        //Armazenar os dados obtidos para usar nos próximos testes
        Environment.SetEnvironmentVariable("petId", petId.ToString());

    }

    [Test, Order(2)]
    public void GetPetTest()
    {
        //Configura
        int petId = 688977;         //Campo de pesquisa 
        String petName = "Sansa";  //Resultado Esperado
        String categoryName = "dog";
        String tagsName = "vacinado";

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"pet/{petId}", Method.Get);   //forma de concatenação $ na frente

        //Executa
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
        Console.WriteLine(responseBody);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.id, Is.EqualTo(petId));
        Assert.That((String)responseBody.name, Is.EqualTo(petName));
        Assert.That((String)responseBody.category.name, Is.EqualTo(categoryName));
        Assert.That((String)responseBody.tags[0].name, Is.EqualTo(tagsName));
    }

    [Test, Order(3)]
    public void PutPetTest()
    {
        //Configura
        //Os dados de entrada vão formar o body da alteração
        //Put precisa enviar todos os dados, mesmo os que não vão ser alterados
        //Vamos usar uma classe de modelo
        PetModel petModel = new PetModel();  //classe pai PetModel   objeto petModel herda da classe pai - instanciando
        petModel.id = 688977;
        petModel.category = new Category(1, "dog");
        petModel.name = "Sansa";
        petModel.photoUrls = new string[] { "" };
        petModel.tags = new Tag[] { new Tag(1, "vacinado"), new Tag(2, "castrado") };
        petModel.status = "pending";

        //Transformar o modelo acima em um arquivo Json

        String jsonBody = JsonConvert.SerializeObject(petModel, Formatting.Indented);
        Console.WriteLine(jsonBody);

        var client = new RestClient(BASE_URL);
        var request = new RestRequest("pet", Method.Put);
        request.AddBody(jsonBody);

        //Executa
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
        Console.WriteLine(responseBody);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.id, Is.EqualTo(petModel.id));
        Assert.That((String)responseBody.tags[1].name, Is.EqualTo(petModel.tags[1].name));
        Assert.That((String)responseBody.status, Is.EqualTo(petModel.status));
    }

    [Test, Order(4)]
    public void DeletePetTest()
    {
        //Configura
        String petId = Environment.GetEnvironmentVariable("petId");

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"pet/{petId}", Method.Delete);
        //Executa

        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.code, Is.EqualTo(200));
        Assert.That((String)responseBody.message, Is.EqualTo(petId));

        Console.WriteLine(responseBody);
    }

    [TestCaseSource("getTestData", new object[]{}), Order(5)]
    //void - método
    public void PostPetDDTest(int petId, int categoryId, String categoryName, 
                             String petName, String photoUrls, String tagsIds,
                             String tagsName, String status)
    {
        //Configura

        PetModel petModel = new PetModel();  //classe pai PetModel   objeto petModel herda da classe pai - instanciando
        petModel.id = petId;
        petModel.category = new Category(categoryId, categoryName);
        petModel.name = petName;
        petModel.photoUrls = new string[] {photoUrls};

        //Codigo para gerar as multiplas tags que o pet pode ter
        String[] tagsIdsList = tagsIds.Split(";");   //Ler 
        String[] tagsNameList = tagsName.Split(";");  // Ler
        List<Tag> tagList = new List<Tag>(); //Gravar depois do For

        for (int i = 0; i < tagsIdsList.Length; i++)
        {
            int tagId = int.Parse(tagsIdsList[i]);  //[i] índice
            String tagName = tagsNameList[i];

            Tag tag = new Tag(tagId, tagName);
            tagList.Add(tag);
        } 

        petModel.tags = tagList.ToArray();
        petModel.status = status;

        //A Estrutura de dados está pronta, agora vamos serializar (transforma em Json)
        String jsonBody = JsonConvert.SerializeObject(petModel, Formatting.Indented);
        Console.WriteLine(jsonBody);

        // Instancia o objeto do ThreadPriority RestClient com o endereço da API
        var client = new RestClient(BASE_URL);

        //Instancia o objeto do tipo RestRequest com o complemento de endereço
        //Como "pet" e configurando o método para ser um post(inclusão)
        var request = new RestRequest("pet", Method.Post);

        //Adiciona na requisição o conteúdo do arquivo pet1.json
        request.AddBody(jsonBody);

        //Executa
        //Executa a requisição conforme a configuração realizada
        //Guarda o json retornado no objeto response
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        //Exibe o responseBody no console
        Console.WriteLine(responseBody);

        // Valide que na resposta, o status code , é igual ao resultado esperado
        //Status 200 significa que a comunicação deu certo, precisa validar o conteúdo da resposta
        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        //Valida o petId       
        Assert.That((int)responseBody.id, Is.EqualTo(petId));

        //Valida o nome do pet na resposta
         Assert.That((String)responseBody.name, Is.EqualTo(petName));

         //Valida o Status do pet na resposta
         Assert.That((String)responseBody.status, Is.EqualTo(status));

    }

    [Test,Order(6)]

    public void GetUserLoginTest()
    {
        //Configura
        String username = "Raul";
        String password = "Test";

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"user/login?username={username}&password={password}", Method.Get);
                
        //Executa
         var response = client.Execute(request);

        //Valida

        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.code, Is.EqualTo(200));
        String message = responseBody.message;
        String token = message.Substring(message.LastIndexOf(":")+1); //Trazer o número do token

        Console.WriteLine($"token = {token}");
        Environment.SetEnvironmentVariable("token", token);
        
    }


}
