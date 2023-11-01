using System.Formats.Asn1;

namespace Models;

public class PetModel
{
    public int id;
    public Category category;
    public String name;
    public String[] photoUrls;
    public Tag[] tags;
    public String status;

}

public class Category
{
    public int id;
    public String name;

//Construtor  - conecta
    public Category(int id, String name) 
    {
        this.id = id;
        this.name = name;
    }

}

public class Tag
{
    public int id;
    public String name;

    public Tag(int id, String name)
    {
        this.id = id;
        this.name = name;
    }
}

/*{
  "id": 0,
  "category": {
    "id": 0,
    "name": "string"
  },
  "name": "doggie",
  "photoUrls": [
    "string"
  ],
  "tags": [
    {
      "id": 0,
      "name": "string"
    }
  ],
  "status": "available"
}*/