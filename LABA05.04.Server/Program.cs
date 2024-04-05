using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

var tcpListener = new TcpListener(IPAddress.Any, 8888);

try
{
    tcpListener.Start();    // запускаем сервер
    Console.WriteLine("Server started!");

    while (true)
    {
        try
        {
            // получаем подключение в виде TcpClient
            var tcpClient = await tcpListener.AcceptTcpClientAsync();
            // создаем новый поток для обслуживания нового клиента
            new Thread(async () => await ProcessClientAsync(tcpClient)).Start();
        }
        catch
        {
            break;
        }

    }
}
finally
{
    tcpListener.Stop();
}
// обрабатываем клиент
async Task ProcessClientAsync(TcpClient tcpClient)
{
    string filePathServer = @"C:\Users\kristik\OneDrive\ynik\gittt\LABA05.04.Server";
    string filePathClient = @"C:\Users\kristik\OneDrive\ynik\gittt\LABA05.04.Client";
    string filePathFilesDB = @"C:\Users\kristik\OneDrive\ynik\gittt\id.txt";

    SortedDictionary<int, string> serverFiles = new SortedDictionary<int, string>();

    var stream = tcpClient.GetStream();
    using (stream)
    using (StreamReader reader = new StreamReader(stream))
    using (StreamWriter writer = new StreamWriter(stream))
    {
        //заполнение словаря файлами хранящимися на сервере
        ReadAllServerFiles(filePathFilesDB, serverFiles);
        while (true)
        {
            string command = reader.ReadLine(); 
            string serverFileName = "";
            string userFileName = "";
            string serverFilePath = "";
            switch (command)
            {
                case "1"://get a file
                    string nameorId = reader.ReadLine(); //считываем команду по имени либо айди

                    if (nameorId == "1")// By Name
                    {
                        serverFileName = reader.ReadLine();
                    }
                    else if (nameorId == "2")// By ID
                    {
                        string serverFileId = reader.ReadLine();
                        serverFileName = GetFileNameById(int.Parse(serverFileId), serverFiles);
                        if (serverFileName == "")//значит файл не существует
                        {
                            writer.WriteLine("404");
                            writer.Flush();
                            break;
                        }
                    }
                    serverFilePath = Path.Combine(filePathServer, serverFileName);//путь к файлу внутри сервера

                    if (!File.Exists(serverFilePath))
                    {

                        writer.WriteLine("404");
                        writer.Flush();
                    }
                    else
                    {
                        try
                        {
                            File.Copy(serverFilePath, Path.Combine(filePathClient, serverFileName));//"загружаем" файл с сервера клиенту
                            writer.WriteLine("200");
                            writer.Flush();

                            userFileName = reader.ReadLine();
                            if (userFileName != "")
                            {
                                File.Move(Path.Combine(filePathClient, serverFileName), Path.Combine(filePathClient, userFileName));//если клиент дал имя файлу то переименовываем его
                            }
                        }
                        catch
                        {
                            writer.WriteLine("404");
                            writer.Flush();
                        }
                    }
                    break;
                case "2": //save a file
                    userFileName = reader.ReadLine();
                    string userFilePath = Path.Combine(filePathClient, userFileName);
                    serverFileName = reader.ReadLine();

                    serverFilePath = Path.Combine(filePathServer, serverFileName);
                    if (serverFileName == "" || serverFileName == "\n")
                    {
                        serverFilePath = Path.Combine(filePathServer, userFileName);

                    }


                    if (File.Exists(userFilePath))
                    {
                        File.Move(userFilePath, serverFilePath);
                        string name = userFileName;
                        if (!(serverFileName == ""))
                        {
                            File.Move(serverFilePath, Path.Combine(filePathServer, serverFileName));
                            name = serverFileName;
                        }
                        int id = serverFiles.LastOrDefault().Key + 1;
                        Console.WriteLine($"serverFiles Last Key = {serverFiles.LastOrDefault().Key}");
                        if (serverFiles.LastOrDefault().Key == 0) { id = 1; }
                        serverFiles.Add(id, serverFileName);
                        //запись новой пары id-имя в файл-базу данных
                        using (StreamWriter sw = new StreamWriter(filePathFilesDB, true)) // true указывает на необходимость добавления данных в конец файла
                        {
                            sw.WriteLine($"{id}|{serverFileName}");
                        }
                        writer.WriteLine("200");
                        writer.Flush();
                        writer.WriteLine(id);
                        writer.Flush();

                    }
                    else
                    {
                        writer.WriteLine("404");
                        writer.Flush();

                    }

                    break;
                case "3": //delete
                    string nameorIdDel = reader.ReadLine(); //считываем команду по имени либо айди

                    if (nameorIdDel == "1")// By Name
                    {
                        serverFileName = reader.ReadLine();
                    }
                    else if (nameorIdDel == "2")// By ID
                    {
                        string serverFileId = reader.ReadLine();
                        serverFileName = GetFileNameById(int.Parse(serverFileId), serverFiles);
                        if (serverFileName == "")//значит файла нет на сервере
                        {
                            writer.WriteLine("404");
                            writer.Flush();
                            break;
                        }
                    }
                    serverFilePath = Path.Combine(filePathServer, serverFileName); //папка с файлом из сервера

                    if (!File.Exists(serverFilePath))
                    {
                        writer.WriteLine("404");
                        writer.Flush();
                    }
                    else
                    {
                        try
                        {
                            File.Delete(serverFilePath);
                            // перезаписываем файл с айди-именем тк мы файл удалили 
                            using (StreamWriter sw = new StreamWriter(File.Open(filePathFilesDB, FileMode.Create)))
                            {
                                foreach (KeyValuePair<int, string> kvp in serverFiles)
                                {
                                    // запись пары ключ-значение, разделенной символом '|'
                                    sw.WriteLine($"{kvp.Key}|{kvp.Value}");
                                }
                            }
                            writer.WriteLine("200");
                            writer.Flush();
                        }
                        catch
                        {
                            writer.WriteLine("404");
                            writer.Flush();
                        }
                    }
                    break;
                case "exit":
                    tcpClient.Close();
                    stream.Close();
                    reader.Close();
                    writer.Close();
                    tcpListener.Stop();
                    return;
                    break;

            }
        }
    }

    //метод добычи имени файла по айдишнику из текстового файла сервера
    static string GetFileNameById(int fileId, SortedDictionary<int, string> serverFile)
    {
        foreach (var line in serverFile)
        {
            if (line.Key == fileId)
            {
                return line.Value;
            }
        }
        return "";

    }

    //метод заполнения словаря
    static void ReadAllServerFiles(string filePathFilesDB, SortedDictionary<int, string> serverFiles)
    {
        if (!File.Exists(filePathFilesDB))
        {
            Console.WriteLine($"Нету filePathFilesDB {filePathFilesDB}");
            File.Create(filePathFilesDB);
        }
        var lines = File.ReadAllLines(filePathFilesDB);//массив строк 

        foreach (var line in lines)
        {
            var parts = line.Split('|');
            serverFiles.Add(int.Parse(parts[0]), parts[1]);
            Console.WriteLine($"{line} , {parts[0]}, {parts[1]}");
        }
    }
}
