using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;


// создание клиента
using TcpClient tcpClient = new TcpClient();
await tcpClient.ConnectAsync("127.0.0.1", 8888);
var stream = tcpClient.GetStream();

using (stream)
using (StreamReader reader = new StreamReader(stream))
using (StreamWriter writer = new StreamWriter(stream))
{
    while (true)
    {
        string userInput = "";
        Console.Write("Enter action (1 - get a file, 2 - save a file, 3 - delete a file): ");
        string command = Console.ReadLine();
        userInput += command;
        writer.WriteLine(command);
        writer.Flush();
        switch (command)
        {
            case "1":
                Console.Write("Do you want to get the file by name or by id (1 - name, 2 - id):");
                string NameorId = Console.ReadLine();
                userInput += "|" + NameorId;
                writer.WriteLine(NameorId);
                writer.Flush();
                switch (NameorId)
                {
                    case "1":
                        Console.Write("Enter name:  ");
                       
                        writer.WriteLine(Console.ReadLine());
                        writer.Flush();
                        Console.WriteLine("The request was sent.");

                        string response1 = reader.ReadLine();
                        if (response1 == "200")
                        {
                            Console.Write("The file was downloaded! Specify a name for it:  ");
                            writer.WriteLine(Console.ReadLine()); 
                            writer.Flush();
                            Console.WriteLine("File saved on the hard drive!");
                        }
                        else if (response1 == "404")
                        {
                            Console.WriteLine("The response says that this file is not found!");
                        }
                        break;
                    case "2": //byID
                        Console.Write("Enter ID:  ");
                        string serverFileID = Console.ReadLine();
                        writer.WriteLine(serverFileID);
                        writer.Flush();
                        Console.WriteLine("The request was sent.");

                        string response2 = reader.ReadLine();
                        if (response2 == "200")
                        {
                            Console.Write("The file was downloaded! Specify a name for it:  ");
                            writer.WriteLine(Console.ReadLine()); //userFileName
                            writer.Flush();
                            Console.WriteLine("File saved on the hard drive!");
                        }
                        else if (response2 == "404")
                        {
                            Console.WriteLine("The response says that this file is not found!");
                        }
                        break;
                }
                break;
            case "2": //save a file
                Console.Write("Enter name of the file:  ");
                string userFileName = Console.ReadLine();
                writer.WriteLine(userFileName);
                writer.Flush();
                Console.Write("Enter name of the file to be saved on server:  ");
                string serverFileName = Console.ReadLine();
                writer.WriteLine(serverFileName);
                writer.Flush();
                Console.WriteLine("The request was sent.");

                string response = reader.ReadLine();
                if (response == "200")
                {
                    string IdOfFile = reader.ReadLine();
                    Console.WriteLine($"Response says that file is saved! ID = {IdOfFile}");
                }
                else if (response == "404")
                {
                    Console.WriteLine("The response says that the file can't be created");
                }
                break;
            case "3": //delete
                Console.Write("Do you want to delete the file by name or by id (1 - name, 2 - id):");
                string NameorIdDel = Console.ReadLine();
                writer.WriteLine(NameorIdDel);
                writer.Flush();
                switch (NameorIdDel)
                {
                    case "1": 
                        Console.Write("Enter name:  ");
                        writer.WriteLine(Console.ReadLine());
                        writer.Flush();
                        Console.WriteLine("The request was sent.");

                        Console.WriteLine("The response says that this file is not found!");
                        break;
                    case "2": //byID
                        Console.Write("Enter ID:  ");
                        string serverFileID = Console.ReadLine();
                        writer.WriteLine(serverFileID);
                        writer.Flush();
                        Console.WriteLine("The request was sent.");
                        break;
                }
                string responseDel = reader.ReadLine();
                if (responseDel == "200")
                {
                    Console.WriteLine("The response says that this file was deleted successfully!");
                }
                else
                {
                    Console.WriteLine("The response says that this file is not found!");
                }
                break;
            case "exit":
                tcpClient.Close();
                stream.Close();
                reader.Close();
                writer.Close();
                break;
        }
        if (tcpClient.Connected == false) { break; }


    }
}
